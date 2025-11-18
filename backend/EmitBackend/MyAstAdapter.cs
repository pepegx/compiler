using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace EmitBackend
{
    internal static class R
    {
        public static object P(object o, params string[] names)
        {
            var t = o.GetType();
            foreach (var n in names)
            {
                var pi = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (pi != null) return pi.GetValue(o, null);
                var fi = t.GetField(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (fi != null) return fi.GetValue(o);
            }
            return null;
        }

        public static string S(object o)
        {
            if (o == null) return null;
            if (o is string s) return s;
            var text = P(o, "Text", "Name", "Id", "TypeName");
            return text == null ? null : text.ToString();
        }

        public static IEnumerable<object> AsEnum(object o)
        {
            var e = o as System.Collections.IEnumerable;
            if (e == null) return new object[0];
            var list = new List<object>();
            foreach (var it in e) list.Add(it);
            return list;
        }

        public static bool ContainsIgnoreCase(string haystack, string needle)
        {
            if (haystack == null || needle == null) return false;
            return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public sealed class MyAstAdapter : IProgramNode
    {
        private readonly object _root;
        public MyAstAdapter(object rootAst) { _root = rootAst; }

        public IEnumerable<IClassDecl> Classes
        {
            get
            {
                var classes = R.P(_root, "Classes", "ClassList", "Decls", "Declarations");
                if (classes == null) throw new InvalidOperationException("Root has no Classes.");
                foreach (var c in R.AsEnum(classes))
                    yield return new ClassA(c);
            }
        }

        private sealed class ClassA : IClassDecl
        {
            private readonly object _c;
            public ClassA(object c) { _c = c; }

            public string Name
            {
                get
                {
                    var s = R.S(R.P(_c, "Name"));
                    if (string.IsNullOrWhiteSpace(s)) throw new InvalidOperationException("Class.Name missing");
                    return s;
                }
            }

            public string BaseName
            {
                get
                {
                    var b = R.P(_c, "BaseName", "Extends", "Parent", "Super");
                    var s = R.S(b);
                    return string.IsNullOrWhiteSpace(s) ? null : s;
                }
            }

            public IEnumerable<IVariableDecl> Fields
            {
                get
                {
                    var explicitFields = R.P(_c, "Fields", "FieldList");
                    if (explicitFields != null)
                    {
                        foreach (var f in R.AsEnum(explicitFields))
                            yield return new FieldA(f);
                        yield break;
                    }

                    foreach (var m in GetMembers())
                    {
                        var tn = m.GetType().Name ?? string.Empty;
                        var kind = R.S(R.P(m, "Kind", "DeclarationKind")) ?? string.Empty;

                        var isField =
                            R.ContainsIgnoreCase(tn, "field") ||
                            R.ContainsIgnoreCase(tn, "vardecl") ||
                            R.ContainsIgnoreCase(tn, "variable") ||
                            string.Equals(kind, "field", StringComparison.OrdinalIgnoreCase) ||
                            (bool?)R.P(m, "IsField") == true;

                        if (isField)
                            yield return new FieldA(m);
                    }
                }
            }

            public IEnumerable<IMethodDecl> Methods
            {
                get
                {
                    var explicitMethods = R.P(_c, "Methods", "MethodList", "Functions", "Funcs");
                    if (explicitMethods != null)
                    {
                        foreach (var m in R.AsEnum(explicitMethods))
                            yield return new MethodA(m);
                        yield break;
                    }

                    foreach (var m in GetMembers())
                    {
                        var tn   = m.GetType().Name ?? string.Empty;
                        var kind = R.S(R.P(m, "Kind", "DeclarationKind")) ?? string.Empty;

                        var looksLikeMethod =
                            R.ContainsIgnoreCase(tn, "method")   ||
                            R.ContainsIgnoreCase(tn, "func")     ||
                            R.ContainsIgnoreCase(tn, "function") ||
                            R.ContainsIgnoreCase(tn, "def")      ||
                            string.Equals(kind, "method",   StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(kind, "function", StringComparison.OrdinalIgnoreCase);

                        if (looksLikeMethod)
                            yield return new MethodA(m);
                    }
                }
            }

            public IEnumerable<IConstructorDecl> Ctors
            {
                get
                {
                    foreach (var m in GetMembers())
                    {
                        var n = m.GetType().Name ?? string.Empty;
                        if (R.ContainsIgnoreCase(n, "Ctor") || R.ContainsIgnoreCase(n, "Constructor"))
                            yield return new CtorA(m);
                    }
                }
            }

            private IEnumerable<object> GetMembers()
            {
                return R.AsEnum(R.P(_c, "Members", "Body", "Declarations", "Defs"));
            }
        }

        private sealed class FieldA : IVariableDecl
        {
            private readonly object _f;
            public FieldA(object f) { _f = f; }

            public string Name
            {
                get
                {
                    var s = R.S(R.P(_f, "Name", "Id"));
                    return string.IsNullOrEmpty(s) ? "field" : s;
                }
            }

            public string TypeName
            {
                get
                {
                    var t = R.S(R.P(_f, "TypeName", "Type"));
                    if (!string.IsNullOrWhiteSpace(t)) return t;
                    var init = R.P(_f, "Init", "Initializer", "Expression");
                    var guess = GuessTypeFromExpr(init);
                    return guess ?? "Integer";
                }
            }

            public IExpression Init
            {
                get
                {
                    var e = R.P(_f, "Init", "Initializer", "Expression");
                    if (e == null) throw new InvalidOperationException("Field initializer required");
                    return ExprA.Wrap(e);
                }
            }

            private static string GuessTypeFromExpr(object e)
            {
                if (e == null) return null;
                var n = e.GetType().Name ?? string.Empty;
                if (R.ContainsIgnoreCase(n, "int")) return "Integer";
                if (R.ContainsIgnoreCase(n, "real") || R.ContainsIgnoreCase(n, "double") || R.ContainsIgnoreCase(n, "float")) return "Real";
                if (R.ContainsIgnoreCase(n, "bool")) return "Boolean";
                var newClass = R.S(R.P(e, "ClassName", "Name", "Id"));
                return newClass;
            }
        }

        private sealed class MethodA : IMethodDecl
        {
            private readonly object _m;
            public MethodA(object m) { _m = m; }

            public string Name
            {
                get
                {
                    var s = R.S(R.P(_m, "Name", "Id"));
                    return string.IsNullOrEmpty(s) ? "m" : s;
                }
            }

            public IReadOnlyList<(string name, string type)> Params
            {
                get
                {
                    var list = new List<(string name, string type)>();
                    foreach (var p in R.AsEnum(R.P(_m, "Params", "Parameters")))
                    {
                        var n = R.S(R.P(p, "Name", "Id")) ?? "arg";
                        var t = R.S(R.P(p, "TypeName", "Type")) ?? "Integer";
                        list.Add((n, t));
                    }
                    return list;
                }
            }

            public string ReturnType
            {
                get
                {
                    var t = R.S(R.P(_m,
                        "ReturnType", "Type", "ReturnTypeName", "ResultType", "RetType", "Return", "Result"));
                    if (!string.IsNullOrWhiteSpace(t)) return t;

                    var e = R.P(_m, "ExprBody", "ExpressionBody", "Expr", "Expression");
                    if (e != null)
                    {
                        var n = e.GetType().Name ?? string.Empty;
                        if (R.ContainsIgnoreCase(n, "int"))    return "Integer";
                        if (R.ContainsIgnoreCase(n, "real") ||
                            R.ContainsIgnoreCase(n, "double") ||
                            R.ContainsIgnoreCase(n, "float"))  return "Real";
                        if (R.ContainsIgnoreCase(n, "bool"))   return "Boolean";

                        var cls = R.S(R.P(e, "ClassName", "Name", "Id"));
                        if (!string.IsNullOrWhiteSpace(cls)) return cls;
                    }
                    return null;
                }
            }

            public IBody Body
            {
                get
                {
                    var body = R.P(_m, "Body", "Block");
                    return body == null ? null : new BodyA(body);
                }
            }

            public bool IsVirtual  { get { return (bool?)R.P(_m, "IsVirtual")  == true; } }
            public bool IsOverride { get { return (bool?)R.P(_m, "IsOverride") == true; } }

            public bool IsExpressionBodied =>
                (bool?)R.P(_m, "IsArrowBody") == true ||
                R.P(_m, "ArrowExpr", "ExprBody", "ExpressionBody", "Expr", "Expression") != null;

            public IExpression ExprBody
            {
                get
                {
                    var e = R.P(_m, "ArrowExpr", "ExprBody", "ExpressionBody", "Expr", "Expression");
                    return e == null ? null : ExprA.Wrap(e);
                }
            }
        }

        private sealed class CtorA : IConstructorDecl
        {
            private readonly object _k;
            public CtorA(object k) { _k = k; }

            public IReadOnlyList<(string name, string type)> Params
            {
                get
                {
                    var list = new List<(string name, string type)>();
                    foreach (var p in R.AsEnum(R.P(_k, "Params", "Parameters")))
                    {
                        var n = R.S(R.P(p, "Name", "Id")) ?? "arg";
                        var t = R.S(R.P(p, "TypeName", "Type")) ?? "Integer";
                        list.Add((n, t));
                    }
                    return list;
                }
            }

            public IBody Body
            {
                get
                {
                    var b = R.P(_k, "Body", "Block");
                    if (b == null) throw new InvalidOperationException("Ctor body missing");
                    return new BodyA(b);
                }
            }
        }

        private sealed class BodyA : IBody
        {
            private readonly object _b;
            public BodyA(object b) { _b = b; }

            public IEnumerable<IVariableDecl> Locals
            {
                get
                {
                    foreach (var v in R.AsEnum(R.P(_b, "Locals", "LocalVars", "Variables")))
                        yield return new LocalA(v);
                }
            }

            public IEnumerable<IStatement> Statements
            {
                get
                {
                    foreach (var s in R.AsEnum(R.P(_b, "Statements", "Stmts", "Body")))
                        yield return StmtA.Wrap(s);
                }
            }
        }

        private sealed class LocalA : IVariableDecl
        {
            private readonly object _v;
            public LocalA(object v) { _v = v; }

            public string Name     { get { return R.S(R.P(_v, "Name", "Id")) ?? "v"; } }
            public string TypeName { get { return R.S(R.P(_v, "TypeName", "Type")) ?? "Integer"; } }
            public IExpression Init
            {
                get
                {
                    var e = R.P(_v, "Init", "Initializer", "Expression");
                    if (e == null) throw new InvalidOperationException("Local initializer required");
                    return ExprA.Wrap(e);
                }
            }
        }

        private sealed class StmtA
        {
            public static IStatement Wrap(object s)
            {
                var tn = s.GetType().Name;
                var low = (tn ?? string.Empty).ToLowerInvariant();
                if (R.ContainsIgnoreCase(low, "assign")) return new AssignA(s);
                if (R.ContainsIgnoreCase(low, "while"))  return new WhileA(s);
                if (low == "if" || R.ContainsIgnoreCase(low, "ifstmt")) return new IfA(s);
                if (R.ContainsIgnoreCase(low, "return")) return new ReturnA(s);
                throw new NotSupportedException("Unknown statement: " + s.GetType().Name);
            }
        }

        private sealed class AssignA : IAssignment
        {
            private readonly object _s;
            public AssignA(object s) { _s = s; }

            public string Target
            {
                get
                {
                    var t = R.S(R.P(_s, "TargetName", "Target", "Left", "Lhs"));
                    return string.IsNullOrEmpty(t) ? "x" : t;
                }
            }

            public IExpression Value
            {
                get
                {
                    var e = R.P(_s, "Value", "Right", "Rhs");
                    if (e == null) throw new InvalidOperationException("Assign value missing");
                    return ExprA.Wrap(e);
                }
            }
        }

        private sealed class WhileA : IWhile
        {
            private readonly object _s; public WhileA(object s) { _s = s; }
            public IExpression Cond
            {
                get
                {
                    var e = R.P(_s, "Cond", "Condition", "Expr");
                    if (e == null) throw new InvalidOperationException("While cond missing");
                    return ExprA.Wrap(e);
                }
            }
            public IBody Body
            {
                get
                {
                    var b = R.P(_s, "Body", "Block");
                    if (b == null) throw new InvalidOperationException("While body missing");
                    return new BodyA(b);
                }
            }
        }

        private sealed class IfA : IIf
        {
            private readonly object _s; public IfA(object s) { _s = s; }
            public IExpression Cond
            {
                get
                {
                    var e = R.P(_s, "Cond", "Condition", "Expr");
                    if (e == null) throw new InvalidOperationException("If cond missing");
                    return ExprA.Wrap(e);
                }
            }
            public IBody Then
            {
                get
                {
                    var b = R.P(_s, "Then", "ThenBlock", "ThenBody");
                    if (b == null) throw new InvalidOperationException("If then missing");
                    return new BodyA(b);
                }
            }
            public IBody Else
            {
                get
                {
                    var e = R.P(_s, "Else", "ElseBlock", "ElseBody");
                    return e == null ? null : new BodyA(e);
                }
            }
        }

        private sealed class ReturnA : IReturn
        {
            private readonly object _s; public ReturnA(object s) { _s = s; }
            public IExpression Expr
            {
                get
                {
                    var e = R.P(_s, "Expr", "Expression", "Value");
                    return e == null ? null : ExprA.Wrap(e);
                }
            }
        }

        internal sealed class ExprA :
            ILiteralInt, ILiteralReal, ILiteralBool, IThis, IName, ICall, INew, IDot, IExpression
        {
            private readonly object _e;
            public ExprA(object e) { _e = e; }
            public static IExpression Wrap(object e) { return new ExprA(e); }

            internal string Kind => (_e.GetType().Name ?? string.Empty).ToLowerInvariant();

            internal bool IsIntLiteralLike()
            {
                var n = Kind;
                if (R.ContainsIgnoreCase(n, "intliteral")) return true;
                if (R.ContainsIgnoreCase(n, "int") && R.P(_e, "Value") != null && !R.ContainsIgnoreCase(n, "call"))
                    return true;
                if (R.ContainsIgnoreCase(n, "new") || R.ContainsIgnoreCase(n, "ctor"))
                {
                    var cls = R.S(R.P(_e, "ClassName", "Name", "Id"));
                    if (string.Equals(cls, "Integer", StringComparison.OrdinalIgnoreCase))
                    {
                        var args = R.AsEnum(R.P(_e, "Args", "Arguments", "Params")).ToList();
                        return args.Count == 1 && Wrap(args[0]) is ExprA ex && ex.IsIntLiteralLike();
                    }
                }
                return false;
            }

            internal bool IsRealLiteralLike()
            {
                var n = Kind;
                if (R.ContainsIgnoreCase(n, "real") || R.ContainsIgnoreCase(n, "double"))
                    return R.P(_e, "Value") != null && !R.ContainsIgnoreCase(n, "call");
                if (R.ContainsIgnoreCase(n, "new") || R.ContainsIgnoreCase(n, "ctor"))
                {
                    var cls = R.S(R.P(_e, "ClassName", "Name", "Id"));
                    if (string.Equals(cls, "Real", StringComparison.OrdinalIgnoreCase))
                    {
                        var args = R.AsEnum(R.P(_e, "Args", "Arguments", "Params")).ToList();
                        return args.Count == 1 && Wrap(args[0]) is ExprA ex && ex.IsRealLiteralLike();
                    }
                }
                return false;
            }

            internal bool IsBoolLiteralLike()
            {
                var n = Kind;
                if (R.ContainsIgnoreCase(n, "bool") && R.P(_e, "Value") != null && !R.ContainsIgnoreCase(n, "call"))
                    return true;
                if (R.ContainsIgnoreCase(n, "new") || R.ContainsIgnoreCase(n, "ctor"))
                {
                    var cls = R.S(R.P(_e, "ClassName", "Name", "Id"));
                    if (string.Equals(cls, "Boolean", StringComparison.OrdinalIgnoreCase))
                    {
                        var args = R.AsEnum(R.P(_e, "Args", "Arguments", "Params")).ToList();
                        return args.Count == 1 && Wrap(args[0]) is ExprA ex && ex.IsBoolLiteralLike();
                    }
                }
                return false;
            }

            public int Value
            {
                get
                {
                    if (IsIntLiteralLike())
                        return Convert.ToInt32(R.P(_e, "Value") ?? 0);

                    var n = Kind;
                    if (R.ContainsIgnoreCase(n, "new") || R.ContainsIgnoreCase(n, "ctor"))
                    {
                        var cls = R.S(R.P(_e, "ClassName", "Name", "Id"));
                        if (string.Equals(cls, "Integer", StringComparison.OrdinalIgnoreCase))
                        {
                            var args = R.AsEnum(R.P(_e, "Args", "Arguments", "Params")).ToList();
                            if (args.Count == 1)
                                return Wrap(args[0]) is ILiteralInt li ? li.Value : 0;
                        }
                    }
                    return 0;
                }
            }
            double ILiteralReal.Value
            {
                get
                {
                    if (IsRealLiteralLike())
                        return Convert.ToDouble(R.P(_e, "Value") ?? 0.0);
                    return 0.0;
                }
            }
            bool ILiteralBool.Value
            {
                get
                {
                    if (IsBoolLiteralLike())
                        return Convert.ToBoolean(R.P(_e, "Value") ?? false);
                    return false;
                }
            }

            public string Id { get { return R.S(R.P(_e, "Id", "Name", "Text")) ?? "id"; } }
            public IExpression Left
            {
                get
                {
                    var l = R.P(_e, "Left", "Target");
                    if (l == null) throw new InvalidOperationException("Dot.Left missing");
                    return Wrap(l);
                }
            }
            public string Member { get { return R.S(R.P(_e, "Member", "Name")) ?? "member"; } }

            public IExpression Target
            {
                get
                {
                    var t = R.P(_e, "Target", "Callee", "Receiver");
                    if (t == null) throw new InvalidOperationException("Call.Target missing");
                    return Wrap(t);
                }
            }
            public IReadOnlyList<IExpression> Args
            {
                get
                {
                    var list = new List<IExpression>();
                    foreach (var a in R.AsEnum(R.P(_e, "Args", "Arguments", "Params")))
                        list.Add(Wrap(a));
                    return list;
                }
            }

            public string ClassName { get { return R.S(R.P(_e, "ClassName", "Name", "Id")) ?? "Object"; } }
            IReadOnlyList<IExpression> INew.Args { get { return Args; } }

            public override string ToString()
            {
                var k = Kind;
                if (R.ContainsIgnoreCase(k, "this")) return "this";
                if (R.ContainsIgnoreCase(k, "name") || k.EndsWith("ident") || k.EndsWith("identifier"))
                    return Id;
                if (R.ContainsIgnoreCase(k, "dot") || R.ContainsIgnoreCase(k, "member"))
                    return Wrap(R.P(_e, "Left", "Target")).ToString() + "." + R.S(R.P(_e, "Member", "Name"));
                return base.ToString();
            }
        }
    }
}
