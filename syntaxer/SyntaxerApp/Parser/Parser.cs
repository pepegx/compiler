using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using O_Lexer;
using O_Parser.AST;

namespace O_Parser
{
    public sealed class Parser
    {
        private readonly List<Token> _toks;
        private int _pos;
        private Token Cur => _pos < _toks.Count ? _toks[_pos] : _toks[^1];
        private TokenType CurT => Cur.Type;
        private Token LA(int k) => (_pos + k < _toks.Count) ? _toks[_pos + k] : _toks[^1];
        private bool AtEnd => CurT == TokenType.EOF;

        public Parser(Lexer lexer)
        {
            _toks = lexer.Tokenize().ToList();
            _pos = 0;
        }

        public ProgramNode ParseProgram()
        {
            var prog = new ProgramNode();
            while (!AtEnd)
            {
                prog.Classes.Add(ParseClassDecl());
            }
            return prog;
        }

        private ClassDeclNode ParseClassDecl()
        {
            Expect(TokenType.Class);
            var nameTok = Expect(TokenType.Identifier);
            string? baseName = null;
            if (Accept(TokenType.Extends))
            {
                baseName = Expect(TokenType.Identifier).Value;
            }
            Expect(TokenType.Is);
            var cls = new ClassDeclNode(nameTok.Value, baseName);
            while (!Check(TokenType.End))
            {
                cls.Members.Add(ParseMemberDecl());
            }
            Expect(TokenType.End);
            return cls;
        }

        private MemberDeclNode ParseMemberDecl()
        {
            if (Check(TokenType.Var))
            {
                return ParseVarDecl();
            }
            if (Check(TokenType.Method))
            {
                return ParseMethodDecl();
            }
            if (Check(TokenType.This))
            {
                return ParseCtorDecl();
            }
            throw Error($"[member] expected 'var' | 'method' | 'this', got {Cur}");
        }

        private VarDeclNode ParseVarDecl()
        {
            Expect(TokenType.Var);
            var name = Expect(TokenType.Identifier).Value;
            Expect(TokenType.Colon);
            var init = ParseExpression();
            return new VarDeclNode(name, init);
        }

        private MethodDeclNode ParseMethodDecl()
        {
            Expect(TokenType.Method);
            var name = Expect(TokenType.Identifier).Value;
            var pars = ParseOptParameters();
            string? ret = null;
            if (Accept(TokenType.Colon))
            {
                ret = ParseTypeName();
            }

            if (Accept(TokenType.Is))
            {
                var body = ParseBody(stopOnElse:false);
                Expect(TokenType.End);
                return new MethodDeclNode(name, pars, ret, body);
            }
            if (Accept(TokenType.Arrow))
            {
                var expr = ParseExpression();
                return new MethodDeclNode(name, pars, ret, expr);
            }
            return new MethodDeclNode(name, pars, ret, body: null);
        }

        private CtorDeclNode ParseCtorDecl()
        {
            Expect(TokenType.This);
            var pars = ParseOptParameters();
            Expect(TokenType.Is);
            var body = ParseBody(stopOnElse:false);
            Expect(TokenType.End);
            return new CtorDeclNode(pars, body);
        }

        private List<ParameterNode> ParseOptParameters()
        {
            var res = new List<ParameterNode>();
            if (!Accept(TokenType.LParen)) return res;
            if (Accept(TokenType.RParen)) return res;
            while (true)
            {
                var pname = Expect(TokenType.Identifier).Value;
                Expect(TokenType.Colon);
                var ptype = ParseTypeName();
                res.Add(new ParameterNode(pname, ptype));
                if (Accept(TokenType.Comma)) continue;
                Expect(TokenType.RParen);
                break;
            }
            return res;
        }

        // TypeName := Identifier ('[' TypeName (',' TypeName)* ']')*
        private string ParseTypeName()
        {
            var name = Expect(TokenType.Identifier).Value;
            while (Accept(TokenType.LBracket))
            {
                var args = new List<string>();
                args.Add(ParseTypeName());
                while (Accept(TokenType.Comma))
                    args.Add(ParseTypeName());
                Expect(TokenType.RBracket);
                name = $"{name}[{string.Join(",", args)}]";
            }
            return name;
        }

        private BlockNode ParseBody(bool stopOnElse)
        {
            var block = new BlockNode();
            while (true)
            {
                if (Check(TokenType.End) || AtEnd) break;
                if (stopOnElse && Check(TokenType.Else)) break;

                if (Check(TokenType.Var))
                {
                    block.Locals.Add(ParseVarDecl());
                }
                else
                {
                    block.Statements.Add(ParseStatement());
                }
            }
            return block;
        }

        private StmtNode ParseStatement()
        {
            if (Check(TokenType.While))  return ParseWhile();
            if (Check(TokenType.If))     return ParseIf();
            if (Check(TokenType.Return)) return ParseReturn();

            if (Check(TokenType.Identifier) && LA(1).Type == TokenType.Assign)
            {
                var name = Expect(TokenType.Identifier).Value;
                Expect(TokenType.Assign);
                var expr = ParseExpression();
                return new AssignStmt(name, expr);
            }

            var e = ParseExpression();
            return new ExprStmt(e);
        }

        private WhileStmt ParseWhile()
        {
            Expect(TokenType.While);
            var cond = ParseExpression();
            Expect(TokenType.Loop);
            var body = ParseBody(stopOnElse:false);
            Expect(TokenType.End);
            return new WhileStmt(cond, body);
        }

        private IfStmt ParseIf()
        {
            Expect(TokenType.If);
            var cond = ParseExpression();
            Expect(TokenType.Then);
            var thenBody = ParseBody(stopOnElse:true);
            BlockNode? elseBody = null;
            if (Accept(TokenType.Else))
            {
                elseBody = ParseBody(stopOnElse:false);
            }
            Expect(TokenType.End);
            return new IfStmt(cond, thenBody, elseBody);
        }

        private ReturnStmt ParseReturn()
        {
            Expect(TokenType.Return);
            if (Check(TokenType.End) || Check(TokenType.Else) || Check(TokenType.Loop) || AtEnd)
                return new ReturnStmt(null);
            var expr = ParseExpression();
            return new ReturnStmt(expr);
        }

        private ExprNode ParseExpression()
        {
            var expr = ParseStartAtom();

            while (true)
            {
                if (Accept(TokenType.LParen))
                {
                    var args = ParseArgumentsAfterLParen();
                    expr = new CallExpr(expr, args);
                    continue;
                }
                if (Accept(TokenType.Dot))
                {
                    var member = Expect(TokenType.Identifier).Value;
                    expr = new MemberAccessExpr(expr, member);
                    continue;
                }
                break;
            }
            return expr;
        }

        private ExprNode ParseStartAtom()
        {
            if (Accept(TokenType.True))  return new BoolLiteral(true);
            if (Accept(TokenType.False)) return new BoolLiteral(false);
            if (Check(TokenType.IntegerLiteral))
            {
                var t = Expect(TokenType.IntegerLiteral);
                long v = long.Parse(t.Value, CultureInfo.InvariantCulture);
                return new IntLiteral(v);
            }
            if (Check(TokenType.RealLiteral))
            {
                var t = Expect(TokenType.RealLiteral);
                double v = double.Parse(t.Value, CultureInfo.InvariantCulture);
                return new RealLiteral(v);
            }
            if (Accept(TokenType.This)) return ThisExpr.Instance;

            if (Check(TokenType.Identifier))
            {
                var name = Expect(TokenType.Identifier).Value;
                if (Accept(TokenType.LParen))
                {
                    var args = ParseArgumentsAfterLParen();
                    return new NewExpr(name, args);
                }
                return new IdentifierExpr(name);
            }

            throw Error($"invalid expression start: {Cur}");
        }

        private List<ExprNode> ParseArgumentsAfterLParen()
        {
            var args = new List<ExprNode>();
            if (Accept(TokenType.RParen)) return args;
            while (true)
            {
                args.Add(ParseExpression());
                if (Accept(TokenType.Comma)) continue;
                Expect(TokenType.RParen);
                break;
            }
            return args;
        }

        private Token Expect(TokenType type)
        {
            if (CurT != type)
                throw Error($"expected {type}, got {Cur}");
            var t = Cur; _pos++; return t;
        }
        private bool Accept(TokenType type)
        {
            if (CurT == type) { _pos++; return true; }
            return false;
        }
        private bool Check(TokenType type) => CurT == type;

        private Exception Error(string message) => new SyntaxError($"[pos {_pos}] {message}");
    }
}
