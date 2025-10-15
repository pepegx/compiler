using System;
using System.Globalization;
using System.Linq;
using O_Parser.AST;

namespace O_Parser.Utilities
{
    public static class AstTreePrinter
    {
        public static void Print(ProgramNode ast)
        {
            Console.WriteLine("Program");
            for (int i = 0; i < ast.Classes.Count; i++)
                PrintNode(ast.Classes[i], "", i == ast.Classes.Count - 1);
        }

        static void Emit(string indent, bool isLast, string text)
        {
            Console.Write(indent);
            Console.Write(isLast ? "└─ " : "├─ ");
            Console.WriteLine(text);
        }
        static string ChildIndent(string indent, bool isLast)
            => indent + (isLast ? "   " : "│  ");

        static void PrintNode(Node n, string indent, bool isLast)
        {
            switch (n)
            {
                case ClassDeclNode c:
                {
                    Emit(indent, isLast, $"Class {c.Name}" + (c.BaseName != null ? $" : {c.BaseName}" : ""));
                    var ci = ChildIndent(indent, isLast);
                    for (int i = 0; i < c.Members.Count; i++)
                        PrintNode(c.Members[i], ci, i == c.Members.Count - 1);
                    break;
                }

                case VarDeclNode v:
                {
                    Emit(indent, isLast, $"Var {v.Name}");
                    var ci = ChildIndent(indent, isLast);
                    Emit(ci, true, "Initializer");
                    var ci2 = ChildIndent(ci, true);
                    PrintExpr(v.Initializer, ci2, true);
                    break;
                }

                case MethodDeclNode m:
                {
                    var pars = string.Join(", ", m.Parameters.Select(p => $"{p.Name}:{p.TypeName}"));
                    var head = $"Method {m.Name}({pars})" + (m.ReturnType != null ? $" : {m.ReturnType}" : "");
                    Emit(indent, isLast, head);
                    var ci = ChildIndent(indent, isLast);

                    if (m.IsArrowBody && m.ArrowExpr != null)
                    {
                        Emit(ci, true, "ArrowBody");
                        var ci2 = ChildIndent(ci, true);
                        PrintExpr(m.ArrowExpr, ci2, true);
                    }
                    else if (m.Body == null)
                    {
                        Emit(ci, true, "[forward]");
                    }
                    else
                    {
                        PrintBlock(m.Body, ci, last: true);
                    }
                    break;
                }

                case CtorDeclNode ctor:
                {
                    var pars = string.Join(", ", ctor.Parameters.Select(p => $"{p.Name}:{p.TypeName}"));
                    Emit(indent, isLast, $"Ctor this({pars})");
                    var ci = ChildIndent(indent, isLast);
                    PrintBlock(ctor.Body, ci, last: true);
                    break;
                }

                case WhileStmt w:
                {
                    Emit(indent, isLast, "While");
                    var ci = ChildIndent(indent, isLast);

                    Emit(ci, false, "Condition");
                    var ciCond = ChildIndent(ci, false);
                    PrintExpr(w.Condition, ciCond, true);

                    Emit(ci, true, "Body");
                    var ciBody = ChildIndent(ci, true);
                    PrintBlock(w.Body, ciBody, last: true);
                    break;
                }

                case IfStmt i:
                {
                    Emit(indent, isLast, "If");
                    var ci = ChildIndent(indent, isLast);

                    Emit(ci, false, "Condition");
                    var ciCond = ChildIndent(ci, false);
                    PrintExpr(i.Condition, ciCond, true);

                    bool hasElse = i.ElseBody != null;
                    Emit(ci, !hasElse, "Then");
                    var ciThen = ChildIndent(ci, !hasElse);
                    PrintBlock(i.ThenBody, ciThen, last: true);

                    if (hasElse)
                    {
                        Emit(ci, true, "Else");
                        var ciElse = ChildIndent(ci, true);
                        PrintBlock(i.ElseBody!, ciElse, last: true);
                    }
                    break;
                }

                case ReturnStmt r:
                {
                    Emit(indent, isLast, "Return");
                    if (r.Value != null)
                    {
                        var ci = ChildIndent(indent, isLast);
                        PrintExpr(r.Value, ci, true);
                    }
                    break;
                }

                case AssignStmt a:
                {
                    Emit(indent, isLast, $"Assign {a.TargetName}");
                    var ci = ChildIndent(indent, isLast);
                    PrintExpr(a.Value, ci, true);
                    break;
                }

                case ExprStmt es:
                {
                    Emit(indent, isLast, "Expr");
                    var ci = ChildIndent(indent, isLast);
                    PrintExpr(es.Expr, ci, true);
                    break;
                }

                case BlockNode b:
                {
                    PrintBlock(b, indent, isLast);
                    break;
                }

                default:
                    Emit(indent, isLast, n.GetType().Name);
                    break;
            }
        }

        static void PrintBlock(BlockNode b, string indent, bool last)
        {
            Emit(indent, last, "Block");
            var ci = ChildIndent(indent, last);

            Emit(ci, false, "Locals");
            var ciLoc = ChildIndent(ci, false);
            if (b.Locals.Count == 0) Emit(ciLoc, true, "[none]");
            else
                for (int i = 0; i < b.Locals.Count; i++)
                    PrintNode(b.Locals[i], ciLoc, i == b.Locals.Count - 1);

            Emit(ci, true, "Statements");
            var ciStmt = ChildIndent(ci, true);
            if (b.Statements.Count == 0) Emit(ciStmt, true, "[none]");
            else
                for (int i = 0; i < b.Statements.Count; i++)
                    PrintNode(b.Statements[i], ciStmt, i == b.Statements.Count - 1);
        }

        static void PrintExpr(ExprNode e, string indent, bool isLast)
        {
            switch (e)
            {
                case ThisExpr:
                    Emit(indent, isLast, "This");
                    break;

                case BoolLiteral b:
                    Emit(indent, isLast, $"Bool({(b.Value ? "true" : "false")})");
                    break;

                case IntLiteral i:
                    Emit(indent, isLast, $"Int({i.Value.ToString(CultureInfo.InvariantCulture)})");
                    break;

                case RealLiteral r:
                    Emit(indent, isLast, $"Real({r.Value.ToString(CultureInfo.InvariantCulture)})");
                    break;

                case IdentifierExpr id:
                    Emit(indent, isLast, $"Identifier: {id.Name}");
                    break;

                case MemberAccessExpr ma:
                {
                    Emit(indent, isLast, $"MemberAccess: .{ma.Member}");
                    var ci = ChildIndent(indent, isLast);
                    Emit(ci, true, "Target");
                    var ciT = ChildIndent(ci, true);
                    PrintExpr(ma.Target, ciT, true);
                    break;
                }

                case CallExpr call:
                {
                    Emit(indent, isLast, $"Call: {call.Arguments.Count} args");
                    var ci = ChildIndent(indent, isLast);
                    Emit(ci, call.Arguments.Count == 0, "Callee");
                    var ciCallee = ChildIndent(ci, call.Arguments.Count == 0);
                    PrintExpr(call.Callee, ciCallee, true);

                    for (int i = 0; i < call.Arguments.Count; i++)
                    {
                        bool argLast = (i == call.Arguments.Count - 1);
                        Emit(ci, argLast, $"Arg {i}");
                        var ciArg = ChildIndent(ci, argLast);
                        PrintExpr(call.Arguments[i], ciArg, true);
                    }
                    break;
                }

                case NewExpr nw:
                {
                    Emit(indent, isLast, $"New: {nw.ClassName}");
                    var ci = ChildIndent(indent, isLast);
                    if (nw.Arguments.Count == 0)
                    {
                        Emit(ci, true, "[no args]");
                    }
                    else
                    {
                        for (int i = 0; i < nw.Arguments.Count; i++)
                        {
                            bool argLast = (i == nw.Arguments.Count - 1);
                            Emit(ci, argLast, $"Arg {i}");
                            var ciArg = ChildIndent(ci, argLast);
                            PrintExpr(nw.Arguments[i], ciArg, true);
                        }
                    }
                    break;
                }

                default:
                    Emit(indent, isLast, e.GetType().Name);
                    break;
            }
        }
    }
}
