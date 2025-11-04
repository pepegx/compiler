using System;

namespace O_Parser.Analyzer
{
    public class SemanticError : Exception
    {
        public SemanticError(string message) : base(message) { }
    }
}
