using System;
using System.IO;
using O_Lexer;
using O_Parser;
using O_Parser.AST;
using O_Parser.Utilities;
using O_Parser.Analyzer;
using EmitBackend.Utilities;

namespace EmitBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run -- <sourcefile.o> [--compile-net] [-o <out.exe>] [--start <Class>]");
                return;
            }

            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Logger.Error($"File '{filePath}' not found.");
                return;
            }

            bool compileNet = Array.IndexOf(args, "--compile-net") >= 0;
            bool enableOptimizations = Array.IndexOf(args, "--no-optimize") < 0;
            string outputPath = null;
            string entryClass = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-o" && i + 1 < args.Length)
                {
                    outputPath = args[i + 1];
                }
                if (args[i] == "--start" && i + 1 < args.Length)
                {
                    entryClass = args[i + 1];
                }
            }

            if (outputPath == null)
            {
                outputPath = Path.GetFileNameWithoutExtension(filePath) + "_output";
            }

            Logger.Info($"Parsing file: {filePath}");

            try
            {
                string code = File.ReadAllText(filePath);
                var lexer = new Lexer(code);
                var parser = new Parser(lexer);

                var ast = parser.ParseProgram();
                Logger.Success($"Parsing successful. Classes: {ast.Classes.Count}");

                // Семантический анализ
                var analyzer = new SemanticAnalyzer();
                ast = analyzer.Analyze(ast, enableOptimizations: enableOptimizations);

                // Компиляция в .NET сборку
                if (compileNet)
                {
                    var compiler = new Compiler(Path.GetFileNameWithoutExtension(outputPath));
                    compiler.Compile(ast, outputPath, entryClass);
                }
            }
            catch (SyntaxError ex)
            {
                Logger.Error($"Syntax error: {ex.Message}");
                Environment.Exit(1);
            }
            catch (SemanticError ex)
            {
                Logger.Error($"Semantic error: {ex.Message}");
                Environment.Exit(1);
            }
            catch (CompilationException ex)
            {
                Logger.Error($"Compilation error: {ex.Message}");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}

