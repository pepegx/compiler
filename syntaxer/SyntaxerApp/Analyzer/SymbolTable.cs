using System;
using System.Collections.Generic;

namespace O_Parser.Analyzer
{
    // Symbol kinds to track what each symbol represents
    public enum SymbolKind
    {
        Class,
        Method,
        Constructor,
        Variable,
        Parameter
    }

    // Symbol information
    public class Symbol
    {
        public string Name { get; }
        public SymbolKind Kind { get; }
        public string? TypeName { get; }
        public bool IsUsed { get; set; }

        public Symbol(string name, SymbolKind kind, string? typeName = null)
        {
            Name = name;
            Kind = kind;
            TypeName = typeName;
            IsUsed = false;
        }
    }

    // Scope for managing symbol visibility
    public class Scope
    {
        private readonly Dictionary<string, Symbol> _symbols = new();
        public Scope? Parent { get; }

        public Scope(Scope? parent = null)
        {
            Parent = parent;
        }

        public void Define(Symbol symbol)
        {
            if (_symbols.ContainsKey(symbol.Name))
            {
                throw new SemanticError($"Symbol '{symbol.Name}' is already defined in this scope");
            }
            _symbols[symbol.Name] = symbol;
        }

        public Symbol? Resolve(string name)
        {
            if (_symbols.TryGetValue(name, out var symbol))
            {
                return symbol;
            }
            return Parent?.Resolve(name);
        }

        public bool ExistsInCurrentScope(string name)
        {
            return _symbols.ContainsKey(name);
        }

        public IEnumerable<Symbol> GetAllSymbols()
        {
            return _symbols.Values;
        }
    }

    // Symbol table for managing scopes
    public class SymbolTable
    {
        private Scope _currentScope;
        private readonly Scope _globalScope;

        public SymbolTable()
        {
            _globalScope = new Scope();
            _currentScope = _globalScope;
        }

        public void EnterScope()
        {
            _currentScope = new Scope(_currentScope);
        }

        public void ExitScope()
        {
            if (_currentScope.Parent != null)
            {
                _currentScope = _currentScope.Parent;
            }
        }

        public void Define(string name, SymbolKind kind, string? typeName = null)
        {
            var symbol = new Symbol(name, kind, typeName);
            _currentScope.Define(symbol);
        }

        public Symbol? Resolve(string name)
        {
            return _currentScope.Resolve(name);
        }

        public void MarkUsed(string name)
        {
            var symbol = Resolve(name);
            if (symbol != null)
            {
                symbol.IsUsed = true;
            }
        }

        public Scope CurrentScope => _currentScope;
        public Scope GlobalScope => _globalScope;
    }
}
