using System.Collections.Generic;

namespace EmitBackend
{
    public interface IProgramNode { IEnumerable<IClassDecl> Classes { get; } }

    public interface IClassDecl
    {
        string Name { get; }
        string? BaseName { get; }
        IEnumerable<IVariableDecl> Fields { get; }
        IEnumerable<IMethodDecl> Methods { get; }
        IEnumerable<IConstructorDecl> Ctors { get; }
    }

    public interface IVariableDecl
    {
        string Name { get; }
        string TypeName { get; }   
        IExpression Init { get; }
    }

    public interface IMethodDecl
    {
        string Name { get; }
        IReadOnlyList<(string name, string type)> Params { get; }
        string? ReturnType { get; }   
        IBody? Body { get; }          
        bool IsVirtual { get; }
        bool IsOverride { get; }
        bool IsExpressionBodied { get; }
        IExpression? ExprBody { get; }
    }

    public interface IConstructorDecl
    {
        IReadOnlyList<(string name, string type)> Params { get; }
        IBody Body { get; }
    }

    public interface IBody
    {
        IEnumerable<IVariableDecl> Locals { get; }
        IEnumerable<IStatement> Statements { get; }
    }

    public interface IStatement { }
    public interface IAssignment : IStatement { string Target { get; } IExpression Value { get; } }
    public interface IWhile      : IStatement { IExpression Cond { get; } IBody Body { get; } }
    public interface IIf         : IStatement { IExpression Cond { get; } IBody Then { get; } IBody? Else { get; } }
    public interface IReturn     : IStatement { IExpression? Expr { get; } }

    public interface IExpression { }
    public interface ILiteralInt  : IExpression { int Value { get; } }
    public interface ILiteralReal : IExpression { double Value { get; } }
    public interface ILiteralBool : IExpression { bool Value { get; } }
    public interface IThis        : IExpression { }
    public interface IName        : IExpression { string Id { get; } }
    public interface ICall        : IExpression { IExpression Target { get; } IReadOnlyList<IExpression> Args { get; } }
    public interface INew         : IExpression { string ClassName { get; } IReadOnlyList<IExpression> Args { get; } }
    public interface IDot         : IExpression { IExpression Left { get; } string Member { get; } }
}
