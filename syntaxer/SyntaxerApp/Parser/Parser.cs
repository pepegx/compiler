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
            while (!Check(TokenType.End) && !AtEnd)
            {
                // CRITICAL: If we encounter a new class declaration, we're done parsing this class
                // This handles cases where multiple classes are in the same file
                // This is similar to M&m's approach - they check for Class token and throw an error
                // But in our case, we should stop parsing this class and let ParseProgram handle the next class
                if (Check(TokenType.Class))
                {
                    // We've reached the next class - stop parsing this one
                    // This means the previous class is missing an 'end', but we'll handle it gracefully
                    // Don't consume the Class token - let ParseProgram handle it
                    break;
                }
                cls.Members.Add(ParseMemberDecl());
            }
            
            // CRITICAL: Only expect End if we didn't break due to encountering a new class
            // If we broke due to Class token, the next ParseClassDecl() will consume it
            // This is important - we must NOT consume the Class token here
            if (!Check(TokenType.Class) && !AtEnd)
            {
                Expect(TokenType.End);
            }
            return cls;
        }

        private MemberDeclNode ParseMemberDecl()
        {
            // CRITICAL: If we encounter a new class declaration, we're done parsing members
            // This handles cases where multiple classes are in the same file
            if (Check(TokenType.Class))
            {
                // This should not happen in normal flow, but if it does, it means we've reached the next class
                // Return null or throw - actually, ParseClassDecl should handle this, so this is defensive
                throw Error($"[member] unexpected class declaration, expected 'var' | 'method' | 'this' | 'end'");
            }
            
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
            // Allow keywords as variable names (e.g., "var loop : LoopTest()")
            // This handles cases where keywords are used as identifiers
            string name;
            if (Check(TokenType.Identifier))
            {
                name = Expect(TokenType.Identifier).Value;
            }
            else if (Check(TokenType.Loop) || Check(TokenType.While) || Check(TokenType.If) || 
                     Check(TokenType.Then) || Check(TokenType.Else) || Check(TokenType.End) ||
                     Check(TokenType.Class) || Check(TokenType.Var) || Check(TokenType.Method) ||
                     Check(TokenType.This) || Check(TokenType.Return) || Check(TokenType.Is) ||
                     Check(TokenType.Extends) || Check(TokenType.True) || Check(TokenType.False))
            {
                // Treat keyword as identifier for variable name
                name = Cur.Value;
                _pos++; // Consume the token
            }
            else
            {
                throw Error($"expected Identifier or keyword, got {Cur}");
            }
            Expect(TokenType.Colon);
            // In O language, after colon can be either:
            // 1. TypeName(args) - constructor call
            // 2. Expression - any expression
            // Always parse as expression - it will handle both cases
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
                // Allow keywords as parameter names (like M&m does)
                // This handles cases like "method innerLoop(start: Integer, end: Integer)"
                string pname;
                if (Check(TokenType.Identifier))
                {
                    pname = Expect(TokenType.Identifier).Value;
                }
                else if (Check(TokenType.Loop) || Check(TokenType.While) || Check(TokenType.If) || 
                         Check(TokenType.Then) || Check(TokenType.Else) || Check(TokenType.End) ||
                         Check(TokenType.Class) || Check(TokenType.Var) || Check(TokenType.Method) ||
                         Check(TokenType.This) || Check(TokenType.Return) || Check(TokenType.Is) ||
                         Check(TokenType.Extends) || Check(TokenType.True) || Check(TokenType.False))
                {
                    // Treat keyword as identifier for parameter name
                    pname = Cur.Value;
                    _pos++; // Consume the token
                }
                else
                {
                    throw Error($"expected Identifier or keyword for parameter name, got {Cur}");
                }
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
                // CRITICAL: If we encounter a new class declaration, we're done parsing this body
                // This handles cases where multiple classes are in the same file
                if (Check(TokenType.Class)) break;

                if (Check(TokenType.Var))
                {
                    var varDecl = ParseVarDecl();
                    block.Locals.Add(varDecl);
                    block.Body.Add(varDecl);  // Сохраняем порядок
                }
                else if (Check(TokenType.While) || Check(TokenType.If) || Check(TokenType.Return))
                {
                    // Valid statement starters
                    var stmt = ParseStatement();
                    block.Statements.Add(stmt);
                    block.Body.Add(stmt);  // Сохраняем порядок
                }
                else if ((Check(TokenType.Identifier) && LA(1).Type == TokenType.Assign) ||
                         ((Check(TokenType.Loop) || Check(TokenType.While) || Check(TokenType.If) || 
                          Check(TokenType.Then) || Check(TokenType.Else) || Check(TokenType.End) ||
                          Check(TokenType.Class) || Check(TokenType.Var) || Check(TokenType.Method) ||
                          Check(TokenType.This) || Check(TokenType.Return) || Check(TokenType.Is) ||
                          Check(TokenType.Extends) || Check(TokenType.True) || Check(TokenType.False)) 
                          && LA(1).Type == TokenType.Assign))
                {
                    // Assignment statement (identifier or keyword used as identifier)
                    var stmt = ParseStatement();
                    block.Statements.Add(stmt);
                    block.Body.Add(stmt);  // Сохраняем порядок
                }
                else if (Check(TokenType.Identifier) || Check(TokenType.True) || Check(TokenType.False) ||
                         Check(TokenType.IntegerLiteral) || Check(TokenType.RealLiteral) || Check(TokenType.StringLiteral) || Check(TokenType.This) ||
                         Check(TokenType.Loop) || Check(TokenType.While) || Check(TokenType.If) || 
                         Check(TokenType.Then) || Check(TokenType.Else) || Check(TokenType.End) ||
                         Check(TokenType.Class) || Check(TokenType.Var) || Check(TokenType.Method) ||
                         Check(TokenType.Return) || Check(TokenType.Is) || Check(TokenType.Extends))
                {
                    // Expression statement - parse as expression statement
                    // This matches M&m approach - ParseStatement() always parses expression at the end
                    // Also allow keywords as identifiers in expressions
                    var stmt = ParseStatement();
                    block.Statements.Add(stmt);
                    block.Body.Add(stmt);  // Сохраняем порядок
                }
                else
                {
                    // Not a valid statement or variable declaration - we're done
                    // This handles cases where we've reached the end of the body
                    break;
                }
            }
            return block;
        }

        private StmtNode ParseStatement()
        {
            if (Check(TokenType.While))  return ParseWhile();
            if (Check(TokenType.If))     return ParseIf();
            if (Check(TokenType.Return)) return ParseReturn();

            // Check for assignment: identifier/keyword followed by :=
            // Also support this.field := value syntax
            bool isAssign = false;
            string? assignName = null;
            
            // Check for this.field := value
            if (Check(TokenType.This) && LA(1).Type == TokenType.Dot && LA(2).Type == TokenType.Identifier && LA(3).Type == TokenType.Assign)
            {
                isAssign = true;
                Expect(TokenType.This);
                Expect(TokenType.Dot);
                assignName = Expect(TokenType.Identifier).Value;
            }
            else if (Check(TokenType.Identifier) && LA(1).Type == TokenType.Assign)
            {
                isAssign = true;
                assignName = Expect(TokenType.Identifier).Value;
            }
            else if ((Check(TokenType.Loop) || Check(TokenType.While) || Check(TokenType.If) || 
                     Check(TokenType.Then) || Check(TokenType.Else) || Check(TokenType.End) ||
                     Check(TokenType.Class) || Check(TokenType.Var) || Check(TokenType.Method) ||
                     Check(TokenType.This) || Check(TokenType.Return) || Check(TokenType.Is) ||
                     Check(TokenType.Extends) || Check(TokenType.True) || Check(TokenType.False)) 
                     && LA(1).Type == TokenType.Assign)
            {
                // Keyword used as identifier in assignment
                isAssign = true;
                assignName = Cur.Value;
                _pos++; // Consume the keyword token
            }
            
            if (isAssign)
            {
                Expect(TokenType.Assign);
                var expr = ParseExpression();
                return new AssignStmt(assignName, expr);
            }

            // Only parse as expression if current token can start an expression
            // Dot cannot start an expression - it should be handled in ParseExpression's while loop
            if (Check(TokenType.Dot))
            {
                throw Error($"unexpected Dot - expressions cannot start with Dot: {Cur}");
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

            // Use M&m approach exactly: check for continuation tokens (LParen, Dot)
            // If none match, break naturally. This handles termination on keywords like 'loop', 'then', etc.
            while (true)
            {
                // Check for tokens that terminate expressions BEFORE checking for continuation tokens
                // This prevents entering Dot/LParen blocks when we should stop
                if (Check(TokenType.Loop) || Check(TokenType.Then) || Check(TokenType.End) || 
                    Check(TokenType.Else) || Check(TokenType.Class) || Check(TokenType.While) || 
                    Check(TokenType.If) || Check(TokenType.Return) || Check(TokenType.Var) ||
                    Check(TokenType.RParen) || Check(TokenType.Comma) || Check(TokenType.RBracket))
                {
                    break;
                }
                
                if (Check(TokenType.LParen))
                {
                    Accept(TokenType.LParen);
                    var args = ParseArgumentsAfterLParen();
                    
                    // If expr is an identifier, check if it's a built-in type constructor
                    // Built-in types: Integer, Real, Boolean, String, Array[...]
                    // If it's not a built-in type, treat as method call on 'this'
                    if (expr is IdentifierExpr idExpr)
                    {
                        var name = idExpr.Name;
                        // Check if it's a built-in type constructor
                        if (name == "Integer" || name == "Real" || name == "Boolean" || name == "String" || name.StartsWith("Array[") || name.StartsWith("List["))
                        {
                            expr = new NewExpr(name, args);
                        }
                        else
                        {
                            // Method call on 'this' - create implicit this.method() call
                            expr = new CallExpr(new MemberAccessExpr(ThisExpr.Instance, name), args);
                        }
                    }
                    else
                    {
                        // Method call on an expression
                        expr = new CallExpr(expr, args);
                    }
                    // CRITICAL: After parsing method/constructor call, check for terminator tokens BEFORE continuing the loop
                    // This is essential to stop parsing when we encounter keywords like 'loop', 'then', 'end', etc.
                    // This matches M&m approach - they check for terminators after each expression part
                    if (Check(TokenType.Loop) || Check(TokenType.Then) || Check(TokenType.End) || 
                        Check(TokenType.Else) || Check(TokenType.Class) || Check(TokenType.While) || 
                        Check(TokenType.If) || Check(TokenType.Return) || Check(TokenType.Var) ||
                        Check(TokenType.RParen) || Check(TokenType.Comma) || Check(TokenType.RBracket))
                    {
                        // We've encountered a terminator - stop parsing the expression
                        break;
                    }
                    // If no terminator, continue the loop to check for more Dot/LParen
                }
                else if (Check(TokenType.Dot))
                {
                    // CRITICAL: Check lookahead BEFORE accepting Dot (like M&m does)
                    // If the token after Dot is not Identifier (e.g., it's Loop, Then, End, etc.), 
                    // we should stop parsing the expression, not accept the Dot
                    var lookahead = LA(1);
                    if (lookahead.Type != TokenType.Identifier)
                    {
                        // Token after Dot is not Identifier - we're done parsing the expression
                        // Don't accept the Dot, just break
                        break;
                    }
                    
                    Accept(TokenType.Dot);
                    var memberName = Expect(TokenType.Identifier).Value;
                    
                    // Check if it's a method call
                    if (Check(TokenType.LParen))
                    {
                        Accept(TokenType.LParen);
                        var args = new List<ExprNode>();
                        if (!Check(TokenType.RParen))
                        {
                            do
                            {
                                args.Add(ParseExpression());
                            } while (Accept(TokenType.Comma));
                        }
                        Expect(TokenType.RParen);
                        // In O language, method calls are: expr.member(args)
                        expr = new CallExpr(new MemberAccessExpr(expr, memberName), args);
                        // CRITICAL: After parsing method call, check for terminator tokens BEFORE continuing the loop
                        // This is essential to stop parsing when we encounter keywords like 'loop', 'then', 'end', etc.
                        // This matches M&m approach - they check for terminators after each expression part
                        if (Check(TokenType.Loop) || Check(TokenType.Then) || Check(TokenType.End) || 
                            Check(TokenType.Else) || Check(TokenType.Class) || Check(TokenType.While) || 
                            Check(TokenType.If) || Check(TokenType.Return) || Check(TokenType.Var) ||
                            Check(TokenType.RParen) || Check(TokenType.Comma) || Check(TokenType.RBracket))
                        {
                            // We've encountered a terminator - stop parsing the expression
                            break;
                        }
                        // If no terminator, continue the loop to check for more Dot/LParen
                    }
                    else
                    {
                        // Just member access, no method call
                        expr = new MemberAccessExpr(expr, memberName);
                    }
                }
                else
                {
                    // M&m approach: if token is not LParen or Dot, break
                    // This naturally handles termination on keywords like 'loop', 'then', 'end', etc.
                    break;
                }
            }
            return expr;
        }

        private ExprNode ParseStartAtom()
        {
            // If current token is Dot, it means we're trying to parse an expression that starts with Dot
            // This shouldn't happen - Dot should be handled in ParseExpression's while loop
            // But if it does, it's likely a parsing error - maybe we're in the wrong context
            if (Check(TokenType.Dot))
            {
                // This is an error - Dot cannot start an expression
                // But let's check if we're in a context where this might be valid
                // Actually, this should never happen, so throw an error
                throw Error($"unexpected Dot at expression start: {Cur}");
            }
            
            // CRITICAL: Handle unary minus for negative numbers (e.g., Integer(-1))
            // Check if current token is Unknown with value "-" and next token is a number
            if (Check(TokenType.Unknown) && Cur.Value == "-")
            {
                // Look ahead to see if next token is a number
                if (LA(1).Type == TokenType.IntegerLiteral)
                {
                    Expect(TokenType.Unknown); // Consume the "-"
                    var t = Expect(TokenType.IntegerLiteral);
                    long v = long.Parse(t.Value, CultureInfo.InvariantCulture);
                    return new IntLiteral(-v); // Return negative integer
                }
                if (LA(1).Type == TokenType.RealLiteral)
                {
                    Expect(TokenType.Unknown); // Consume the "-"
                    var t = Expect(TokenType.RealLiteral);
                    double v = double.Parse(t.Value, CultureInfo.InvariantCulture);
                    return new RealLiteral(-v); // Return negative real
                }
            }
            
            if (Accept(TokenType.True))  return new BoolLiteral(true);
            if (Accept(TokenType.False)) return new BoolLiteral(false);
            if (Check(TokenType.StringLiteral))
            {
                var t = Expect(TokenType.StringLiteral);
                return new StringLiteral(t.Value);
            }
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

            // Allow keywords as identifiers in expressions (e.g., "loop.sumToN(...)")
            // This handles cases where keywords are used as variable names
            string name;
            if (Check(TokenType.Identifier))
            {
                name = Expect(TokenType.Identifier).Value;
            }
            else if (Check(TokenType.Loop) || Check(TokenType.While) || Check(TokenType.If) || 
                     Check(TokenType.Then) || Check(TokenType.Else) || Check(TokenType.End) ||
                     Check(TokenType.Class) || Check(TokenType.Var) || Check(TokenType.Method) ||
                     Check(TokenType.This) || Check(TokenType.Return) || Check(TokenType.Is) ||
                     Check(TokenType.Extends) || Check(TokenType.True) || Check(TokenType.False))
            {
                // Treat keyword as identifier for expression
                name = Cur.Value;
                _pos++; // Consume the token
            }
            else
            {
                throw Error($"invalid expression start: {Cur}");
            }
            
            // CRITICAL: Handle generic types like Array[Integer]
            // Check if next token is LBracket (for generic type syntax)
            string fullTypeName = name;
            if (Check(TokenType.LBracket))
            {
                // Parse generic type parameters: Array[Integer] or Array[Integer, Real]
                Accept(TokenType.LBracket);
                var typeArgs = new List<string>();
                typeArgs.Add(ParseTypeName()); // Parse first type argument
                while (Accept(TokenType.Comma))
                {
                    typeArgs.Add(ParseTypeName()); // Parse additional type arguments
                }
                Expect(TokenType.RBracket);
                fullTypeName = $"{name}[{string.Join(",", typeArgs)}]";
            }
            
            if (Accept(TokenType.LParen))
            {
                var args = ParseArgumentsAfterLParen();
                // Check if it's a built-in type constructor
                // Built-in types: Integer, Real, Boolean, String, Array[...], List[...]
                // If it's not a built-in type, treat as method call on 'this'
                if (fullTypeName == "Integer" || fullTypeName == "Real" || fullTypeName == "Boolean" || fullTypeName == "String" || fullTypeName.StartsWith("Array[") || fullTypeName.StartsWith("List["))
                {
                    return new NewExpr(fullTypeName, args);
                }
                else
                {
                    // Method call on 'this' - create implicit this.method() call
                    return new CallExpr(new MemberAccessExpr(ThisExpr.Instance, fullTypeName), args);
                }
            }
            return new IdentifierExpr(fullTypeName);
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
