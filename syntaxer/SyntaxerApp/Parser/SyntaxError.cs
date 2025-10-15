using System;

namespace O_Parser
{
    public sealed class SyntaxError : Exception
    {
        public SyntaxError(string msg) : base(msg) { }
    }
}
