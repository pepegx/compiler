using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace EmitBackend
{
    public static class Entry
    {
        public static bool CompileFromAst(object astRoot, string inputPath, string[] args)
        {
            if (!args.Contains("--compile-net")) return false;

            var programNode = new MyAstAdapter(astRoot);
            string startClass = GetOption(args, "--start") ?? programNode.Classes.First().Name;
            string outPath    = GetOption(args, "-o") ?? Path.ChangeExtension(Path.GetFileName(inputPath), ".exe");

            IlCompiler.Compile(programNode, outPath, startClassName: startClass, makeExe: true);
            Console.WriteLine($"[Emit] OK → {outPath}");
            return true;
        }

        public static bool TryHandleCompile(string[] args)
        {
            if (!args.Contains("--compile-net")) return false;

            var inputPath = args.FirstOrDefault(a => !a.StartsWith("-"));
            if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
                throw new FileNotFoundException("Input file not found for --compile-net", inputPath ?? "<null>");

            var root = ParseAndAnalyze(inputPath, noOptimize: args.Contains("--no-optimize"));

            var programNode = new MyAstAdapter(root);
            string startClass = GetOption(args, "--start") ?? programNode.Classes.First().Name;
            string outPath    = GetOption(args, "-o") ?? Path.ChangeExtension(Path.GetFileName(inputPath), ".exe");

            IlCompiler.Compile(programNode, outPath, startClassName: startClass, makeExe: true);
            Console.WriteLine($"[Emit] OK → {outPath}");
            return true;
        }

        private static object ParseAndAnalyze(string path, bool noOptimize)
        {
            var source = File.ReadAllText(path);
            var all = AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeGetTypes).ToArray();

            var parserType = all.FirstOrDefault(t =>
                (t.Namespace?.IndexOf("Parser", StringComparison.OrdinalIgnoreCase) >= 0) &&
                t.Name.IndexOf("Parser", StringComparison.OrdinalIgnoreCase) >= 0)
                ?? throw new InvalidOperationException("Parser type not found.");

            if (TryStaticParse(parserType, path, source, out var rootStatic))
                return AnalyzeIfAny(all, rootStatic, noOptimize);

            var (parser, ctorArgKind, ctorArgValue) = CreateParserInstance(parserType, path, source, all);

            var root = InvokeInstanceParse(parserType, parser, path, source, ctorArgKind, ctorArgValue, all);

            return AnalyzeIfAny(all, root, noOptimize);
        }

        private static bool TryStaticParse(Type parserType, string path, string source, out object root)
        {
            var ms = parserType.GetMethods(BindingFlags.Public | BindingFlags.Static);

            var m = ms.FirstOrDefault(x => x.Name.IndexOf("ParseFile", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                           x.GetParameters().Length == 1 &&
                                           x.GetParameters()[0].ParameterType == typeof(string));
            if (m != null) { root = m.Invoke(null, new object[] { path })!; return true; }

            m = ms.FirstOrDefault(x => x.Name.Equals("Parse", StringComparison.OrdinalIgnoreCase) &&
                                       x.GetParameters().Length == 1 &&
                                       x.GetParameters()[0].ParameterType == typeof(string));
            if (m != null) { root = m.Invoke(null, new object[] { source })!; return true; }

            m = ms.FirstOrDefault(x => x.Name.Equals("Parse", StringComparison.OrdinalIgnoreCase) &&
                                       x.GetParameters().Length == 1 &&
                                       typeof(TextReader).IsAssignableFrom(x.GetParameters()[0].ParameterType));
            if (m != null)
            {
                using var tr = new StringReader(source);
                root = m.Invoke(null, new object[] { tr })!;
                return true;
            }

            root = null!;
            return false;
        }

        private enum ArgKind { None, String, TextReader, Lexer, Tokens }

        private static (object parser, ArgKind kind, object? arg) CreateParserInstance(Type parserType, string path, string source, IEnumerable<Type> allTypes)
        {
            var ctor = parserType.GetConstructor(Type.EmptyTypes);
            if (ctor != null) return (ctor.Invoke(null)!, ArgKind.None, null);

            ctor = parserType.GetConstructor(new[] { typeof(string) });
            if (ctor != null) return (ctor.Invoke(new object[] { source })!, ArgKind.String, source);

            ctor = parserType.GetConstructor(new[] { typeof(TextReader) });
            if (ctor != null)
            {
                var tr = new StringReader(source);
                return (ctor.Invoke(new object[] { tr })!, ArgKind.TextReader, tr);
            }

            var lexerType = allTypes.FirstOrDefault(t =>
                (t.Namespace?.IndexOf("Lexer", StringComparison.OrdinalIgnoreCase) >= 0) &&
                 t.Name.IndexOf("Lexer", StringComparison.OrdinalIgnoreCase) >= 0);

            if (lexerType != null)
            {
                ctor = parserType.GetConstructor(new[] { lexerType });
                if (ctor != null)
                {
                    var lexer = CreateLexerInstance(lexerType, source);
                    return (ctor.Invoke(new object[] { lexer })!, ArgKind.Lexer, lexer);
                }
            }

            var tokenish = allTypes.FirstOrDefault(t =>
                t.Name.IndexOf("Token", StringComparison.OrdinalIgnoreCase) >= 0);
            if (tokenish != null)
            {
                ctor = parserType.GetConstructor(new[] { tokenish });
                if (ctor != null)
                {
                    var tokens = CreateTokensViaLexer(lexerType, source);
                    return (ctor.Invoke(new object[] { tokens })!, ArgKind.Tokens, tokens);
                }
            }

            throw new InvalidOperationException($"No suitable constructor found for {parserType.FullName}.");
        }

        private static object InvokeInstanceParse(Type parserType, object parser, string path, string source,
                                                  ArgKind ctorArgKind, object? ctorArg, IEnumerable<Type> allTypes)
        {
            var ms = parserType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            var m = ms.FirstOrDefault(x => x.Name.Equals("Parse", StringComparison.OrdinalIgnoreCase) &&
                                           x.GetParameters().Length == 0);
            if (m != null) return m.Invoke(parser, Array.Empty<object>())!;

            m = ms.FirstOrDefault(x => x.Name.IndexOf("ParseFile", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                       x.GetParameters().Length == 1 &&
                                       x.GetParameters()[0].ParameterType == typeof(string));
            if (m != null) return m.Invoke(parser, new object[] { path })!;

            m = ms.FirstOrDefault(x => x.Name.Equals("Parse", StringComparison.OrdinalIgnoreCase) &&
                                       x.GetParameters().Length == 1 &&
                                       x.GetParameters()[0].ParameterType == typeof(string));
            if (m != null) return m.Invoke(parser, new object[] { source })!;

            m = ms.FirstOrDefault(x => x.Name.Equals("Parse", StringComparison.OrdinalIgnoreCase) &&
                                       x.GetParameters().Length == 1 &&
                                       typeof(TextReader).IsAssignableFrom(x.GetParameters()[0].ParameterType));
            if (m != null)
            {
                using var tr = new StringReader(source);
                return m.Invoke(parser, new object[] { tr })!;
            }

            var tokMethod = ms.FirstOrDefault(x =>
                x.Name.Equals("Parse", StringComparison.OrdinalIgnoreCase) &&
                x.GetParameters().Length == 1 &&
                x.GetParameters()[0].ParameterType.Name.IndexOf("Token", StringComparison.OrdinalIgnoreCase) >= 0);

            if (tokMethod != null)
            {
                object tokens;
                if (ctorArgKind == ArgKind.Tokens && ctorArg != null) tokens = ctorArg;
                else
                {
                    var lexerType = allTypes.FirstOrDefault(t =>
                        (t.Namespace?.IndexOf("Lexer", StringComparison.OrdinalIgnoreCase) >= 0) &&
                         t.Name.IndexOf("Lexer", StringComparison.OrdinalIgnoreCase) >= 0);
                    tokens = CreateTokensViaLexer(lexerType, source);
                }
                return tokMethod.Invoke(parser, new[] { tokens })!;
            }

            throw new InvalidOperationException("No suitable instance Parse*(...) method found in Parser.");
        }

        private static object CreateLexerInstance(Type lexerType, string source)
        {
            var ctor = lexerType.GetConstructor(new[] { typeof(string) });
            if (ctor != null) return ctor.Invoke(new object[] { source })!;

            ctor = lexerType.GetConstructor(new[] { typeof(TextReader) });
            if (ctor != null)
            {
                var tr = new StringReader(source);
                return ctor.Invoke(new object[] { tr })!;
            }

            ctor = lexerType.GetConstructor(Type.EmptyTypes);
            if (ctor != null)
            {
                var lex = ctor.Invoke(null)!;
                var init = lexerType.GetMethod("Reset") ?? lexerType.GetMethod("Init") ?? lexerType.GetMethod("SetSource");
                if (init != null)
                {
                    var ps = init.GetParameters();
                    if (ps.Length == 1 && ps[0].ParameterType == typeof(string))
                        init.Invoke(lex, new object[] { source });
                    else if (ps.Length == 1 && typeof(TextReader).IsAssignableFrom(ps[0].ParameterType))
                        init.Invoke(lex, new object[] { new StringReader(source) });
                }
                return lex;
            }

            throw new InvalidOperationException($"Cannot construct lexer {lexerType.FullName}.");
        }

        private static object CreateTokensViaLexer(Type? lexerType, string source)
        {
            if (lexerType == null)
                throw new InvalidOperationException("Token-based parse requested but no Lexer type found.");

            var lexer = CreateLexerInstance(lexerType, source);

            var pi = lexerType.GetProperty("Tokens") ??
                     lexerType.GetProperty("AllTokens") ??
                     lexerType.GetProperty("TokenStream") ??
                     lexerType.GetProperty("Stream");

            if (pi == null)
                throw new InvalidOperationException($"Lexer {lexerType.FullName} does not expose token stream (Tokens/AllTokens/TokenStream/Stream).");

            return pi.GetValue(lexer)!;
        }

        private static object AnalyzeIfAny(IEnumerable<Type> all, object root, bool noOptimize)
        {
            var analyzerType = all.FirstOrDefault(t =>
                (t.Namespace?.IndexOf("Analyzer", StringComparison.OrdinalIgnoreCase) >= 0) &&
                (t.Name.IndexOf("Analyzer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 t.Name.IndexOf("Semantic", StringComparison.OrdinalIgnoreCase) >= 0));

            if (analyzerType == null) return root;

            var analyzer = Activator.CreateInstance(analyzerType);
            var methods = analyzerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var candidates = new[] { "Analyze", "Run", "Check" };

            MethodInfo? m =
                methods.FirstOrDefault(x => candidates.Contains(x.Name) &&
                    Match(x.GetParameters(), new[] { root.GetType(), typeof(bool) })) ??
                methods.FirstOrDefault(x => candidates.Contains(x.Name) &&
                    Match(x.GetParameters(), new[] { root.GetType() }));

            if (m == null) return root;

            var pars = m.GetParameters();
            var result = pars.Length == 2
                ? m.Invoke(analyzer, new object?[] { root, noOptimize })
                : m.Invoke(analyzer, new object?[] { root });

            return result ?? root;
        }
        public static bool CompileFromAst(object astRoot, string[] args)
        {
            if (!args.Contains("--compile-net")) return false;

            string inputPath = args.FirstOrDefault(a => !a.StartsWith("-")) ?? "program.o";

            var programNode = new MyAstAdapter(astRoot);
            string startClass = GetOption(args, "--start") ?? programNode.Classes.First().Name;
            string outPath    = GetOption(args, "-o") ?? Path.ChangeExtension(Path.GetFileName(inputPath), ".exe");

            IlCompiler.Compile(programNode, outPath, startClassName: startClass, makeExe: true);
            Console.WriteLine($"[Emit] OK → {outPath}");
            return true;
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly a)
        {
            try { return a.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null)!; }
        }

        private static bool Match(ParameterInfo[] ps, Type[] sig)
            => ps.Length == sig.Length && ps.Zip(sig, (p, t) => p.ParameterType.IsAssignableFrom(t)).All(b => b);

        private static string? GetOption(string[] a, string name)
        {
            for (int i = 0; i < a.Length - 1; i++)
                if (a[i] == name) return a[i + 1];
            return null;
        }
    }
}
