using System;

namespace EmitBackend.Utilities
{
    public class CompilationException : Exception
    {
        public CompilationException(string message) : base(message) { }
    }

    public static class ErrorHandler
    {
        public static void ReportError(string message)
        {
            Logger.Error(message);
            throw new CompilationException(message);
        }

        public static void ReportWarning(string message)
        {
            Logger.Warning(message);
        }
    }
}

