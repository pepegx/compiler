using System.Collections.Generic;

namespace O_Parser.AST
{
    public sealed class ProgramNode : Node
    {
        public List<ClassDeclNode> Classes { get; } = new();
    }
}
