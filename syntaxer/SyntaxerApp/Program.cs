using System;
using System.IO;
using O_Lexer;
using O_Parser;
using O_Parser.AST;
using O_Parser.Utilities;

namespace O_Parser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run -- <sourcefile.o>");
                return;
            }

            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: file '{filePath}' not found.");
                return;
            }

            string code = File.ReadAllText(filePath);
            var lexer  = new Lexer(code);
            var parser = new Parser(lexer);

            Console.WriteLine($"Parsing file: {filePath}\n");

            try
            {
                var ast = parser.ParseProgram();
                Console.WriteLine($"Parsed OK. Classes: {ast.Classes.Count}\n");
                AstTreePrinter.Print(ast);
            }
            catch (SyntaxError ex)
            {
                Console.WriteLine("Syntax error: " + ex.Message);
            }
        }
    }
}
