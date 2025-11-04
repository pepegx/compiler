using System;
using System.Collections.Generic;
using O_Parser.AST;

namespace O_Parser.Analyzer
{
    // Context for tracking current analysis state
    public class AnalysisContext
    {
        public bool IsInLoop { get; set; }
        public bool IsInFunction { get; set; }
        public string? CurrentFunctionReturnType { get; set; }
        public string? CurrentClassName { get; set; }
    }

    // Semantic checker performs non-modifying checks on the AST
    public class SemanticChecker
    {
        private readonly SymbolTable _symbolTable;
        private readonly AnalysisContext _context;
        private readonly List<string> _warnings = new();

        public SemanticChecker()
        {
            _symbolTable = new SymbolTable();
            _context = new AnalysisContext();
            
            // Register built-in types
            _symbolTable.Define("Object", SymbolKind.Class);
            _symbolTable.Define("Integer", SymbolKind.Class);
            _symbolTable.Define("Real", SymbolKind.Class);
            _symbolTable.Define("Boolean", SymbolKind.Class);
            _symbolTable.Define("String", SymbolKind.Class);
        }

        public List<string> Warnings => _warnings;

        public void Check(ProgramNode program)
        {
            // First pass: collect all class declarations
            foreach (var cls in program.Classes)
            {
                CheckClassDeclaration(cls);
            }

            // Second pass: check class members
            foreach (var cls in program.Classes)
            {
                CheckClassMembers(cls);
            }
        }

        private void CheckClassDeclaration(ClassDeclNode cls)
        {
            // Check for duplicate class declaration
            if (_symbolTable.CurrentScope.ExistsInCurrentScope(cls.Name))
            {
                throw new SemanticError($"Class '{cls.Name}' is already declared");
            }
            _symbolTable.Define(cls.Name, SymbolKind.Class);

            // Check if base class exists
            if (cls.BaseName != null)
            {
                var baseSymbol = _symbolTable.Resolve(cls.BaseName);
                if (baseSymbol == null)
                {
                    throw new SemanticError($"Base class '{cls.BaseName}' for class '{cls.Name}' is not declared");
                }
                if (baseSymbol.Kind != SymbolKind.Class)
                {
                    throw new SemanticError($"'{cls.BaseName}' is not a class, cannot be used as base class");
                }
            }
        }

        private void CheckClassMembers(ClassDeclNode cls)
        {
            _context.CurrentClassName = cls.Name;
            _symbolTable.EnterScope(); // Enter class scope

            // Collect all members first to allow forward references
            foreach (var member in cls.Members)
            {
                if (member is MethodDeclNode method)
                {
                    if (_symbolTable.CurrentScope.ExistsInCurrentScope(method.Name))
                    {
                        throw new SemanticError($"Method '{method.Name}' is already declared in class '{cls.Name}'");
                    }
                    _symbolTable.Define(method.Name, SymbolKind.Method, method.ReturnType);
                }
                else if (member is CtorDeclNode)
                {
                    if (_symbolTable.CurrentScope.ExistsInCurrentScope("this"))
                    {
                        throw new SemanticError($"Constructor is already declared in class '{cls.Name}'");
                    }
                    _symbolTable.Define("this", SymbolKind.Constructor);
                }
                else if (member is VarDeclNode varDecl)
                {
                    if (_symbolTable.CurrentScope.ExistsInCurrentScope(varDecl.Name))
                    {
                        throw new SemanticError($"Variable '{varDecl.Name}' is already declared in class '{cls.Name}'");
                    }
                    _symbolTable.Define(varDecl.Name, SymbolKind.Variable);
                }
            }

            // Now check the implementations
            foreach (var member in cls.Members)
            {
                if (member is MethodDeclNode method)
                {
                    CheckMethod(method);
                }
                else if (member is CtorDeclNode ctor)
                {
                    CheckConstructor(ctor);
                }
                else if (member is VarDeclNode varDecl)
                {
                    CheckExpression(varDecl.Initializer);
                }
            }

            _symbolTable.ExitScope(); // Exit class scope
            _context.CurrentClassName = null;
        }

        private void CheckMethod(MethodDeclNode method)
        {
            _context.IsInFunction = true;
            _context.CurrentFunctionReturnType = method.ReturnType;
            _symbolTable.EnterScope(); // Enter method scope

            // Add parameters to scope
            foreach (var param in method.Parameters)
            {
                if (_symbolTable.CurrentScope.ExistsInCurrentScope(param.Name))
                {
                    throw new SemanticError($"Parameter '{param.Name}' is already declared in method '{method.Name}'");
                }
                _symbolTable.Define(param.Name, SymbolKind.Parameter, param.TypeName);
            }

            // Check method body
            if (method.IsArrowBody && method.ArrowExpr != null)
            {
                CheckExpression(method.ArrowExpr);
                // Arrow function implicitly returns the expression
            }
            else if (method.Body != null)
            {
                CheckBlock(method.Body);
            }

            _symbolTable.ExitScope(); // Exit method scope
            _context.IsInFunction = false;
            _context.CurrentFunctionReturnType = null;
        }

        private void CheckConstructor(CtorDeclNode ctor)
        {
            _context.IsInFunction = true;
            _symbolTable.EnterScope(); // Enter constructor scope

            // Add parameters to scope
            foreach (var param in ctor.Parameters)
            {
                if (_symbolTable.CurrentScope.ExistsInCurrentScope(param.Name))
                {
                    throw new SemanticError($"Parameter '{param.Name}' is already declared in constructor");
                }
                _symbolTable.Define(param.Name, SymbolKind.Parameter, param.TypeName);
            }

            // Check constructor body
            if (ctor.Body != null)
            {
                CheckBlock(ctor.Body);
            }

            _symbolTable.ExitScope(); // Exit constructor scope
            _context.IsInFunction = false;
        }

        private void CheckBlock(BlockNode block)
        {
            // Add local variables to scope
            foreach (var localVar in block.Locals)
            {
                if (_symbolTable.CurrentScope.ExistsInCurrentScope(localVar.Name))
                {
                    throw new SemanticError($"Variable '{localVar.Name}' is already declared in this scope");
                }
                _symbolTable.Define(localVar.Name, SymbolKind.Variable);
                CheckExpression(localVar.Initializer);
            }

            // Check statements
            foreach (var stmt in block.Statements)
            {
                CheckStatement(stmt);
            }
        }

        private void CheckStatement(StmtNode stmt)
        {
            switch (stmt)
            {
                case AssignStmt assign:
                    // Check: variable must be declared before usage
                    var symbol = _symbolTable.Resolve(assign.TargetName);
                    if (symbol == null)
                    {
                        throw new SemanticError($"Variable '{assign.TargetName}' is not declared");
                    }
                    if (symbol.Kind != SymbolKind.Variable && symbol.Kind != SymbolKind.Parameter)
                    {
                        throw new SemanticError($"'{assign.TargetName}' is not a variable, cannot assign to it");
                    }
                    _symbolTable.MarkUsed(assign.TargetName);
                    CheckExpression(assign.Value);
                    break;

                case ExprStmt exprStmt:
                    CheckExpression(exprStmt.Expr);
                    break;

                case WhileStmt whileStmt:
                    CheckExpression(whileStmt.Condition);
                    bool wasInLoop = _context.IsInLoop;
                    _context.IsInLoop = true;
                    CheckBlock(whileStmt.Body);
                    _context.IsInLoop = wasInLoop;
                    break;

                case IfStmt ifStmt:
                    CheckExpression(ifStmt.Condition);
                    CheckBlock(ifStmt.ThenBody);
                    if (ifStmt.ElseBody != null)
                    {
                        CheckBlock(ifStmt.ElseBody);
                    }
                    break;

                case ReturnStmt returnStmt:
                    // Check: return must be inside a function
                    if (!_context.IsInFunction)
                    {
                        throw new SemanticError("'return' statement can only be used inside a function or method");
                    }
                    if (returnStmt.Value != null)
                    {
                        CheckExpression(returnStmt.Value);
                    }
                    // Type checking: verify return type matches function signature
                    if (_context.CurrentFunctionReturnType == null && returnStmt.Value != null)
                    {
                        _warnings.Add($"WARNING: Function has no return type but returns a value");
                    }
                    break;

                default:
                    break;
            }
        }

        private void CheckExpression(ExprNode expr)
        {
            switch (expr)
            {
                case ThisExpr:
                    // 'this' can only be used inside a class
                    if (_context.CurrentClassName == null)
                    {
                        throw new SemanticError("'this' can only be used inside a class");
                    }
                    break;

                case IdentifierExpr identifier:
                    // Check: variable/function must be declared before usage
                    var symbol = _symbolTable.Resolve(identifier.Name);
                    if (symbol == null)
                    {
                        throw new SemanticError($"Identifier '{identifier.Name}' is not declared");
                    }
                    _symbolTable.MarkUsed(identifier.Name);
                    break;

                case MemberAccessExpr memberAccess:
                    CheckExpression(memberAccess.Target);
                    // Note: We can't fully check member existence without type information
                    break;

                case CallExpr call:
                    CheckExpression(call.Callee);
                    foreach (var arg in call.Arguments)
                    {
                        CheckExpression(arg);
                    }
                    break;

                case NewExpr newExpr:
                    // Check: class must be declared
                    var classSymbol = _symbolTable.Resolve(newExpr.ClassName);
                    if (classSymbol == null)
                    {
                        throw new SemanticError($"Class '{newExpr.ClassName}' is not declared");
                    }
                    if (classSymbol.Kind != SymbolKind.Class)
                    {
                        throw new SemanticError($"'{newExpr.ClassName}' is not a class");
                    }
                    _symbolTable.MarkUsed(newExpr.ClassName);
                    foreach (var arg in newExpr.Arguments)
                    {
                        CheckExpression(arg);
                    }
                    break;

                case BoolLiteral:
                case IntLiteral:
                case RealLiteral:
                    // Literals are always valid
                    break;

                default:
                    break;
            }
        }

        // Report unused variables
        public void CheckUnusedSymbols()
        {
            CheckUnusedInScope(_symbolTable.GlobalScope);
        }

        private void CheckUnusedInScope(Scope scope)
        {
            foreach (var symbol in scope.GetAllSymbols())
            {
                if (!symbol.IsUsed && symbol.Kind == SymbolKind.Variable)
                {
                    _warnings.Add($"WARNING: Variable '{symbol.Name}' is declared but never used");
                }
            }
        }
    }
}
