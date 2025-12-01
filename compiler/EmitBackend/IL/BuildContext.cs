using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using O_Parser.AST;

namespace EmitBackend.IL
{
    public class BuildContext
    {
        public ILGenerator IL { get; }
        public TypeBuilder CurrentType { get; }
        public ClassDeclNode CurrentClass { get; }
        public Dictionary<string, TypeBuilder> Classes { get; }
        public Dictionary<string, Type> BuiltinTypes { get; }
        // Изменено: теперь хранит список методов для каждого имени (поддержка перегрузки)
        public Dictionary<string, Dictionary<string, List<MethodBuilder>>> ClassMethods { get; }
        public Dictionary<MethodBuilder, List<Type>> MethodParamTypes { get; }
        public Dictionary<string, List<ConstructorBuilder>> ClassConstructors { get; }
        public Dictionary<ConstructorBuilder, List<Type>> ConstructorParamTypes { get; }

        private readonly Dictionary<string, LocalBuilder> _locals = new Dictionary<string, LocalBuilder>();
        private readonly Dictionary<string, Type> _localRealTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, (int index, Type type)> _parameters = new Dictionary<string, (int, Type)>();
        private readonly Dictionary<string, Type> _parameterRealTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, FieldBuilder> _fields = new Dictionary<string, FieldBuilder>();
        private readonly Dictionary<string, Type> _fieldRealTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _arrayElementTypes = new Dictionary<string, Type>();

        public BuildContext(
            ILGenerator il,
            TypeBuilder currentType,
            ClassDeclNode currentClass,
            Dictionary<string, TypeBuilder> classes,
            Dictionary<string, Type> builtinTypes,
            Dictionary<string, Dictionary<string, List<MethodBuilder>>> classMethods,
            Dictionary<MethodBuilder, List<Type>> methodParamTypes,
            Dictionary<string, List<ConstructorBuilder>> classConstructors,
            Dictionary<ConstructorBuilder, List<Type>> constructorParamTypes)
        {
            IL = il;
            CurrentType = currentType;
            CurrentClass = currentClass;
            Classes = classes;
            BuiltinTypes = builtinTypes;
            ClassMethods = classMethods;
            MethodParamTypes = methodParamTypes;
            ClassConstructors = classConstructors;
            ConstructorParamTypes = constructorParamTypes;
        }

        public void RegisterLocal(string name, LocalBuilder local, Type realType = null)
        {
            _locals[name] = local;
            if (realType != null)
            {
                _localRealTypes[name] = realType;
            }
        }

        public bool TryGetLocal(string name, out LocalBuilder local)
        {
            return _locals.TryGetValue(name, out local);
        }

        public bool TryGetLocalRealType(string name, out Type realType)
        {
            return _localRealTypes.TryGetValue(name, out realType);
        }

        public void RegisterParameter(string name, int index, Type type, Type realType = null)
        {
            _parameters[name] = (index, type);
            if (realType != null)
            {
                _parameterRealTypes[name] = realType;
            }
        }

        public bool TryGetParameter(string name, out int index, out Type type)
        {
            if (_parameters.TryGetValue(name, out var param))
            {
                index = param.index;
                type = param.type;
                return true;
            }
            index = -1;
            type = typeof(object);
            return false;
        }

        public bool TryGetParameterRealType(string name, out Type realType)
        {
            return _parameterRealTypes.TryGetValue(name, out realType);
        }

        public void RegisterField(string name, FieldBuilder field, Type realType = null)
        {
            _fields[name] = field;
            if (realType != null)
            {
                _fieldRealTypes[name] = realType;
            }
        }

        public bool TryGetField(string name, out FieldBuilder field)
        {
            return _fields.TryGetValue(name, out field);
        }

        public bool TryGetFieldRealType(string name, out Type realType)
        {
            return _fieldRealTypes.TryGetValue(name, out realType);
        }

        public void RegisterArrayElementType(string arrayName, Type elementType)
        {
            _arrayElementTypes[arrayName] = elementType;
        }

        public bool TryGetArrayElementType(string arrayName, out Type elementType)
        {
            return _arrayElementTypes.TryGetValue(arrayName, out elementType);
        }

        public Type ResolveArrayElementType(string arrayTypeName)
        {
            // Извлекаем тип элемента из Array[ElementType]
            if (arrayTypeName.StartsWith("Array[") && arrayTypeName.EndsWith("]"))
            {
                var elementTypeName = arrayTypeName.Substring(6, arrayTypeName.Length - 7);
                return ResolveType(elementTypeName);
            }
            return typeof(object);
        }

        public Type ResolveType(string typeName)
        {
            // Проверяем, является ли это массивом
            if (typeName.StartsWith("Array[") && typeName.EndsWith("]"))
            {
                return typeof(object[]);
            }

            // Проверяем, является ли это списком
            if (typeName.StartsWith("List[") && typeName.EndsWith("]"))
            {
                return typeof(System.Collections.Generic.List<object>);
            }

            if (BuiltinTypes.TryGetValue(typeName, out var builtinType))
            {
                return builtinType;
            }

            if (Classes.TryGetValue(typeName, out var classType))
            {
                return classType;
            }

            return typeof(object);
        }

        public MethodBuilder FindMethod(TypeBuilder typeBuilder, string methodName, List<Type> argTypes)
        {
            if (ClassMethods.TryGetValue(typeBuilder.Name, out var methodsByName))
            {
                if (methodsByName.TryGetValue(methodName, out var overloads))
                {
                    // Ищем точное совпадение по количеству и типам параметров
                    foreach (var method in overloads)
                    {
                        if (MethodParamTypes.TryGetValue(method, out var paramTypes))
                        {
                            if (paramTypes.Count == argTypes.Count)
                            {
                                bool match = true;
                                for (int i = 0; i < paramTypes.Count; i++)
                                {
                                    var paramType = paramTypes[i];
                                    var argType = argTypes[i];
                                    
                                    if (!TypesAreCompatible(paramType, argType))
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                                if (match)
                                {
                                    return method;
                                }
                            }
                        }
                    }
                    
                    // Если точное совпадение не найдено, ищем совместимый метод
                    // (для случаев когда argType = object, а paramType = TypeBuilder)
                    foreach (var method in overloads)
                    {
                        if (MethodParamTypes.TryGetValue(method, out var paramTypes))
                        {
                            if (paramTypes.Count == argTypes.Count)
                            {
                                return method;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private bool TypesAreCompatible(Type paramType, Type argType)
        {
            if (paramType == argType) return true;
            
            // Для TypeBuilder сравниваем по имени
            if (paramType is TypeBuilder paramTypeBuilder && argType is TypeBuilder argTypeBuilder)
            {
                return paramTypeBuilder.Name == argTypeBuilder.Name;
            }
            
            // argType может быть object для локальной переменной, но реальный тип - TypeBuilder
            if (paramType is TypeBuilder && argType == typeof(object))
            {
                return true;
            }
            
            if (argType is TypeBuilder && paramType == typeof(object))
            {
                return true;
            }
            
            return false;
        }

        public ConstructorBuilder FindConstructor(TypeBuilder typeBuilder, List<Type> argTypes)
        {
            if (ClassConstructors.TryGetValue(typeBuilder.Name, out var constructors))
            {
                foreach (var ctor in constructors)
                {
                    if (ConstructorParamTypes.TryGetValue(ctor, out var paramTypes))
                    {
                        if (paramTypes.Count == argTypes.Count)
                        {
                            bool match = true;
                            for (int i = 0; i < paramTypes.Count; i++)
                            {
                                var paramType = paramTypes[i];
                                var argType = argTypes[i];
                                
                                if (!TypesAreCompatible(paramType, argType))
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                return ctor;
                            }
                        }
                    }
                }
                
                // Если точное совпадение не найдено, ищем по количеству параметров
                foreach (var ctor in constructors)
                {
                    if (ConstructorParamTypes.TryGetValue(ctor, out var paramTypes))
                    {
                        if (paramTypes.Count == argTypes.Count)
                        {
                            return ctor;
                        }
                    }
                }
            }
            return null;
        }
    }
}
