using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using O_Parser.AST;
using EmitBackend.IL;
using EmitBackend.Utilities;

namespace EmitBackend
{
    public class Compiler
    {
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _moduleBuilder;
        private readonly Dictionary<string, TypeBuilder> _classes = new Dictionary<string, TypeBuilder>();
        private readonly Dictionary<string, Type> _builtinTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, Dictionary<string, FieldBuilder>> _classFields = new Dictionary<string, Dictionary<string, FieldBuilder>>();
        private readonly Dictionary<string, Dictionary<string, Type>> _classFieldRealTypes = new Dictionary<string, Dictionary<string, Type>>();
        // Изменено: теперь хранит список методов для каждого имени (поддержка перегрузки)
        private readonly Dictionary<string, Dictionary<string, List<MethodBuilder>>> _classMethods = new Dictionary<string, Dictionary<string, List<MethodBuilder>>>();
        private readonly Dictionary<MethodBuilder, List<Type>> _methodParamTypes = new Dictionary<MethodBuilder, List<Type>>();
        private readonly Dictionary<string, List<ConstructorBuilder>> _classConstructors = new Dictionary<string, List<ConstructorBuilder>>();
        private readonly Dictionary<ConstructorBuilder, List<Type>> _constructorParamTypes = new Dictionary<ConstructorBuilder, List<Type>>();
        private TypeBuilder _programTypeBuilder = null;

        public Compiler(string assemblyName)
        {
            var assemblyNameObj = new AssemblyName(assemblyName);
#if NETFRAMEWORK
            _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                assemblyNameObj,
                AssemblyBuilderAccess.Save);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(
                $"{assemblyName}.exe",
                $"{assemblyName}.exe");
#else
            // В .NET Core/.NET 5+ используем Run для генерации IL, но сохранение не поддерживается напрямую
            // Для анализа IL будем использовать только net48
            throw new NotSupportedException("Saving assemblies is only supported on .NET Framework. Please use net48 target framework.");
#endif

            InitializeBuiltinTypes();
        }

        private void InitializeBuiltinTypes()
        {
            _builtinTypes["Integer"] = typeof(int);
            _builtinTypes["Real"] = typeof(double);
            _builtinTypes["Boolean"] = typeof(bool);
            _builtinTypes["String"] = typeof(string);
        }

        public void Compile(ProgramNode program, string outputPath, string entryClass = null)
        {
            // Создаем все типы классов
            foreach (var cls in program.Classes)
            {
                CreateClassType(cls);
            }

            // Эмитим содержимое классов
            foreach (var cls in program.Classes)
            {
                EmitClass(cls);
            }

            // Создаем все типы классов
            var createdTypes = new Dictionary<string, Type>();
            foreach (var cls in program.Classes)
            {
                var createdType = _classes[cls.Name].CreateType();
                createdTypes[cls.Name] = createdType;
                Logger.Info($"Created type: {createdType.Name}");
            }

            // Создаем entry point ДО создания entry point типа
            var className = entryClass != null ? entryClass : program.Classes[0].Name;
            CreateEntryPoint(program, entryClass);

            // Создаем entry point тип и устанавливаем entry point
            if (_programTypeBuilder != null)
            {
                try
                {
                    var programType = _programTypeBuilder.CreateType();
                    var mainMethod = programType.GetMethod("Main", 
                        BindingFlags.Public | BindingFlags.Static);
                    if (mainMethod != null)
                    {
#if NETFRAMEWORK
                        _assemblyBuilder.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication);
#else
                        // В .NET Core SetEntryPoint не поддерживается
                        throw new NotSupportedException("Setting entry point is only supported on .NET Framework.");
#endif
                    }
                    else
                    {
                        Logger.Warning("Main method not found in entry point type");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to create entry point type: {ex.Message}");
                    throw;
                }
            }

#if NETFRAMEWORK
            var fileName = Path.GetFileName($"{outputPath}.exe");
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            _assemblyBuilder.Save(fileName);
            // Перемещаем файл в нужную директорию, если указан путь
            if (!string.IsNullOrEmpty(directory))
            {
                var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                var targetPath = Path.Combine(directory, fileName);
                if (File.Exists(sourcePath) && sourcePath != targetPath)
                {
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }
                    File.Move(sourcePath, targetPath);
                }
            }
#else
            throw new NotSupportedException("Saving assemblies is only supported on .NET Framework.");
#endif
            Logger.Success($"Compilation successful. Output: {outputPath}.exe");
        }

        private void CreateClassType(ClassDeclNode cls)
        {
            var baseType = cls.BaseName != null && _classes.ContainsKey(cls.BaseName)
                ? _classes[cls.BaseName]
                : typeof(object);

            var typeBuilder = _moduleBuilder.DefineType(
                cls.Name,
                TypeAttributes.Public | TypeAttributes.Class,
                baseType);

            _classes[cls.Name] = typeBuilder;
        }

        private void EmitClass(ClassDeclNode cls)
        {
            var typeBuilder = _classes[cls.Name];

            // Сначала создаем все поля
            foreach (var member in cls.Members)
            {
                if (member is VarDeclNode field)
                {
                    EmitField(typeBuilder, field);
                }
            }

            // Сначала создаем все методы (только MethodBuilder, без тела)
            // Это нужно, чтобы методы были доступны при эмиссии конструкторов
            foreach (var member in cls.Members)
            {
                if (member is MethodDeclNode method)
                {
                    CreateMethodBuilder(typeBuilder, cls, method);
                }
            }

            // Затем создаем конструкторы (они могут вызывать методы)
            bool hasDefaultCtor = false;
            foreach (var member in cls.Members)
            {
                if (member is CtorDeclNode ctor)
                {
                    if (ctor.Parameters.Count == 0)
                    {
                        hasDefaultCtor = true;
                    }
                    EmitConstructor(typeBuilder, cls, ctor);
                }
            }

            // Если нет конструктора без параметров, создаем его
            if (!hasDefaultCtor)
            {
                EmitDefaultConstructor(typeBuilder, cls);
            }

            // Теперь эмитим тела методов (они могут вызывать другие методы)
            foreach (var member in cls.Members)
            {
                if (member is MethodDeclNode method)
                {
                    EmitMethodBody(typeBuilder, cls, method);
                }
            }
        }

        private void EmitField(TypeBuilder typeBuilder, VarDeclNode field)
        {
            var realType = InferType(field.Initializer);
            
            // Если realType равен object, попробуем найти класс напрямую
            if (realType == typeof(object))
            {
                // Проверяем NewExpr
                if (field.Initializer is NewExpr newExpr)
                {
                    // Проверяем массив
                    if (newExpr.ClassName.StartsWith("Array[") && newExpr.ClassName.EndsWith("]"))
                    {
                        realType = typeof(object[]);
                    }
                    else if (_classes.TryGetValue(newExpr.ClassName, out var classTypeBuilder))
                    {
                        realType = classTypeBuilder;
                    }
                }
                // Проверяем CallExpr (конструктор класса)
                else if (field.Initializer is CallExpr callExpr)
                {
                    if (callExpr.Callee is IdentifierExpr calleeId)
                    {
                        if (_classes.TryGetValue(calleeId.Name, out var classTypeBuilder))
                        {
                            realType = classTypeBuilder;
                        }
                    }
                    else if (callExpr.Callee is MemberAccessExpr memberAccess)
                    {
                        // Если Target - это ThisExpr и Member - это имя класса
                        if (memberAccess.Target is ThisExpr && _classes.TryGetValue(memberAccess.Member, out var classTypeBuilder2))
                        {
                            realType = classTypeBuilder2;
                        }
                    }
                }
                // Проверяем IdentifierExpr для типов Array[T] и List[T]
                else if (field.Initializer is IdentifierExpr idExpr)
                {
                    if (idExpr.Name.StartsWith("Array[") && idExpr.Name.EndsWith("]"))
                    {
                        realType = typeof(object[]);
                    }
                    else if (idExpr.Name.StartsWith("List[") && idExpr.Name.EndsWith("]"))
                    {
                        realType = typeof(System.Collections.Generic.List<object>);
                    }
                }
            }
            
            // TypeBuilder нельзя использовать напрямую как тип поля
            // Используем object для пользовательских типов (кроме массивов)
            Type fieldType;
            if (realType is TypeBuilder)
            {
                fieldType = typeof(object);
            }
            else if (realType == typeof(object[]))
            {
                fieldType = typeof(object[]);
            }
            else
            {
                fieldType = realType;
            }
            
            var fieldBuilder = typeBuilder.DefineField(
                field.Name,
                fieldType,
                FieldAttributes.Public);

            if (!_classFields.ContainsKey(typeBuilder.Name))
            {
                _classFields[typeBuilder.Name] = new Dictionary<string, FieldBuilder>();
                _classFieldRealTypes[typeBuilder.Name] = new Dictionary<string, Type>();
            }
            _classFields[typeBuilder.Name][field.Name] = fieldBuilder;
            _classFieldRealTypes[typeBuilder.Name][field.Name] = realType;
        }

        private void RegisterFieldsInContext(TypeBuilder typeBuilder, ClassDeclNode cls, BuildContext context)
        {
            // Сначала регистрируем поля базовых классов (для наследования)
            RegisterBaseClassFields(typeBuilder, context);
            
            // Затем регистрируем поля текущего класса
            if (_classFields.TryGetValue(typeBuilder.Name, out var fields))
            {
                foreach (var field in fields)
                {
                    Type realType = null;
                    if (_classFieldRealTypes.TryGetValue(typeBuilder.Name, out var realTypes))
                    {
                        realTypes.TryGetValue(field.Key, out realType);
                    }
                    
                    // Если realType не найден или равен object, попробуем определить из AST
                    if (realType == null || realType == typeof(object))
                    {
                        var fieldNode = cls.Members.OfType<VarDeclNode>().FirstOrDefault(f => f.Name == field.Key);
                        if (fieldNode != null)
                        {
                            if (fieldNode.Initializer is NewExpr newExpr)
                            {
                                if (_classes.TryGetValue(newExpr.ClassName, out var classTypeBuilder))
                                {
                                    realType = classTypeBuilder;
                                }
                                else
                                {
                                    realType = ResolveType(newExpr.ClassName);
                                }
                            }
                            else if (fieldNode.Initializer is CallExpr callExpr)
                            {
                                // Проверяем CallExpr как конструктор
                                if (callExpr.Callee is IdentifierExpr calleeId && _classes.TryGetValue(calleeId.Name, out var classTypeBuilder2))
                                {
                                    realType = classTypeBuilder2;
                                }
                                else if (callExpr.Callee is MemberAccessExpr memberAccess && memberAccess.Target is ThisExpr && _classes.TryGetValue(memberAccess.Member, out var classTypeBuilder3))
                                {
                                    realType = classTypeBuilder3;
                                }
                            }
                        }
                    }
                    
                    context.RegisterField(field.Key, field.Value, realType);
                    
                    // Для массивов регистрируем тип элемента
                    if (realType == typeof(object[]))
                    {
                        var fieldNode = cls.Members.OfType<VarDeclNode>().FirstOrDefault(f => f.Name == field.Key);
                        if (fieldNode != null)
                        {
                            string arrayTypeName = null;
                            if (fieldNode.Initializer is NewExpr newExpr && newExpr.ClassName.StartsWith("Array[") && newExpr.ClassName.EndsWith("]"))
                            {
                                arrayTypeName = newExpr.ClassName;
                            }
                            else if (fieldNode.Initializer is IdentifierExpr idExpr && idExpr.Name.StartsWith("Array[") && idExpr.Name.EndsWith("]"))
                            {
                                arrayTypeName = idExpr.Name;
                            }
                            
                            if (arrayTypeName != null)
                            {
                                var elementTypeName = arrayTypeName.Substring(6, arrayTypeName.Length - 7);
                                var elementType = ResolveType(elementTypeName);
                                context.RegisterArrayElementType(field.Key, elementType);
                            }
                        }
                    }
                    // Для списков регистрируем тип элемента
                    else if (realType == typeof(System.Collections.Generic.List<object>))
                    {
                        var fieldNode = cls.Members.OfType<VarDeclNode>().FirstOrDefault(f => f.Name == field.Key);
                        if (fieldNode != null)
                        {
                            string listTypeName = null;
                            if (fieldNode.Initializer is NewExpr newExpr && newExpr.ClassName.StartsWith("List[") && newExpr.ClassName.EndsWith("]"))
                            {
                                listTypeName = newExpr.ClassName;
                            }
                            else if (fieldNode.Initializer is IdentifierExpr idExpr && idExpr.Name.StartsWith("List[") && idExpr.Name.EndsWith("]"))
                            {
                                listTypeName = idExpr.Name;
                            }
                            
                            if (listTypeName != null)
                            {
                                var elementTypeName = listTypeName.Substring(5, listTypeName.Length - 6);
                                var elementType = ResolveType(elementTypeName);
                                context.RegisterArrayElementType(field.Key, elementType);
                            }
                        }
                    }
                }
            }
        }

        private void RegisterBaseClassFields(TypeBuilder typeBuilder, BuildContext context)
        {
            var baseType = typeBuilder.BaseType;
            
            // Рекурсивно регистрируем поля всех базовых классов
            while (baseType != null && baseType != typeof(object))
            {
                if (baseType is TypeBuilder baseTypeBuilder)
                {
                    if (_classFields.TryGetValue(baseTypeBuilder.Name, out var baseFields))
                    {
                        foreach (var field in baseFields)
                        {
                            Type realType = null;
                            if (_classFieldRealTypes.TryGetValue(baseTypeBuilder.Name, out var realTypes))
                            {
                                realTypes.TryGetValue(field.Key, out realType);
                            }
                            
                            // Регистрируем поле базового класса (не перезаписываем существующие)
                            context.RegisterField(field.Key, field.Value, realType);
                        }
                    }
                    baseType = baseTypeBuilder.BaseType;
                }
                else
                {
                    break;
                }
            }
        }

        private void EmitConstructor(TypeBuilder typeBuilder, ClassDeclNode cls, CtorDeclNode ctor)
        {
            var paramTypes = ctor.Parameters.Select(p => ResolveType(p.TypeName)).ToList();

            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                paramTypes.ToArray());
            
            RegisterConstructor(typeBuilder.Name, ctorBuilder, paramTypes);

            var il = ctorBuilder.GetILGenerator();
            var context = new BuildContext(il, typeBuilder, cls, _classes, _builtinTypes, _classMethods, _methodParamTypes, _classConstructors, _constructorParamTypes, _classFields);

            RegisterFieldsInContext(typeBuilder, cls, context);

            // Регистрация параметров
            for (int i = 0; i < ctor.Parameters.Count; i++)
            {
                var paramTypeName = ctor.Parameters[i].TypeName;
                Type realType = null;
                // Для массивов сохраняем тип элемента
                if (paramTypeName.StartsWith("Array[") && paramTypeName.EndsWith("]"))
                {
                    var elementTypeName = paramTypeName.Substring(6, paramTypeName.Length - 7);
                    realType = ResolveType(elementTypeName);
                    context.RegisterArrayElementType(ctor.Parameters[i].Name, realType);
                }
                // Для списков сохраняем тип элемента
                else if (paramTypeName.StartsWith("List[") && paramTypeName.EndsWith("]"))
                {
                    var elementTypeName = paramTypeName.Substring(5, paramTypeName.Length - 6);
                    realType = ResolveType(elementTypeName);
                    context.RegisterArrayElementType(ctor.Parameters[i].Name, realType);
                }
                context.RegisterParameter(ctor.Parameters[i].Name, i + 1, paramTypes[i], realType);
            }

            // Если есть базовый класс, пытаемся найти базовый конструктор с такими же параметрами
            List<Type> baseCtorParamTypes = paramTypes;
            if (cls.BaseName != null && paramTypes.Count > 0)
            {
                // Проверяем, есть ли базовый конструктор с такими же параметрами
                if (_classes.ContainsKey(cls.BaseName))
                {
                    var baseTypeBuilder = _classes[cls.BaseName];
                    var baseCtor = FindBaseConstructor(baseTypeBuilder.Name, paramTypes);
                    if (baseCtor == null)
                    {
                        // Если не найден, используем дефолтный
                        baseCtorParamTypes = null;
                    }
                }
            }
            else
            {
                baseCtorParamTypes = null;
            }
            
            EmitBaseConstructorCall(il, typeBuilder, baseCtorParamTypes);
            EmitFieldInitializers(il, typeBuilder, cls, context);

            if (ctor.Body != null)
            {
                ILEmitter.EmitBlock(context, ctor.Body);
            }

            il.Emit(OpCodes.Ret);
        }

        private void EmitDefaultConstructor(TypeBuilder typeBuilder, ClassDeclNode cls)
        {
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);
            
            RegisterConstructor(typeBuilder.Name, ctorBuilder, new List<Type>());

            var il = ctorBuilder.GetILGenerator();
            var context = new BuildContext(il, typeBuilder, cls, _classes, _builtinTypes, _classMethods, _methodParamTypes, _classConstructors, _constructorParamTypes, _classFields);

            RegisterFieldsInContext(typeBuilder, cls, context);
            EmitBaseConstructorCall(il, typeBuilder, null);
            EmitFieldInitializers(il, typeBuilder, cls, context);

            il.Emit(OpCodes.Ret);
        }

        private bool IsMethodOverride(Type baseType, string methodName, List<Type> paramTypes)
        {
            if (baseType == null || baseType == typeof(object))
            {
                return false;
            }

            // Проверяем базовые классы рекурсивно
            while (baseType != null && baseType != typeof(object))
            {
                if (baseType is TypeBuilder baseTypeBuilder)
                {
                    // Проверяем в наших зарегистрированных методах
                    if (_classMethods.TryGetValue(baseTypeBuilder.Name, out var methodsByName) &&
                        methodsByName.TryGetValue(methodName, out var overloads))
                    {
                        foreach (var method in overloads)
                        {
                            if (_methodParamTypes.TryGetValue(method, out var baseParamTypes))
                            {
                                if (baseParamTypes.Count == paramTypes.Count)
                                {
                                    bool match = true;
                                    for (int i = 0; i < paramTypes.Count; i++)
                                    {
                                        if (!TypesMatch(baseParamTypes[i], paramTypes[i]))
                                        {
                                            match = false;
                                            break;
                                        }
                                    }
                                    if (match)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    baseType = baseTypeBuilder.BaseType;
                }
                else
                {
                    // Для обычных типов используем reflection
                    var methods = baseType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var m in methods)
                    {
                        if (m.Name == methodName && m.IsVirtual)
                        {
                            var mParams = m.GetParameters();
                            if (mParams.Length == paramTypes.Count)
                            {
                                bool match = true;
                                for (int i = 0; i < paramTypes.Count; i++)
                                {
                                    if (mParams[i].ParameterType != paramTypes[i])
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                                if (match)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    baseType = baseType.BaseType;
                }
            }

            return false;
        }

        private void RegisterConstructor(string className, ConstructorBuilder ctorBuilder, List<Type> paramTypes)
        {
            if (!_classConstructors.ContainsKey(className))
            {
                _classConstructors[className] = new List<ConstructorBuilder>();
            }
            _classConstructors[className].Add(ctorBuilder);
            _constructorParamTypes[ctorBuilder] = paramTypes;
        }

        private void EmitBaseConstructorCall(ILGenerator il, TypeBuilder typeBuilder, List<Type> paramTypes = null)
        {
            il.Emit(OpCodes.Ldarg_0);
            
            if (typeBuilder.BaseType == typeof(object))
            {
                var baseCtor = typeof(object).GetConstructor(Type.EmptyTypes);
                if (baseCtor != null)
                {
                    il.Emit(OpCodes.Call, baseCtor);
                }
            }
            else if (typeBuilder.BaseType is TypeBuilder baseTypeBuilder)
            {
                ConstructorBuilder baseCtor = null;
                if (paramTypes != null && paramTypes.Count > 0)
                {
                    baseCtor = FindBaseConstructor(baseTypeBuilder.Name, paramTypes);
                }
                if (baseCtor == null)
                {
                    baseCtor = FindDefaultConstructor(baseTypeBuilder.Name);
                }
                if (baseCtor != null)
                {
                    // Загружаем параметры на стек перед вызовом только если они есть
                    if (paramTypes != null && paramTypes.Count > 0)
                    {
                        for (int i = 0; i < paramTypes.Count; i++)
                        {
                            il.Emit(OpCodes.Ldarg, i + 1);
                        }
                    }
                    il.Emit(OpCodes.Call, baseCtor);
                }
                else
                {
                    var objCtor = typeof(object).GetConstructor(Type.EmptyTypes);
                    if (objCtor != null)
                    {
                        il.Emit(OpCodes.Call, objCtor);
                    }
                }
            }
            else
            {
                ConstructorInfo baseCtor = null;
                if (paramTypes != null && paramTypes.Count > 0)
                {
                    baseCtor = typeBuilder.BaseType.GetConstructor(paramTypes.ToArray());
                }
                if (baseCtor == null)
                {
                    baseCtor = typeBuilder.BaseType.GetConstructor(Type.EmptyTypes);
                }
                if (baseCtor != null)
                {
                    if (paramTypes != null && paramTypes.Count > 0)
                    {
                        for (int i = 0; i < paramTypes.Count; i++)
                        {
                            il.Emit(OpCodes.Ldarg, i + 1);
                        }
                    }
                    il.Emit(OpCodes.Call, baseCtor);
                }
            }
        }
        
        
        private ConstructorBuilder FindBaseConstructor(string className, List<Type> paramTypes)
        {
            if (_classConstructors.TryGetValue(className, out var constructors))
            {
                foreach (var ctor in constructors)
                {
                    if (_constructorParamTypes.TryGetValue(ctor, out var ctorParamTypes) && 
                        ctorParamTypes.Count == paramTypes.Count)
                    {
                        bool match = true;
                        for (int i = 0; i < paramTypes.Count; i++)
                        {
                            if (!TypesMatch(ctorParamTypes[i], paramTypes[i]))
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
            return null;
        }

        private ConstructorBuilder FindDefaultConstructor(string className)
        {
            if (_classConstructors.TryGetValue(className, out var constructors))
            {
                foreach (var ctor in constructors)
                {
                    if (_constructorParamTypes.TryGetValue(ctor, out var paramTypes) && paramTypes.Count == 0)
                    {
                        return ctor;
                    }
                }
            }
            return null;
        }

        private void EmitFieldInitializers(ILGenerator il, TypeBuilder typeBuilder, ClassDeclNode cls, BuildContext context)
        {
            if (_classFields.TryGetValue(typeBuilder.Name, out var fields))
            {
                foreach (var member in cls.Members)
                {
                    if (member is VarDeclNode field && fields.TryGetValue(field.Name, out var fieldBuilder))
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        
                        // Проверяем, является ли инициализатор только именем типа (без вызова конструктора)
                        // В этом случае нужно инициализировать поле значением по умолчанию
                        if (field.Initializer is IdentifierExpr typeId && 
                            (typeId.Name.StartsWith("Array[") || typeId.Name.StartsWith("List[") || 
                             _classes.ContainsKey(typeId.Name) || 
                             typeId.Name == "Integer" || typeId.Name == "Real" || 
                             typeId.Name == "Boolean" || typeId.Name == "String"))
                        {
                            // Это имя типа без конструктора - генерируем значение по умолчанию
                            if (typeId.Name == "Integer")
                            {
                                il.Emit(OpCodes.Ldc_I4_0);
                            }
                            else if (typeId.Name == "Real")
                            {
                                il.Emit(OpCodes.Ldc_R8, 0.0);
                            }
                            else if (typeId.Name == "Boolean")
                            {
                                il.Emit(OpCodes.Ldc_I4_0);
                            }
                            else if (typeId.Name == "String")
                            {
                                il.Emit(OpCodes.Ldnull);
                            }
                            else
                            {
                                // Для массивов, списков и пользовательских классов - null
                                il.Emit(OpCodes.Ldnull);
                            }
                        }
                        else
                        {
                            ILEmitter.EmitExpression(context, field.Initializer);
                        }
                        il.Emit(OpCodes.Stfld, fieldBuilder);
                    }
                }
            }
        }

        private void CreateMethodBuilder(TypeBuilder typeBuilder, ClassDeclNode cls, MethodDeclNode method)
        {
            var paramTypes = new List<Type>();
            foreach (var param in method.Parameters)
            {
                paramTypes.Add(ResolveType(param.TypeName));
            }

            var returnType = method.ReturnType != null
                ? ResolveType(method.ReturnType)
                : typeof(void);

            // Определяем атрибуты метода - все методы виртуальные для поддержки полиморфизма
            var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;
            
            // Проверяем, есть ли метод с таким же именем и сигнатурой в базовом классе
            bool isOverride = IsMethodOverride(typeBuilder.BaseType, method.Name, paramTypes);
            if (!isOverride)
            {
                // Новый виртуальный метод - добавляем NewSlot
                methodAttributes |= MethodAttributes.NewSlot;
            }

            var methodBuilder = typeBuilder.DefineMethod(
                method.Name,
                methodAttributes,
                returnType,
                paramTypes.ToArray());
            
            // Сохраняем метод - теперь в списке для поддержки перегрузки
            if (!_classMethods.ContainsKey(typeBuilder.Name))
            {
                _classMethods[typeBuilder.Name] = new Dictionary<string, List<MethodBuilder>>();
            }
            if (!_classMethods[typeBuilder.Name].ContainsKey(method.Name))
            {
                _classMethods[typeBuilder.Name][method.Name] = new List<MethodBuilder>();
            }
            _classMethods[typeBuilder.Name][method.Name].Add(methodBuilder);
            _methodParamTypes[methodBuilder] = paramTypes;
        }

        private void EmitMethodBody(TypeBuilder typeBuilder, ClassDeclNode cls, MethodDeclNode method)
        {
            // Получаем уже созданный MethodBuilder по типам параметров
            var paramTypes = method.Parameters.Select(p => ResolveType(p.TypeName)).ToList();
            
            MethodBuilder methodBuilder = null;
            if (_classMethods.TryGetValue(typeBuilder.Name, out var methodsByName) && 
                methodsByName.TryGetValue(method.Name, out var overloads))
            {
                // Находим метод с соответствующими параметрами
                foreach (var mb in overloads)
                {
                    if (_methodParamTypes.TryGetValue(mb, out var mbParamTypes))
                    {
                        if (mbParamTypes.Count == paramTypes.Count)
                        {
                            bool match = true;
                            for (int i = 0; i < paramTypes.Count; i++)
                            {
                                if (!TypesMatch(mbParamTypes[i], paramTypes[i]))
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                methodBuilder = mb;
                                break;
                            }
                        }
                    }
                }
            }
            
            if (methodBuilder == null)
            {
                throw new InvalidOperationException($"Method '{method.Name}' with matching parameters not found in class '{typeBuilder.Name}'");
            }

            var returnType = method.ReturnType != null
                ? ResolveType(method.ReturnType)
                : typeof(void);

            var il = methodBuilder.GetILGenerator();
            var context = new BuildContext(il, typeBuilder, cls, _classes, _builtinTypes, _classMethods, _methodParamTypes, _classConstructors, _constructorParamTypes, _classFields);

            // Регистрация полей класса
            RegisterFieldsInContext(typeBuilder, cls, context);

            // Регистрация параметров
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var paramTypeName = method.Parameters[i].TypeName;
                var paramType = ResolveType(paramTypeName);
                Type realElementType = null;
                // Для массивов сохраняем тип элемента
                if (paramTypeName.StartsWith("Array[") && paramTypeName.EndsWith("]"))
                {
                    var elementTypeName = paramTypeName.Substring(6, paramTypeName.Length - 7);
                    realElementType = ResolveType(elementTypeName);
                    context.RegisterArrayElementType(method.Parameters[i].Name, realElementType);
                }
                // Для списков сохраняем тип элемента
                else if (paramTypeName.StartsWith("List[") && paramTypeName.EndsWith("]"))
                {
                    var elementTypeName = paramTypeName.Substring(5, paramTypeName.Length - 6);
                    realElementType = ResolveType(elementTypeName);
                    context.RegisterArrayElementType(method.Parameters[i].Name, realElementType);
                }
                context.RegisterParameter(method.Parameters[i].Name, i + 1, paramType, realElementType);
            }

            if (method.IsArrowBody && method.ArrowExpr != null)
            {
                ILEmitter.EmitExpression(context, method.ArrowExpr);
                il.Emit(OpCodes.Ret);
            }
            else if (method.Body != null)
            {
                ILEmitter.EmitBlock(context, method.Body);
                if (returnType == typeof(void))
                {
                    il.Emit(OpCodes.Ret);
                }
            }
        }

        private bool TypesMatch(Type t1, Type t2)
        {
            if (t1 == t2) return true;
            if (t1 is TypeBuilder tb1 && t2 is TypeBuilder tb2)
            {
                return tb1.Name == tb2.Name;
            }
            return false;
        }

        private void CreateEntryPoint(ProgramNode program, string entryClass)
        {
            // Определяем класс для entry point
            var className = entryClass != null ? entryClass : program.Classes[0].Name;
            
            // Создаем отдельный тип для entry point (Main метод должен быть статическим)
            // Используем уникальное имя с пространством имен, чтобы избежать конфликта
            var entryTypeName = "<EntryPoint>";
            
            // Проверяем, не создан ли уже такой тип
            if (_programTypeBuilder != null)
            {
                throw new InvalidOperationException("Entry point type already created");
            }
            
            try
            {
                _programTypeBuilder = _moduleBuilder.DefineType(
                    entryTypeName,
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed,
                    typeof(object));
            }
            catch (ArgumentException ex)
            {
                Logger.Error($"Failed to create entry point type '{entryTypeName}': {ex.Message}");
                // Попробуем с другим именем
                entryTypeName = $"EntryPoint_{DateTime.Now.Ticks}";
                _programTypeBuilder = _moduleBuilder.DefineType(
                    entryTypeName,
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed,
                    typeof(object));
            }

            var mainMethod = _programTypeBuilder.DefineMethod(
                "Main",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(void),
                new[] { typeof(string[]) });

            var il = mainMethod.GetILGenerator();

            // Создаем экземпляр entry класса
            if (_classes.TryGetValue(className, out var classTypeBuilder))
            {
                // Ищем конструктор без параметров
                ConstructorBuilder defaultCtor = null;
                if (_classConstructors.TryGetValue(className, out var constructors))
                {
                    // Проверяем конструкторы по количеству параметров из AST
                    var targetClass = program.Classes.FirstOrDefault(c => c.Name == className);
                    if (targetClass != null)
                    {
                        // Проверяем, есть ли явный конструктор без параметров
                        var defaultCtorNode = targetClass.Members.OfType<CtorDeclNode>()
                            .FirstOrDefault(c => c.Parameters.Count == 0);
                        
                        if (defaultCtorNode != null)
                        {
                            // Найдем соответствующий ConstructorBuilder
                            var ctorIndex = targetClass.Members.OfType<CtorDeclNode>()
                                .TakeWhile(c => c != defaultCtorNode).Count();
                            if (ctorIndex < constructors.Count)
                            {
                                defaultCtor = constructors[ctorIndex];
                            }
                        }
                        else
                        {
                            // Если нет явного конструктора, должен быть создан дефолтный
                            // Дефолтный конструктор всегда первый в списке, если нет явных
                            if (constructors.Count > 0)
                            {
                                // Проверяем, есть ли явные конструкторы
                                var explicitCtors = targetClass.Members.OfType<CtorDeclNode>().ToList();
                                if (explicitCtors.Count == 0)
                                {
                                    // Нет явных конструкторов, значит первый - дефолтный
                                    defaultCtor = constructors[0];
                                }
                                else
                                {
                                    // Есть явные конструкторы, но нет без параметров
                                    // Значит дефолтный должен быть создан после всех явных
                                    defaultCtor = constructors[explicitCtors.Count];
                                }
                            }
                        }
                    }
                }
                
                if (defaultCtor == null)
                {
                    Logger.Warning($"Constructor without parameters not found for class {className}");
                    il.Emit(OpCodes.Ret);
                    return;
                }
                
                // Создаем экземпляр класса
                il.Emit(OpCodes.Newobj, defaultCtor);
                
                // Вызываем метод main
                MethodBuilder mainMethodBuilder = null;
                if (_classMethods.TryGetValue(className, out var methodsByName) &&
                    methodsByName.TryGetValue("main", out var mainOverloads) &&
                    mainOverloads.Count > 0)
                {
                    // Берем первый main без параметров
                    foreach (var mb in mainOverloads)
                    {
                        if (_methodParamTypes.TryGetValue(mb, out var paramTypes) && paramTypes.Count == 0)
                        {
                            mainMethodBuilder = mb;
                            break;
                        }
                    }
                    // Если не нашли без параметров, берем первый
                    if (mainMethodBuilder == null)
                    {
                        mainMethodBuilder = mainOverloads[0];
                    }
                }
                
                if (mainMethodBuilder == null)
                {
                    Logger.Warning($"Method 'main' not found in class {className}");
                    il.Emit(OpCodes.Pop);
                }
                else
                {
                    il.Emit(OpCodes.Callvirt, mainMethodBuilder);
                    // Если main возвращает не void, нужно удалить результат со стека
                    if (mainMethodBuilder.ReturnType != typeof(void))
                    {
                        il.Emit(OpCodes.Pop);
                    }
                }
            }
            else
            {
                Logger.Warning($"Class {className} not found");
            }

            il.Emit(OpCodes.Ret);
        }

        private Type ResolveType(string typeName)
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
            
            if (_builtinTypes.TryGetValue(typeName, out var type))
            {
                return type;
            }
            if (_classes.TryGetValue(typeName, out var classType))
            {
                return classType;
            }
            return typeof(object);
        }

        private Type InferType(ExprNode expr)
        {
            if (expr is IntLiteral)
            {
                return typeof(int);
            }
            if (expr is RealLiteral)
            {
                return typeof(double);
            }
            if (expr is BoolLiteral)
            {
                return typeof(bool);
            }
            if (expr is StringLiteral)
            {
                return typeof(string);
            }
            if (expr is NewExpr newExpr)
            {
                // Проверяем, является ли это массивом
                if (newExpr.ClassName.StartsWith("Array[") && newExpr.ClassName.EndsWith("]"))
                {
                    return typeof(object[]);
                }
                return ResolveType(newExpr.ClassName);
            }
            return typeof(object);
        }
    }
}
