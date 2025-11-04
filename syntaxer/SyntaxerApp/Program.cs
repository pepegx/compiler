using System;
using System.IO;
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
                Console.WriteLine("Usage: dotnet run -- <sourcefile.o> [--no-optimize]");
                return;
            }

            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: file '{filePath}' not found.");
                return;
            }

            bool enableOptimizations = true;
            if (args.Length > 1 && args[1] == "--no-optimize")
            {
                enableOptimizations = false;
            }

            string code = File.ReadAllText(filePath);
            var lexer  = new Lexer(code);
            var parser = new Parser(lexer);

            Console.WriteLine($"Parsing file: {filePath}\n");

            try
            {
                var ast = parser.ParseProgram();
                Console.WriteLine($"✓ Parsing successful. Classes: {ast.Classes.Count}\n");
                
                // Run semantic analysis
                var analyzer = new SemanticAnalyzer();
                ast = analyzer.Analyze(ast, enableOptimizations);
                
                // Print the AST (potentially optimized)
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
