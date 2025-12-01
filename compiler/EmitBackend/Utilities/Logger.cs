using System;

namespace EmitBackend.Utilities
{
    public static class Logger
    {
        public static void Info(string message)
        {
            Console.WriteLine($"ℹ {message}");
        }

        public static void Success(string message)
        {
            Console.WriteLine($"✓ {message}");
        }

        public static void Warning(string message)
        {
            Console.WriteLine($"⚠ {message}");
        }

        public static void Error(string message)
        {
            Console.WriteLine($"✗ {message}");
        }
    }
}

