using System.Collections.Generic;

namespace O_Parser.AST
{
    public abstract class MemberDeclNode : Node { }

    public sealed class VarDeclNode : MemberDeclNode
    {
        public string Name { get; }
        public ExprNode Initializer { get; }
        public VarDeclNode(string name, ExprNode init)
        {
            Name = name; Initializer = init;
        }
    }

    public sealed class ParameterNode : Node
    {
        public string Name { get; }
        public string TypeName { get; }
        public ParameterNode(string name, string typeName)
        {
            Name = name; TypeName = typeName;
        }
    }

    public abstract class RoutineDeclNode : MemberDeclNode
    {
        public string Name { get; protected set; } = "";
        public List<ParameterNode> Parameters { get; } = new();
        public BlockNode? Body { get; set; }
    }

    public sealed class MethodDeclNode : RoutineDeclNode
    {
        public string? ReturnType { get; }
        public bool IsArrowBody { get; }
        public ExprNode? ArrowExpr { get; }
        public MethodDeclNode(string name, IEnumerable<ParameterNode> pars, string? retType, BlockNode? body)
        {
            Name = name; Parameters.AddRange(pars); ReturnType = retType; Body = body;
        }
        public MethodDeclNode(string name, IEnumerable<ParameterNode> pars, string? retType, ExprNode arrowExpr)
        {
            Name = name; Parameters.AddRange(pars); ReturnType = retType; IsArrowBody = true; ArrowExpr = arrowExpr;
        }
    }

    public sealed class CtorDeclNode : RoutineDeclNode
    {
        public CtorDeclNode(IEnumerable<ParameterNode> pars, BlockNode body)
        {
            Name = "this"; Parameters.AddRange(pars); Body = body;
        }
    }

    public sealed class BlockNode : Node
    {
        public List<VarDeclNode> Locals { get; } = new();
        public List<StmtNode> Statements { get; } = new();
        // Смешанный список для сохранения порядка: VarDeclNode или StmtNode
        public List<Node> Body { get; } = new();
    }
}
