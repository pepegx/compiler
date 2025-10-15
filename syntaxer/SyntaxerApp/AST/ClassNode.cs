using System.Collections.Generic;

namespace O_Parser.AST
{
    public sealed class ClassDeclNode : Node
    {
        public string Name { get; }
        public string? BaseName { get; }
        public List<MemberDeclNode> Members { get; } = new();
        public ClassDeclNode(string name, string? baseName)
        {
            Name = name; BaseName = baseName;
        }
    }
}
