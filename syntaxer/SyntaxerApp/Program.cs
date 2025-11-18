using System;
using System.IO;
// using EmitBackend;  // не нужен — вызываем с полным именем
using O_Lexer;
using O_Parser;
using O_Parser.AST;
using O_Parser.Utilities;
using O_Parser.Analyzer;

namespace O_Parser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run -- <sourcefile.o> [--no-optimize] [--compile-net] [--start <Class>] [-o <out.exe>]");
                return;
            }

            // В твоих запусках файл всегда идёт первым позиционным аргументом
            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: file '{filePath}' not found.");
                return;
            }

            // Оптимизации включены по умолчанию; отключаются флагом --no-optimize
            bool enableOptimizations = Array.IndexOf(args, "--no-optimize") < 0;

            Console.WriteLine($"Parsing file: {filePath}\n");

            try
            {
                string code  = File.ReadAllText(filePath);
                var lexer    = new Lexer(code);
                var parser   = new Parser(lexer);

                var ast = parser.ParseProgram();
                Console.WriteLine($"✓ Parsing successful. Classes: {ast.Classes.Count}\n");

                // Семантика
                var analyzer = new SemanticAnalyzer();
                ast = analyzer.Analyze(ast, enableOptimizations);

                // <<< ВАЖНО: компиляция в бинарь — ТОЛЬКО здесь, когда AST уже готов >>>
                if (EmitBackend.Entry.CompileFromAst(ast, args)) return;
                // ^ вернётся true, если был флаг --compile-net; тогда завершаем программу

                // Иначе — печать AST (как и раньше)
                Console.WriteLine("=== Final AST ===");
                AstTreePrinter.Print(ast);
            }
            catch (SyntaxError ex)
            {
                Console.WriteLine("✗ Syntax error: " + ex.Message);
                Environment.Exit(1);
            }
            catch (SemanticError ex)
            {
                Console.WriteLine("✗ Semantic error: " + ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
