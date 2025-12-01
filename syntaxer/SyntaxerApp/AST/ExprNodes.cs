using System.Collections.Generic;

namespace O_Parser.AST
{
    public abstract class ExprNode : Node { }

    public sealed class ThisExpr : ExprNode { public static readonly ThisExpr Instance = new(); private ThisExpr() { } }
    public sealed class BoolLiteral : ExprNode { public bool Value { get; } public BoolLiteral(bool v){Value=v;} }
    public sealed class IntLiteral : ExprNode { public long Value { get; } public IntLiteral(long v){Value=v;} }
    public sealed class RealLiteral : ExprNode { public double Value { get; } public RealLiteral(double v){Value=v;} }
    public sealed class StringLiteral : ExprNode { public string Value { get; } public StringLiteral(string v){Value=v;} }
    public sealed class IdentifierExpr : ExprNode { public string Name { get; } public IdentifierExpr(string n){Name=n;} }

    public sealed class MemberAccessExpr : ExprNode
    {
        public ExprNode Target { get; }
        public string Member { get; }
        public MemberAccessExpr(ExprNode target, string member){Target=target; Member=member;}
    }

    public sealed class CallExpr : ExprNode
    {
        public ExprNode Callee { get; }
        public List<ExprNode> Arguments { get; } = new();
        public CallExpr(ExprNode callee, IEnumerable<ExprNode> args){Callee=callee;Arguments.AddRange(args);}    
    }

    public sealed class NewExpr : ExprNode
    {
        public string ClassName { get; }
        public List<ExprNode> Arguments { get; } = new();
        public NewExpr(string className, IEnumerable<ExprNode> args){ClassName=className;Arguments.AddRange(args);}    
    }
}
