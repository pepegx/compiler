using System;
using System.Collections.Generic;
using System.IO;

namespace O_Lexer
{
    public enum TokenType
    {
        // Keywords
        Class, Extends, Is, End, Var, Method, This, Return,
        While, Loop, If, Then, Else,
        True, False,

        // Symbols
        Colon, Semicolon, Comma, Dot,
        Assign, Arrow, LParen, RParen, LBracket, RBracket,

        // Literals
        IntegerLiteral, RealLiteral, BooleanLiteral,

        // Identifier
        Identifier,

        // Other
        EOF,
        Unknown
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Type,-15} '{Value}'";
        }
    }

    public class Lexer
    {
        private readonly string _input;
    private int _pos = 0;

        private static readonly Dictionary<string, TokenType> Keywords = new()
        {
            { "class", TokenType.Class },
            { "extends", TokenType.Extends },
            { "is", TokenType.Is },
            { "end", TokenType.End },
            { "var", TokenType.Var },
            { "method", TokenType.Method },
            { "this", TokenType.This },
            { "return", TokenType.Return },
            { "while", TokenType.While },
            { "loop", TokenType.Loop },
            { "if", TokenType.If },
            { "then", TokenType.Then },
            { "else", TokenType.Else },
            { "true", TokenType.True },
            { "false", TokenType.False }
        };

        public Lexer(string input)
        {
            _input = input;
        }

        private char Current => _pos < _input.Length ? _input[_pos] : '\0';

        private void Advance()
        {
            _pos++;
        }

        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(Current)) Advance();
        }

        private string ReadWhile(Func<char, bool> predicate)
        {
            int start = _pos;
            while (predicate(Current)) Advance();
            return _input.Substring(start, _pos - start);
        }

        public Token NextToken()
        {
            SkipWhitespace();

            if (_pos >= _input.Length)
                return new Token(TokenType.EOF, "");

            char c = Current;

            // Identifiers or keywords
            if (char.IsLetter(c))
            {
                string ident = ReadWhile(ch => char.IsLetterOrDigit(ch));
                if (Keywords.ContainsKey(ident))
                    return new Token(Keywords[ident], ident);
                return new Token(TokenType.Identifier, ident);
            }

            // Numbers
            if (char.IsDigit(c))
            {
                string number = ReadWhile(ch => char.IsDigit(ch));
                if (Current == '.')
                {
                    Advance();
                    number += "." + ReadWhile(ch => char.IsDigit(ch));
                    return new Token(TokenType.RealLiteral, number);
                }
                return new Token(TokenType.IntegerLiteral, number);
            }

            // Operators & symbols
            switch (c)
            {
                case ':':
                    Advance();
                    if (Current == '=')
                    {
                        Advance();
                        return new Token(TokenType.Assign, ":=");
                    }
                    return new Token(TokenType.Colon, ":");

                case ';': Advance(); return new Token(TokenType.Semicolon, ";");
                case ',': Advance(); return new Token(TokenType.Comma, ",");
                case '.': Advance(); return new Token(TokenType.Dot, ".");
                case '(': 
                    Advance();
                    return new Token(TokenType.LParen, "(");
                case ')':
                    Advance();
                    return new Token(TokenType.RParen, ")");
                case '[':
                    Advance();
                    return new Token(TokenType.LBracket, "[");
                case ']':
                    Advance();
                    return new Token(TokenType.RBracket, "]");
                case '=':
                    Advance();
                    if (Current == '>')
                    {
                        Advance();
                        return new Token(TokenType.Arrow, "=>");
                    }
                    break;
            }

            // Unknown
            Advance();
            return new Token(TokenType.Unknown, c.ToString());
        }

        public IEnumerable<Token> Tokenize()
        {
            var tokens = new List<Token>();
            Token t;
            while ((t = NextToken()).Type != TokenType.EOF)
            {
                tokens.Add(t);
            }
            tokens.Add(t); // add EOF
            return tokens;
        }
    }

    class Program
    {
        static void Main(string[] args)
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
            Lexer lexer = new(code);

            Console.WriteLine($"Lexing file: {filePath}\n");

            foreach (var token in lexer.Tokenize())
                Console.WriteLine(token);
        }
    }
}
