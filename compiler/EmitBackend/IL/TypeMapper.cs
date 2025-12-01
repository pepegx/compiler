using System;
using System.Collections.Generic;

namespace EmitBackend.IL
{
    public static class TypeMapper
    {
        private static readonly Dictionary<string, Type> _builtinTypes = new Dictionary<string, Type>
        {
            ["Integer"] = typeof(int),
            ["Real"] = typeof(double),
            ["Boolean"] = typeof(bool),
            ["String"] = typeof(string)
        };

        public static Type MapType(string typeName)
        {
            if (_builtinTypes.TryGetValue(typeName, out var type))
            {
                return type;
            }
            
            // Для пользовательских типов вернем object
            // Реальное разрешение будет через BuildContext
            return typeof(object);
        }

        public static bool IsBuiltinType(string typeName)
        {
            return _builtinTypes.ContainsKey(typeName);
        }

        public static Type GetBuiltinType(string typeName)
        {
            return _builtinTypes.TryGetValue(typeName, out var type) ? type : typeof(object);
        }
    }
}

