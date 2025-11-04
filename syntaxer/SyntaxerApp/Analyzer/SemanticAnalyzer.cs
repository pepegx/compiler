using System;
using System.Collections.Generic;
using O_Parser.AST;

namespace O_Parser.Analyzer
{
    // Main Semantic Analyzer that coordinates all checks and optimizations
    public class SemanticAnalyzer
    {
        private readonly SemanticChecker _checker;
        private readonly ASTOptimizer _optimizer;
        private readonly List<string> _allMessages = new();

        public SemanticAnalyzer()
        {
            _checker = new SemanticChecker();
            _optimizer = new ASTOptimizer();
        }

        public List<string> Messages => _allMessages;

        // Analyze the program: perform checks and optimizations
        public ProgramNode Analyze(ProgramNode program, bool enableOptimizations = true)
        {
            _allMessages.Clear();

            Console.WriteLine("=== Starting Semantic Analysis ===\n");

            // Phase 1: Semantic Checks (non-modifying)
            Console.WriteLine("Phase 1: Semantic Checks");
            Console.WriteLine("------------------------");
            
            try
            {
                _checker.Check(program);
                _checker.CheckUnusedSymbols();
                
                Console.WriteLine("✓ All semantic checks passed");
                
                // Add warnings to messages
                foreach (var warning in _checker.Warnings)
                {
                    Console.WriteLine(warning);
                    _allMessages.Add(warning);
                }
            }
            catch (SemanticError ex)
            {
                Console.WriteLine($"✗ Semantic error: {ex.Message}");
                throw; // Re-throw to stop compilation
            }

            Console.WriteLine();

            // Phase 2: AST Optimizations (modifying)
            if (enableOptimizations)
            {
                Console.WriteLine("Phase 2: AST Optimizations");
                Console.WriteLine("--------------------------");
                
                _optimizer.Optimize(program);
                
                if (_optimizer.OptimizationLog.Count > 0)
                {
                    foreach (var optimization in _optimizer.OptimizationLog)
                    {
                        Console.WriteLine(optimization);
                        _allMessages.Add(optimization);
                    }
                }
                else
                {
                    Console.WriteLine("No optimizations applied");
                }
            }
            else
            {
                Console.WriteLine("Phase 2: Optimizations disabled");
            }

            Console.WriteLine();
            Console.WriteLine("=== Semantic Analysis Complete ===\n");

            return program;
        }

        // Analyze with just checks (no optimizations)
        public void CheckOnly(ProgramNode program)
        {
            Analyze(program, enableOptimizations: false);
        }
    }
}
