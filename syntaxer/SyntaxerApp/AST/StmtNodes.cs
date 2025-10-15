namespace O_Parser.AST
{
    public abstract class StmtNode : Node { }

    public sealed class AssignStmt : StmtNode
    {
        public string TargetName { get; }
        public ExprNode Value { get; }
        public AssignStmt(string targetName, ExprNode value)
        { TargetName = targetName; Value = value; }
    }

    public sealed class ExprStmt : StmtNode
    {
        public ExprNode Expr { get; }
        public ExprStmt(ExprNode expr) { Expr = expr; }
    }

    public sealed class WhileStmt : StmtNode
    {
        public ExprNode Condition { get; }
        public BlockNode Body { get; }
        public WhileStmt(ExprNode cond, BlockNode body)
        { Condition = cond; Body = body; }
    }

    public sealed class IfStmt : StmtNode
    {
        public ExprNode Condition { get; }
        public BlockNode ThenBody { get; }
        public BlockNode? ElseBody { get; }
        public IfStmt(ExprNode cond, BlockNode thenBody, BlockNode? elseBody)
        { Condition = cond; ThenBody = thenBody; ElseBody = elseBody; }
    }

    public sealed class ReturnStmt : StmtNode
    {
        public ExprNode? Value { get; }
        public ReturnStmt(ExprNode? value) { Value = value; }
    }
}
