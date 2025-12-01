using System;
using System.Collections.Generic;
using System.Linq;
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
        private ProgramNode? _program;

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
            _symbolTable.Define("Array", SymbolKind.Class); // Array is a built-in generic type
            _symbolTable.Define("List", SymbolKind.Class); // List is a built-in generic type
        }

        public List<string> Warnings => _warnings;

        public void Check(ProgramNode program)
        {
            _program = program;
            
            // First pass: register all class names
            foreach (var cls in program.Classes)
            {
                if (_symbolTable.CurrentScope.ExistsInCurrentScope(cls.Name))
                {
                    throw new SemanticError($"Class '{cls.Name}' is already declared");
                }
                _symbolTable.Define(cls.Name, SymbolKind.Class);
            }
            
            // Second pass: check inheritance relationships
            foreach (var cls in program.Classes)
            {
                CheckClassInheritance(cls, program);
            }

            // Third pass: check class members
            foreach (var cls in program.Classes)
            {
                CheckClassMembers(cls);
            }
        }
        
        private void CheckClassInheritance(ClassDeclNode cls, ProgramNode program)
        {
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
                
                // Check for cyclic inheritance
                var visited = new HashSet<string>();
                string? current = cls.Name;
                while (current != null)
                {
                    if (visited.Contains(current))
                    {
                        throw new SemanticError($"Cyclic inheritance detected: class '{cls.Name}' has circular inheritance chain");
                    }
                    visited.Add(current);
                    
                    // Find the class and get its base
                    var currentClass = program.Classes.FirstOrDefault(c => c.Name == current);
                    current = currentClass?.BaseName;
                }
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

            // Add fields from base classes to the symbol table
            if (cls.BaseName != null && _program != null)
            {
                AddBaseClassFields(cls.BaseName);
            }

            // Collect all members first to allow forward references
            foreach (var member in cls.Members)
            {
                if (member is MethodDeclNode method)
                {
                    // Check if method with same name and signature already exists
                    var existingMethods = cls.Members.OfType<MethodDeclNode>()
                        .Where(m => m.Name == method.Name && m != method)
                        .ToList();
                    
                    // Check for duplicate signature (same parameter count and types, same return type)
                    foreach (var existingMethod in existingMethods)
                    {
                        // Check if signatures match exactly (same parameters and return type)
                        bool sameSignature = existingMethod.Parameters.Count == method.Parameters.Count &&
                                           existingMethod.ReturnType == method.ReturnType;
                        
                        if (sameSignature)
                        {
                            for (int i = 0; i < existingMethod.Parameters.Count; i++)
                            {
                                if (existingMethod.Parameters[i].TypeName != method.Parameters[i].TypeName)
                                {
                                    sameSignature = false;
                                    break;
                                }
                            }
                        }
                        
                        if (sameSignature)
                        {
                            // Same signature - check if it's a forward declaration issue
                            bool existingHasBody = existingMethod.Body != null || existingMethod.ArrowExpr != null;
                            bool currentHasBody = method.Body != null || method.ArrowExpr != null;
                            if (existingHasBody && currentHasBody)
                            {
                                throw new SemanticError($"Method '{method.Name}' with the same signature is already declared in class '{cls.Name}'");
                            }
                            // If one is forward declaration and other is implementation, that's OK
                            // Don't define again if it's a forward declaration
                            if (!currentHasBody)
                            {
                                // This is a forward declaration, don't define in symbol table
                                continue;
                            }
                        }
                    }
                    
                    // Different signature or new method - allow method overloading
                    // Only define in symbol table if not already defined (for forward declarations)
                    var existing = _symbolTable.Resolve(method.Name);
                    if (existing == null || existing.Kind != SymbolKind.Method)
                    {
                        _symbolTable.Define(method.Name, SymbolKind.Method, method.ReturnType);
                    }
                }
                else if (member is CtorDeclNode ctor)
                {
                    // Check for duplicate constructor with same signature
                    var existingCtors = cls.Members.OfType<CtorDeclNode>()
                        .Where(c => c != ctor && c.Parameters.Count == ctor.Parameters.Count);
                    foreach (var existingCtor in existingCtors)
                    {
                        bool sameSignature = true;
                        for (int i = 0; i < ctor.Parameters.Count; i++)
                        {
                            if (existingCtor.Parameters[i].TypeName != ctor.Parameters[i].TypeName)
                            {
                                sameSignature = false;
                                break;
                            }
                        }
                        if (sameSignature)
                        {
                            throw new SemanticError($"Constructor with the same signature is already declared in class '{cls.Name}'");
                        }
                    }
                    // Don't define "this" in symbol table - multiple constructors are allowed with different signatures
                }
                else if (member is VarDeclNode varDecl)
                {
                    if (_symbolTable.CurrentScope.ExistsInCurrentScope(varDecl.Name))
                    {
                        throw new SemanticError($"Variable '{varDecl.Name}' is already declared in class '{cls.Name}'");
                    }
                    // Infer the type of the field from its initializer
                    string? fieldType = InferExpressionType(varDecl.Initializer);
                    _symbolTable.Define(varDecl.Name, SymbolKind.Variable, fieldType);
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

            // Check return type exists
            if (method.ReturnType != null)
            {
                CheckTypeExists(method.ReturnType, $"Return type of method '{method.Name}'");
            }

            // Add parameters to scope
            foreach (var param in method.Parameters)
            {
                if (_symbolTable.CurrentScope.ExistsInCurrentScope(param.Name))
                {
                    throw new SemanticError($"Parameter '{param.Name}' is already declared in method '{method.Name}'");
                }
                // Check if parameter type exists
                CheckTypeExists(param.TypeName, $"Parameter '{param.Name}' in method '{method.Name}'");
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
                // Check if parameter type exists
                CheckTypeExists(param.TypeName, $"Parameter '{param.Name}' in constructor");
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
        
        // Check if a type name refers to an existing type
        private void CheckTypeExists(string typeName, string context)
        {
            // Handle generic types like Array[Integer] or List[Integer]
            string baseTypeName = typeName;
            string? elementTypeName = null;
            
            if (typeName.Contains("[") && typeName.Contains("]"))
            {
                int bracketStart = typeName.IndexOf('[');
                int bracketEnd = typeName.LastIndexOf(']');
                baseTypeName = typeName.Substring(0, bracketStart);
                elementTypeName = typeName.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
            }
            
            // Check base type
            var baseSymbol = _symbolTable.Resolve(baseTypeName);
            if (baseSymbol == null)
            {
                throw new SemanticError($"Type '{baseTypeName}' does not exist. {context} has invalid type.");
            }
            if (baseSymbol.Kind != SymbolKind.Class)
            {
                throw new SemanticError($"'{baseTypeName}' is not a type. {context} has invalid type.");
            }
            
            // Check element type for generic types
            if (elementTypeName != null)
            {
                var elementSymbol = _symbolTable.Resolve(elementTypeName);
                if (elementSymbol == null)
                {
                    throw new SemanticError($"Type '{elementTypeName}' does not exist. {context} uses invalid generic type.");
                }
            }
        }

        private void CheckBlock(BlockNode block)
        {
            // Add local variables to scope first
            // In language O, all var declarations in a block are considered declared 
            // before any statements in that block (hoisting-like behavior)
            foreach (var localVar in block.Locals)
            {
                if (_symbolTable.CurrentScope.ExistsInCurrentScope(localVar.Name))
                {
                    throw new SemanticError($"Variable '{localVar.Name}' is already declared in this scope");
                }
                // Infer type from initializer
                string? varType = InferExpressionType(localVar.Initializer);
                _symbolTable.Define(localVar.Name, SymbolKind.Variable, varType);
                CheckExpression(localVar.Initializer);
            }

            // Check statements
            foreach (var stmt in block.Statements)
            {
                CheckStatement(stmt);
            }
        }
        
        // Check if a statement uses any local variables (which would mean using before declaration)
        private void CheckStatementForLocalUsage(StmtNode stmt, HashSet<string> localNames)
        {
            switch (stmt)
            {
                case AssignStmt assign:
                    // Check if assigning to a local variable (before it's declared)
                    if (localNames.Contains(assign.TargetName))
                    {
                        // Check if it exists in outer scope
                        var symbol = _symbolTable.Resolve(assign.TargetName);
                        if (symbol == null)
                        {
                            throw new SemanticError($"Variable '{assign.TargetName}' is used before it is declared");
                        }
                    }
                    // Check the expression for local usage
                    CheckExpressionForLocalUsage(assign.Value, localNames);
                    break;
                    
                case ExprStmt exprStmt:
                    CheckExpressionForLocalUsage(exprStmt.Expr, localNames);
                    break;
                    
                case ReturnStmt returnStmt:
                    if (returnStmt.Value != null)
                    {
                        CheckExpressionForLocalUsage(returnStmt.Value, localNames);
                    }
                    break;
                    
                case WhileStmt whileStmt:
                    CheckExpressionForLocalUsage(whileStmt.Condition, localNames);
                    break;
                    
                case IfStmt ifStmt:
                    CheckExpressionForLocalUsage(ifStmt.Condition, localNames);
                    break;
            }
        }
        
        // Check if an expression uses any local variables
        private void CheckExpressionForLocalUsage(ExprNode expr, HashSet<string> localNames)
        {
            switch (expr)
            {
                case IdentifierExpr idExpr:
                    if (localNames.Contains(idExpr.Name))
                    {
                        // Check if it exists in outer scope
                        var symbol = _symbolTable.Resolve(idExpr.Name);
                        if (symbol == null)
                        {
                            throw new SemanticError($"Variable '{idExpr.Name}' is used before it is declared");
                        }
                    }
                    break;
                    
                case MemberAccessExpr memberAccess:
                    CheckExpressionForLocalUsage(memberAccess.Target, localNames);
                    break;
                    
                case CallExpr call:
                    CheckExpressionForLocalUsage(call.Callee, localNames);
                    foreach (var arg in call.Arguments)
                    {
                        CheckExpressionForLocalUsage(arg, localNames);
                    }
                    break;
                    
                case NewExpr newExpr:
                    foreach (var arg in newExpr.Arguments)
                    {
                        CheckExpressionForLocalUsage(arg, localNames);
                    }
                    break;
            }
        }
        
        // Check if a statement uses variables that haven't been declared yet
        private void CheckStatementForUndeclaredVars(StmtNode stmt, List<VarDeclNode> locals, HashSet<string> declaredVars)
        {
            switch (stmt)
            {
                case AssignStmt assign:
                    // Check if target is a local that's declared later
                    var localDeclIndex = locals.FindIndex(v => v.Name == assign.TargetName);
                    if (localDeclIndex >= 0 && !declaredVars.Contains(assign.TargetName))
                    {
                        // This variable is declared later in this block
                        // This is an error - using variable before declaration
                        throw new SemanticError($"Variable '{assign.TargetName}' is used before it is declared");
                    }
                    // Check used variables in the expression
                    CheckExpressionForUndeclaredVars(assign.Value, locals, declaredVars);
                    break;
                    
                case ExprStmt exprStmt:
                    CheckExpressionForUndeclaredVars(exprStmt.Expr, locals, declaredVars);
                    break;
                    
                case WhileStmt whileStmt:
                    CheckExpressionForUndeclaredVars(whileStmt.Condition, locals, declaredVars);
                    break;
                    
                case IfStmt ifStmt:
                    CheckExpressionForUndeclaredVars(ifStmt.Condition, locals, declaredVars);
                    break;
                    
                case ReturnStmt returnStmt:
                    if (returnStmt.Value != null)
                        CheckExpressionForUndeclaredVars(returnStmt.Value, locals, declaredVars);
                    break;
            }
        }
        
        // Check if an expression uses variables that haven't been declared yet
        private void CheckExpressionForUndeclaredVars(ExprNode expr, List<VarDeclNode> locals, HashSet<string> declaredVars)
        {
            switch (expr)
            {
                case IdentifierExpr idExpr:
                    var localDeclIndex = locals.FindIndex(v => v.Name == idExpr.Name);
                    if (localDeclIndex >= 0 && !declaredVars.Contains(idExpr.Name))
                    {
                        throw new SemanticError($"Variable '{idExpr.Name}' is used before it is declared");
                    }
                    break;
                    
                case MemberAccessExpr memberAccess:
                    CheckExpressionForUndeclaredVars(memberAccess.Target, locals, declaredVars);
                    break;
                    
                case CallExpr call:
                    CheckExpressionForUndeclaredVars(call.Callee, locals, declaredVars);
                    foreach (var arg in call.Arguments)
                    {
                        CheckExpressionForUndeclaredVars(arg, locals, declaredVars);
                    }
                    break;
                    
                case NewExpr newExpr:
                    foreach (var arg in newExpr.Arguments)
                    {
                        CheckExpressionForUndeclaredVars(arg, locals, declaredVars);
                    }
                    break;
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
                    
                    // Type checking for assignment
                    if (symbol.TypeName != null)
                    {
                        var valueType = InferExpressionType(assign.Value);
                        if (valueType != null && !IsTypeCompatible(symbol.TypeName, valueType))
                        {
                            throw new SemanticError($"Cannot assign '{valueType}' to variable '{assign.TargetName}' of type '{symbol.TypeName}'");
                        }
                    }
                    break;

                case ExprStmt exprStmt:
                    CheckExpression(exprStmt.Expr);
                    break;

                case WhileStmt whileStmt:
                    CheckExpression(whileStmt.Condition);
                    // Check that condition is Boolean
                    var whileCondType = InferExpressionType(whileStmt.Condition);
                    if (whileCondType != null && whileCondType != "Boolean")
                    {
                        throw new SemanticError($"While condition must be Boolean, got '{whileCondType}'");
                    }
                    bool wasInLoop = _context.IsInLoop;
                    _context.IsInLoop = true;
                    CheckBlock(whileStmt.Body);
                    _context.IsInLoop = wasInLoop;
                    break;

                case IfStmt ifStmt:
                    CheckExpression(ifStmt.Condition);
                    // Check that condition is Boolean
                    var ifCondType = InferExpressionType(ifStmt.Condition);
                    if (ifCondType != null && ifCondType != "Boolean")
                    {
                        throw new SemanticError($"If condition must be Boolean, got '{ifCondType}'");
                    }
                    // Create new scope for then body
                    _symbolTable.EnterScope();
                    CheckBlock(ifStmt.ThenBody);
                    _symbolTable.ExitScope();
                    if (ifStmt.ElseBody != null)
                    {
                        // Create new scope for else body
                        _symbolTable.EnterScope();
                        CheckBlock(ifStmt.ElseBody);
                        _symbolTable.ExitScope();
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
                    // Check: method with return type must return a value
                    if (_context.CurrentFunctionReturnType != null && returnStmt.Value == null)
                    {
                        throw new SemanticError($"Method with return type '{_context.CurrentFunctionReturnType}' must return a value");
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
                    // CRITICAL: Handle generic types like Array[Integer] or List[Integer]
                    // Extract base type name (e.g., "Array" from "Array[Integer]")
                    string baseTypeNameForId = identifier.Name;
                    if (identifier.Name.Contains("["))
                    {
                        baseTypeNameForId = identifier.Name.Substring(0, identifier.Name.IndexOf('['));
                    }
                    
                    // Check if it's a built-in generic type
                    if (baseTypeNameForId == "Array" || baseTypeNameForId == "List")
                    {
                        // Array or List is a built-in type, allow it
                        break;
                    }
                    
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
                    // Check built-in method calls
                    CheckBuiltInMethodCall(call);
                    break;

                case NewExpr newExpr:
                    // CRITICAL: Handle generic types like Array[Integer]
                    // Extract base type name (e.g., "Array" from "Array[Integer]")
                    string baseTypeName = newExpr.ClassName;
                    string? elementTypeName = null;
                    
                    if (newExpr.ClassName.Contains("[") && newExpr.ClassName.Contains("]"))
                    {
                        int bracketStart = newExpr.ClassName.IndexOf('[');
                        int bracketEnd = newExpr.ClassName.LastIndexOf(']');
                        baseTypeName = newExpr.ClassName.Substring(0, bracketStart);
                        elementTypeName = newExpr.ClassName.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
                    }
                    
                    // Check if element type exists for generic types (Array/List)
                    if (elementTypeName != null && (baseTypeName == "Array" || baseTypeName == "List"))
                    {
                        var elementSymbol = _symbolTable.Resolve(elementTypeName);
                        if (elementSymbol == null)
                        {
                            throw new SemanticError($"Type '{elementTypeName}' does not exist. Cannot create {baseTypeName}[{elementTypeName}].");
                        }
                    }
                    
                    var classSymbol = _symbolTable.Resolve(baseTypeName);
                    if (classSymbol == null)
                    {
                        throw new SemanticError($"Class '{newExpr.ClassName}' is not declared");
                    }
                    else if (classSymbol.Kind != SymbolKind.Class)
                    {
                        throw new SemanticError($"'{newExpr.ClassName}' is not a class");
                    }
                    else
                    {
                        _symbolTable.MarkUsed(baseTypeName);
                    }
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

        private void AddBaseClassFields(string baseClassName)
        {
            if (_program == null) return;
            
            // Find the base class
            var baseClass = _program.Classes.FirstOrDefault(c => c.Name == baseClassName);
            if (baseClass == null) return;

            // Recursively add fields from parent classes first
            if (baseClass.BaseName != null)
            {
                AddBaseClassFields(baseClass.BaseName);
            }

            // Add fields from this base class
            foreach (var member in baseClass.Members)
            {
                if (member is VarDeclNode varDecl)
                {
                    // Only add if not already in scope (to avoid duplicates)
                    if (!_symbolTable.CurrentScope.ExistsInCurrentScope(varDecl.Name))
                    {
                        _symbolTable.Define(varDecl.Name, SymbolKind.Variable);
                    }
                }
            }
        }

        // Check built-in type methods for correct parameter count
        private void CheckBuiltInMethodCall(CallExpr call)
        {
            if (call.Callee is MemberAccessExpr memberAccess)
            {
                string methodName = memberAccess.Member;
                int argCount = call.Arguments.Count;
                
                // Get target type if possible
                string? targetType = InferTargetType(memberAccess.Target);
                
                // Built-in Integer methods
                var integerMethods = new Dictionary<string, int>
                {
                    {"Plus", 1}, {"Minus", 1}, {"Mult", 1}, {"Div", 1}, {"Rem", 1},
                    {"Less", 1}, {"Greater", 1}, {"LessEqual", 1}, {"GreaterEqual", 1},
                    {"Equal", 1}, {"UnaryMinus", 0}, {"toReal", 0}, {"toBoolean", 0}
                };
                
                // Built-in Real methods
                var realMethods = new Dictionary<string, int>
                {
                    {"Plus", 1}, {"Minus", 1}, {"Mult", 1}, {"Div", 1},
                    {"Less", 1}, {"Greater", 1}, {"LessEqual", 1}, {"GreaterEqual", 1},
                    {"Equal", 1}, {"UnaryMinus", 0}, {"toInteger", 0}
                };
                
                // Built-in Boolean methods
                var booleanMethods = new Dictionary<string, int>
                {
                    {"Or", 1}, {"And", 1}, {"Xor", 1}, {"Not", 0}, {"toInteger", 0}
                };
                
                // Built-in Array methods
                var arrayMethods = new Dictionary<string, int>
                {
                    {"get", 1}, {"set", 2}, {"Length", 0}
                };
                
                // Built-in List methods
                var listMethods = new Dictionary<string, int>
                {
                    {"append", 1}, {"head", 0}, {"tail", 0}, {"Length", 0}, {"get", 1}
                };
                
                Dictionary<string, int>? methodDict = null;
                
                if (targetType == "Integer")
                    methodDict = integerMethods;
                else if (targetType == "Real")
                    methodDict = realMethods;
                else if (targetType == "Boolean")
                    methodDict = booleanMethods;
                else if (targetType != null && targetType.StartsWith("Array["))
                    methodDict = arrayMethods;
                else if (targetType != null && targetType.StartsWith("List["))
                    methodDict = listMethods;
                
                if (methodDict != null)
                {
                    if (!methodDict.ContainsKey(methodName))
                    {
                        throw new SemanticError($"Method '{methodName}' not found on type '{targetType}'");
                    }
                    
                    int expectedArgs = methodDict[methodName];
                    if (argCount != expectedArgs)
                    {
                        throw new SemanticError($"Method '{targetType}.{methodName}' expects {expectedArgs} argument(s), but got {argCount}");
                    }
                    
                    // Check argument types for Array/List methods
                    if (targetType != null && (targetType.StartsWith("Array[") || targetType.StartsWith("List[")))
                    {
                        if (methodName == "get" && argCount == 1)
                        {
                            var indexType = InferExpressionType(call.Arguments[0]);
                            if (indexType != null && indexType != "Integer")
                            {
                                throw new SemanticError($"Array/List index must be Integer, got '{indexType}'");
                            }
                        }
                        else if (methodName == "set" && argCount == 2)
                        {
                            var indexType = InferExpressionType(call.Arguments[0]);
                            if (indexType != null && indexType != "Integer")
                            {
                                throw new SemanticError($"Array/List index must be Integer, got '{indexType}'");
                            }
                        }
                    }
                }
            }
        }
        
        // Try to infer the type of an expression for built-in method checking
        private string? InferTargetType(ExprNode expr)
        {
            switch (expr)
            {
                case IntLiteral _:
                    return "Integer";
                case RealLiteral _:
                    return "Real";
                case BoolLiteral _:
                    return "Boolean";
                case NewExpr newExpr:
                    return newExpr.ClassName;
                case IdentifierExpr idExpr:
                    // Try to find the variable type from symbol table
                    var symbol = _symbolTable.Resolve(idExpr.Name);
                    if (symbol != null && symbol.TypeName != null)
                    {
                        return symbol.TypeName;
                    }
                    return null;
                case CallExpr callExpr:
                    // If it's a constructor call, return the class name
                    if (callExpr.Callee is IdentifierExpr ctorId)
                    {
                        var classSymbol = _symbolTable.Resolve(ctorId.Name);
                        if (classSymbol != null && classSymbol.Kind == SymbolKind.Class)
                        {
                            return ctorId.Name;
                        }
                    }
                    // If it's a method call, infer the return type
                    if (callExpr.Callee is MemberAccessExpr memberAccess)
                    {
                        var targetType = InferTargetType(memberAccess.Target);
                        if (targetType == "Integer")
                        {
                            if (memberAccess.Member == "toReal") return "Real";
                            if (memberAccess.Member == "toBoolean") return "Boolean";
                            if (memberAccess.Member == "Less" || memberAccess.Member == "Greater" ||
                                memberAccess.Member == "LessEqual" || memberAccess.Member == "GreaterEqual" ||
                                memberAccess.Member == "Equal") return "Boolean";
                            return "Integer";
                        }
                        if (targetType == "Real")
                        {
                            if (memberAccess.Member == "toInteger") return "Integer";
                            if (memberAccess.Member == "Less" || memberAccess.Member == "Greater" ||
                                memberAccess.Member == "LessEqual" || memberAccess.Member == "GreaterEqual" ||
                                memberAccess.Member == "Equal") return "Boolean";
                            return "Real";
                        }
                        if (targetType == "Boolean")
                        {
                            if (memberAccess.Member == "toInteger") return "Integer";
                            return "Boolean";
                        }
                    }
                    return null;
                default:
                    return null;
            }
        }
        
        // Infer the type of an expression for variable declaration
        private string? InferExpressionType(ExprNode expr)
        {
            switch (expr)
            {
                case IntLiteral _:
                    return "Integer";
                case RealLiteral _:
                    return "Real";
                case BoolLiteral _:
                    return "Boolean";
                case StringLiteral _:
                    return "String";
                case NewExpr newExpr:
                    return newExpr.ClassName;
                case IdentifierExpr idExpr:
                    var symbol = _symbolTable.Resolve(idExpr.Name);
                    if (symbol != null && symbol.TypeName != null)
                    {
                        return symbol.TypeName;
                    }
                    return null;
                case CallExpr callExpr:
                    // If it's a constructor call, return the class name
                    if (callExpr.Callee is IdentifierExpr ctorId)
                    {
                        var classSymbol = _symbolTable.Resolve(ctorId.Name);
                        if (classSymbol != null && classSymbol.Kind == SymbolKind.Class)
                        {
                            return ctorId.Name;
                        }
                    }
                    // For method calls, try to infer return type
                    if (callExpr.Callee is MemberAccessExpr memberAccess)
                    {
                        var targetType = InferExpressionType(memberAccess.Target);
                        // Integer methods return types
                        if (targetType == "Integer")
                        {
                            if (memberAccess.Member == "toReal") return "Real";
                            if (memberAccess.Member == "toBoolean") return "Boolean";
                            if (memberAccess.Member == "Less" || memberAccess.Member == "Greater" ||
                                memberAccess.Member == "LessEqual" || memberAccess.Member == "GreaterEqual" ||
                                memberAccess.Member == "Equal") return "Boolean";
                            return "Integer";
                        }
                        // Real methods return types
                        if (targetType == "Real")
                        {
                            if (memberAccess.Member == "toInteger") return "Integer";
                            if (memberAccess.Member == "Less" || memberAccess.Member == "Greater" ||
                                memberAccess.Member == "LessEqual" || memberAccess.Member == "GreaterEqual" ||
                                memberAccess.Member == "Equal") return "Boolean";
                            return "Real";
                        }
                        // Boolean methods return types
                        if (targetType == "Boolean")
                        {
                            if (memberAccess.Member == "toInteger") return "Integer";
                            return "Boolean";
                        }
                    }
                    return null;
                default:
                    return null;
            }
        }
        
        // Check if two types are compatible for assignment
        private bool IsTypeCompatible(string targetType, string sourceType)
        {
            // Same type is always compatible
            if (targetType == sourceType) return true;
            
            // Integer and Real are compatible (implicit conversion)
            if ((targetType == "Integer" || targetType == "Real") &&
                (sourceType == "Integer" || sourceType == "Real"))
                return true;
            
            // Object accepts any type
            if (targetType == "Object") return true;
            
            // Check inheritance (would need program context)
            // For now, return false for incompatible types
            return false;
        }
    }
}
