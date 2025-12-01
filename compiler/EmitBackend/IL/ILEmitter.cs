using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using O_Parser.AST;

namespace EmitBackend.IL
{
    public static class ILEmitter
    {
        public static void EmitBlock(BuildContext context, BlockNode block)
        {
            // Сначала объявляем все локальные переменные (для IL нужно объявить заранее)
            foreach (var local in block.Locals)
            {
                Type realType;
                
                // Проверяем, является ли Initializer просто именем типа (без инициализатора)
                if (local.Initializer is IdentifierExpr typeId)
                {
                    // Это объявление переменной без инициализатора: var x : Integer
                    // Определяем тип на основе имени типа
                    var typeName = typeId.Name;
                    if (typeName == "Integer")
                    {
                        realType = typeof(int);
                    }
                    else if (typeName == "Real")
                    {
                        realType = typeof(double);
                    }
                    else if (typeName == "Boolean")
                    {
                        realType = typeof(bool);
                    }
                    else if (typeName == "String")
                    {
                        realType = typeof(string);
                    }
                    else if (typeName.StartsWith("Array[") && typeName.EndsWith("]"))
                    {
                        realType = typeof(object[]);
                        var elementType = context.ResolveArrayElementType(typeName);
                        context.RegisterArrayElementType(local.Name, elementType);
                    }
                    else if (typeName.StartsWith("List[") && typeName.EndsWith("]"))
                    {
                        realType = typeof(System.Collections.Generic.List<object>);
                        var elementTypeName = typeName.Substring(5, typeName.Length - 6);
                        var elementType = context.ResolveType(elementTypeName);
                        context.RegisterArrayElementType(local.Name, elementType);
                    }
                    else
                    {
                        // Пользовательский тип
                        realType = context.ResolveType(typeName);
                    }
                }
                else
                {
                    // Переменная с инициализатором
                    realType = InferType(local.Initializer, context);
                    
                    // Для массивов сохраняем тип элемента
                    if (local.Initializer is NewExpr newExpr && 
                        newExpr.ClassName.StartsWith("Array[") && newExpr.ClassName.EndsWith("]"))
                    {
                        var elementType = context.ResolveArrayElementType(newExpr.ClassName);
                        context.RegisterArrayElementType(local.Name, elementType);
                    }
                    
                    // Для списков сохраняем тип элемента
                    if (local.Initializer is NewExpr listNewExpr && 
                        listNewExpr.ClassName.StartsWith("List[") && listNewExpr.ClassName.EndsWith("]"))
                    {
                        var elementTypeName = listNewExpr.ClassName.Substring(5, listNewExpr.ClassName.Length - 6);
                        var elementType = context.ResolveType(elementTypeName);
                        context.RegisterArrayElementType(local.Name, elementType);
                    }
                    
                    // Для вызова get() на массиве или head() на списке - используем тип элемента
                    if (local.Initializer is CallExpr callExpr && 
                        callExpr.Callee is MemberAccessExpr memberAccess)
                    {
                        if (memberAccess.Member == "get" || memberAccess.Member == "head")
                        {
                            // Проверяем, является ли target массивом или списком
                            if (memberAccess.Target is IdentifierExpr arrayId)
                            {
                                if (context.TryGetArrayElementType(arrayId.Name, out var elementType))
                                {
                                    realType = elementType;
                                }
                            }
                        }
                    }
                }
                
                // TypeBuilder нельзя использовать напрямую как тип локальной переменной
                // Используем object для пользовательских типов
                var localType = realType is TypeBuilder ? typeof(object) : realType;
                var localBuilder = context.IL.DeclareLocal(localType);
                context.RegisterLocal(local.Name, localBuilder, realType);
            }

            // Эмиссия тела блока с сохранением порядка (var и statements чередуются)
            foreach (var item in block.Body)
            {
                if (item is VarDeclNode local)
                {
                    // Инициализация локальной переменной
                    // Если Initializer является IdentifierExpr с именем типа, это объявление без инициализатора
                    if (local.Initializer is IdentifierExpr typeId)
                    {
                        // Переменная объявлена без инициализатора - не эмитим ничего
                        // Локальная переменная уже инициализирована значением по умолчанию (.locals init)
                    }
                    else
                    {
                        // Переменная с инициализатором
                        EmitExpression(context, local.Initializer);
                        if (context.TryGetLocal(local.Name, out var localBuilder))
                        {
                            context.IL.Emit(OpCodes.Stloc, localBuilder);
                        }
                    }
                }
                else if (item is StmtNode stmt)
                {
                    EmitStatement(context, stmt);
                }
            }
        }

        public static void EmitStatement(BuildContext context, StmtNode stmt)
        {
            switch (stmt)
            {
                case AssignStmt assign:
                    EmitAssignment(context, assign);
                    break;
                case ExprStmt expr:
                    EmitExpression(context, expr.Expr);
                    // Pop только если выражение возвращает значение (не void)
                    var exprType = InferType(expr.Expr, context);
                    if (exprType != typeof(void))
                    {
                        context.IL.Emit(OpCodes.Pop);
                    }
                    break;
                case ReturnStmt ret:
                    if (ret.Value != null)
                    {
                        EmitExpression(context, ret.Value);
                    }
                    context.IL.Emit(OpCodes.Ret);
                    break;
                case WhileStmt whileStmt:
                    EmitWhile(context, whileStmt);
                    break;
                case IfStmt ifStmt:
                    EmitIf(context, ifStmt);
                    break;
            }
        }

        private static void EmitAssignment(BuildContext context, AssignStmt assign)
        {
            if (context.TryGetLocal(assign.TargetName, out var local))
            {
                EmitExpression(context, assign.Value);
                context.IL.Emit(OpCodes.Stloc, local);
            }
            else if (context.TryGetParameter(assign.TargetName, out var paramIndex, out _))
            {
                EmitExpression(context, assign.Value);
                context.IL.Emit(OpCodes.Starg, paramIndex);
            }
            else if (context.TryGetField(assign.TargetName, out var field))
            {
                context.IL.Emit(OpCodes.Ldarg_0);
                EmitExpression(context, assign.Value);
                context.IL.Emit(OpCodes.Stfld, field);
            }
        }

        private static void EmitWhile(BuildContext context, WhileStmt whileStmt)
        {
            var startLabel = context.IL.DefineLabel();
            var endLabel = context.IL.DefineLabel();

            context.IL.MarkLabel(startLabel);
            EmitExpression(context, whileStmt.Condition);
            context.IL.Emit(OpCodes.Brfalse, endLabel);
            EmitBlock(context, whileStmt.Body);
            context.IL.Emit(OpCodes.Br, startLabel);
            context.IL.MarkLabel(endLabel);
        }

        private static void EmitIf(BuildContext context, IfStmt ifStmt)
        {
            var elseLabel = context.IL.DefineLabel();
            var endLabel = context.IL.DefineLabel();

            EmitExpression(context, ifStmt.Condition);
            context.IL.Emit(OpCodes.Brfalse, elseLabel);
            EmitBlock(context, ifStmt.ThenBody);
            
            // Проверяем, заканчивается ли then блок на return
            bool thenEndsWithReturn = ifStmt.ThenBody.Statements.Count > 0 &&
                ifStmt.ThenBody.Statements[ifStmt.ThenBody.Statements.Count - 1] is ReturnStmt;
            
            if (ifStmt.ElseBody != null && !thenEndsWithReturn)
            {
                context.IL.Emit(OpCodes.Br, endLabel);
            }
            context.IL.MarkLabel(elseLabel);
            if (ifStmt.ElseBody != null)
            {
                EmitBlock(context, ifStmt.ElseBody);
                context.IL.MarkLabel(endLabel);
            }
        }

        public static void EmitExpression(BuildContext context, ExprNode expr)
        {
            switch (expr)
            {
                case IntLiteral intLit:
                    EmitIntLiteral(context, intLit);
                    break;
                case RealLiteral realLit:
                    context.IL.Emit(OpCodes.Ldc_R8, realLit.Value);
                    break;
                case BoolLiteral boolLit:
                    context.IL.Emit(OpCodes.Ldc_I4, boolLit.Value ? 1 : 0);
                    break;
                case StringLiteral strLit:
                    context.IL.Emit(OpCodes.Ldstr, strLit.Value);
                    break;
                case ThisExpr _:
                    context.IL.Emit(OpCodes.Ldarg_0);
                    break;
                case IdentifierExpr id:
                    EmitIdentifier(context, id);
                    break;
                case NewExpr newExpr:
                    EmitNewExpression(context, newExpr);
                    break;
                case MemberAccessExpr memberAccess:
                    EmitMemberAccess(context, memberAccess);
                    break;
                case CallExpr call:
                    EmitCall(context, call);
                    break;
            }
        }

        private static void EmitIntLiteral(BuildContext context, IntLiteral intLit)
        {
            var value = (int)intLit.Value;
            if (value == 0)
            {
                context.IL.Emit(OpCodes.Ldc_I4_0);
            }
            else if (value == 1)
            {
                context.IL.Emit(OpCodes.Ldc_I4_1);
            }
            else if (value >= -128 && value <= 127)
            {
                context.IL.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
            }
            else
            {
                context.IL.Emit(OpCodes.Ldc_I4, value);
            }
        }

        private static void EmitIdentifier(BuildContext context, IdentifierExpr id)
        {
            if (context.TryGetLocal(id.Name, out var local))
            {
                context.IL.Emit(OpCodes.Ldloc, local);
            }
            else if (context.TryGetParameter(id.Name, out var paramIndex, out _))
            {
                context.IL.Emit(OpCodes.Ldarg, paramIndex);
            }
            else if (context.TryGetField(id.Name, out var field))
            {
                context.IL.Emit(OpCodes.Ldarg_0);
                context.IL.Emit(OpCodes.Ldfld, field);
            }
        }

        private static bool IsArrayType(string typeName, out string elementTypeName)
        {
            if (typeName.StartsWith("Array[") && typeName.EndsWith("]"))
            {
                elementTypeName = typeName.Substring(6, typeName.Length - 7);
                return true;
            }
            elementTypeName = null;
            return false;
        }

        private static bool IsListType(string typeName, out string elementTypeName)
        {
            if (typeName.StartsWith("List[") && typeName.EndsWith("]"))
            {
                elementTypeName = typeName.Substring(5, typeName.Length - 6);
                return true;
            }
            elementTypeName = null;
            return false;
        }

        private static void EmitNewExpression(BuildContext context, NewExpr newExpr)
        {
            // Проверяем, является ли это созданием массива
            if (IsArrayType(newExpr.ClassName, out var elementTypeName))
            {
                // Создаём массив object[] для поддержки полиморфизма
                // Array[Animal](2) -> new object[2]
                if (newExpr.Arguments.Count > 0)
                {
                    EmitExpression(context, newExpr.Arguments[0]);
                }
                else
                {
                    context.IL.Emit(OpCodes.Ldc_I4_0);
                }
                context.IL.Emit(OpCodes.Newarr, typeof(object));
                return;
            }

            // Проверяем, является ли это созданием списка
            if (IsListType(newExpr.ClassName, out var listElementTypeName))
            {
                var listType = typeof(System.Collections.Generic.List<object>);
                
                // Если есть аргумент, проверяем его тип
                if (newExpr.Arguments.Count > 0)
                {
                    var argType = InferType(newExpr.Arguments[0], context);
                    // Если аргумент уже является List, просто возвращаем его (для tail(), append() и т.д.)
                    if (argType == listType)
                    {
                        EmitExpression(context, newExpr.Arguments[0]);
                        return;
                    }
                }
                
                // Создаём List<object> для поддержки полиморфизма
                // List[Integer]() -> new List<object>()
                // List[Integer](5) -> new List<object>() с добавлением элемента
                var listCtor = listType.GetConstructor(Type.EmptyTypes);
                context.IL.Emit(OpCodes.Newobj, listCtor);
                
                // Если есть аргумент, добавляем его в список
                if (newExpr.Arguments.Count > 0)
                {
                    // Дублируем ссылку на список (для вызова Add)
                    context.IL.Emit(OpCodes.Dup);
                    EmitExpression(context, newExpr.Arguments[0]);
                    // Box если примитивный тип
                    var argType = InferType(newExpr.Arguments[0], context);
                    if (argType == typeof(int))
                    {
                        context.IL.Emit(OpCodes.Box, typeof(int));
                    }
                    else if (argType == typeof(double))
                    {
                        context.IL.Emit(OpCodes.Box, typeof(double));
                    }
                    else if (argType == typeof(bool))
                    {
                        context.IL.Emit(OpCodes.Box, typeof(bool));
                    }
                    var addMethod = listType.GetMethod("Add", new[] { typeof(object) });
                    context.IL.Emit(OpCodes.Callvirt, addMethod);
                }
                return;
            }

            var type = context.ResolveType(newExpr.ClassName);

            if (type == typeof(int))
            {
                if (newExpr.Arguments.Count > 0)
                {
                    EmitExpression(context, newExpr.Arguments[0]);
                    var argType = InferType(newExpr.Arguments[0], context);
                    if (argType == typeof(double))
                    {
                        context.IL.Emit(OpCodes.Conv_I4);
                    }
                }
                else
                {
                    context.IL.Emit(OpCodes.Ldc_I4_0);
                }
            }
            else if (type == typeof(double))
            {
                if (newExpr.Arguments.Count > 0)
                {
                    EmitExpression(context, newExpr.Arguments[0]);
                    var argType = InferType(newExpr.Arguments[0], context);
                    if (argType == typeof(int))
                    {
                        context.IL.Emit(OpCodes.Conv_R8);
                    }
                }
                else
                {
                    context.IL.Emit(OpCodes.Ldc_R8, 0.0);
                }
            }
            else if (type == typeof(bool))
            {
                if (newExpr.Arguments.Count > 0)
                {
                    EmitExpression(context, newExpr.Arguments[0]);
                }
                else
                {
                    context.IL.Emit(OpCodes.Ldc_I4_0);
                }
            }
            else if (type == typeof(string))
            {
                if (newExpr.Arguments.Count > 0)
                {
                    // String("value") - просто эмитим аргумент (он уже строка)
                    EmitExpression(context, newExpr.Arguments[0]);
                }
                else
                {
                    // String() - пустая строка
                    context.IL.Emit(OpCodes.Ldstr, "");
                }
            }
            else if (type is TypeBuilder typeBuilder)
            {
                // Создание объекта пользовательского класса
                // Загружаем аргументы конструктора
                var argTypes = new List<Type>();
                foreach (var arg in newExpr.Arguments)
                {
                    EmitExpression(context, arg);
                    argTypes.Add(InferType(arg, context));
                }
                
                // Находим конструктор с подходящими параметрами
                var ctor = context.FindConstructor(typeBuilder, argTypes);
                if (ctor != null)
                {
                    context.IL.Emit(OpCodes.Newobj, ctor);
                }
                else
                {
                    throw new InvalidOperationException($"Constructor with {newExpr.Arguments.Count} parameters not found in class '{typeBuilder.Name}'");
                }
            }
        }

        private static void EmitMemberAccess(BuildContext context, MemberAccessExpr memberAccess)
        {
            // Определяем тип target
            Type targetType;
            if (memberAccess.Target is IdentifierExpr targetId)
            {
                targetType = InferIdentifierType(targetId, context);
            }
            else
            {
                targetType = InferType(memberAccess.Target, context);
            }

            // Обработка Array.Length (без скобок)
            if ((targetType == typeof(object[]) || IsArrayRealType(memberAccess.Target, context)) && memberAccess.Member == "Length")
            {
                EmitExpression(context, memberAccess.Target);
                context.IL.Emit(OpCodes.Ldlen);
                context.IL.Emit(OpCodes.Conv_I4);
                return;
            }

            // Обработка List.Length (без скобок)
            if ((targetType == typeof(System.Collections.Generic.List<object>) || IsListRealType(memberAccess.Target, context)) && memberAccess.Member == "Length")
            {
                EmitExpression(context, memberAccess.Target);
                var listType = typeof(System.Collections.Generic.List<object>);
                var countProp = listType.GetProperty("Count");
                context.IL.Emit(OpCodes.Callvirt, countProp.GetGetMethod());
                return;
            }

            // Обработка List.head (без скобок) - возвращает первый элемент
            if ((targetType == typeof(System.Collections.Generic.List<object>) || IsListRealType(memberAccess.Target, context)) && memberAccess.Member == "head")
            {
                EmitExpression(context, memberAccess.Target);
                context.IL.Emit(OpCodes.Ldc_I4_0);  // индекс 0
                var listType = typeof(System.Collections.Generic.List<object>);
                var getItemMethod = listType.GetMethod("get_Item", new[] { typeof(int) });
                context.IL.Emit(OpCodes.Callvirt, getItemMethod);
                // Unbox если примитивный тип - нужно определить тип элемента
                if (memberAccess.Target is IdentifierExpr listIdHead)
                {
                    Type elementType = null;
                    if (context.TryGetArrayElementType(listIdHead.Name, out elementType) ||
                        context.TryGetParameterRealType(listIdHead.Name, out elementType))
                    {
                        if (elementType == typeof(int))
                        {
                            context.IL.Emit(OpCodes.Unbox_Any, typeof(int));
                        }
                        else if (elementType == typeof(double))
                        {
                            context.IL.Emit(OpCodes.Unbox_Any, typeof(double));
                        }
                        else if (elementType == typeof(bool))
                        {
                            context.IL.Emit(OpCodes.Unbox_Any, typeof(bool));
                        }
                    }
                }
                return;
            }

            // Обработка List.tail (без скобок) - возвращает список без первого элемента
            if ((targetType == typeof(System.Collections.Generic.List<object>) || IsListRealType(memberAccess.Target, context)) && memberAccess.Member == "tail")
            {
                EmitExpression(context, memberAccess.Target);  // список
                context.IL.Emit(OpCodes.Ldc_I4_1);  // начальный индекс = 1
                EmitExpression(context, memberAccess.Target);  // список (для Count)
                var listType = typeof(System.Collections.Generic.List<object>);
                var countProperty = listType.GetProperty("Count");
                context.IL.Emit(OpCodes.Callvirt, countProperty.GetGetMethod());
                context.IL.Emit(OpCodes.Ldc_I4_1);
                context.IL.Emit(OpCodes.Sub);  // Count - 1
                var getRangeMethod = listType.GetMethod("GetRange", new[] { typeof(int), typeof(int) });
                context.IL.Emit(OpCodes.Callvirt, getRangeMethod);
                return;
            }

            EmitExpression(context, memberAccess.Target);

            // Обработка builtin методов без аргументов (как свойства)
            if (targetType == typeof(int))
            {
                if (memberAccess.Member == "UnaryMinus")
                {
                    context.IL.Emit(OpCodes.Neg);
                    return;
                }
                else if (memberAccess.Member == "toReal")
                {
                    context.IL.Emit(OpCodes.Conv_R8);
                    return;
                }
                else if (memberAccess.Member == "toBoolean")
                {
                    // toBoolean: 0 -> false, non-zero -> true
                    context.IL.Emit(OpCodes.Ldc_I4_0);
                    context.IL.Emit(OpCodes.Cgt_Un);
                    return;
                }
            }
            else if (targetType == typeof(double))
            {
                if (memberAccess.Member == "UnaryMinus")
                {
                    context.IL.Emit(OpCodes.Neg);
                    return;
                }
                else if (memberAccess.Member == "toInteger")
                {
                    context.IL.Emit(OpCodes.Conv_I4);
                    return;
                }
            }
            else if (targetType == typeof(bool))
            {
                if (memberAccess.Member == "Not")
                {
                    context.IL.Emit(OpCodes.Ldc_I4_0);
                    context.IL.Emit(OpCodes.Ceq);
                    return;
                }
                else if (memberAccess.Member == "toInteger")
                {
                    // bool уже int32 в CLR
                    return;
                }
            }
            else if (targetType is TypeBuilder typeBuilder)
            {
                // Вызов метода без аргументов на пользовательском классе
                // Например: e.getId (без скобок, эквивалентно e.getId())
                var method = context.FindMethod(typeBuilder, memberAccess.Member, new List<Type>());
                if (method != null)
                {
                    context.IL.Emit(OpCodes.Callvirt, method);
                    return;
                }
                // Попробуем найти метод в базовом классе
                var baseMethod = FindMethodInHierarchy(context, typeBuilder, memberAccess.Member, new List<Type>());
                if (baseMethod != null)
                {
                    context.IL.Emit(OpCodes.Callvirt, baseMethod);
                    return;
                }
                // Если метод не найден, это доступ к полю
                // Ищем поле в текущем классе и базовых классах
                var field = context.FindField(typeBuilder, memberAccess.Member);
                if (field != null)
                {
                    context.IL.Emit(OpCodes.Ldfld, field);
                    return;
                }
                // Если поле не найдено, target уже на стеке
            }
            else if (targetType == typeof(object))
            {
                // Для object (например, локальная переменная из массива) - проверяем реальный тип
                Type realTargetType = null;
                if (memberAccess.Target is IdentifierExpr targetIdExpr)
                {
                    if (context.TryGetLocalRealType(targetIdExpr.Name, out var localRealType))
                    {
                        realTargetType = localRealType;
                    }
                    else if (context.TryGetFieldRealType(targetIdExpr.Name, out var fieldRealType))
                    {
                        realTargetType = fieldRealType;
                    }
                }
                
                if (realTargetType is TypeBuilder realTypeBuilder)
                {
                    var method = context.FindMethod(realTypeBuilder, memberAccess.Member, new List<Type>());
                    if (method != null)
                    {
                        context.IL.Emit(OpCodes.Callvirt, method);
                        return;
                    }
                    var baseMethod = FindMethodInHierarchy(context, realTypeBuilder, memberAccess.Member, new List<Type>());
                    if (baseMethod != null)
                    {
                        context.IL.Emit(OpCodes.Callvirt, baseMethod);
                        return;
                    }
                    // Если метод не найден, это доступ к полю
                    var field = context.FindField(realTypeBuilder, memberAccess.Member);
                    if (field != null)
                    {
                        context.IL.Emit(OpCodes.Ldfld, field);
                        return;
                    }
                }
                // Если не нашли метод или поле, оставляем target на стеке
            }
            // Для других случаев (доступ к полю пользовательского класса) - target уже на стеке
        }

        private static void EmitCall(BuildContext context, CallExpr call)
        {
            // Обработка встроенной функции print
            // print может быть как IdentifierExpr, так и MemberAccessExpr с Member = "print"
            bool isPrint = false;
            if (call.Callee is IdentifierExpr id && id.Name == "print")
            {
                isPrint = true;
            }
            else if (call.Callee is MemberAccessExpr printAccess && printAccess.Member == "print")
            {
                isPrint = true;
            }
            
            if (isPrint)
            {
                if (call.Arguments.Count == 1)
                {
                    EmitExpression(context, call.Arguments[0]);
                    var argType = InferType(call.Arguments[0], context);
                    
                    // Вызов Console.WriteLine с правильным типом
                    if (argType == typeof(int))
                    {
                        var writeLineMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(int) });
                        if (writeLineMethod != null)
                        {
                            context.IL.Emit(OpCodes.Call, writeLineMethod);
                        }
                    }
                    else if (argType == typeof(double))
                    {
                        var writeLineMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(double) });
                        if (writeLineMethod != null)
                        {
                            context.IL.Emit(OpCodes.Call, writeLineMethod);
                        }
                    }
                    else if (argType == typeof(bool))
                    {
                        var writeLineMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(bool) });
                        if (writeLineMethod != null)
                        {
                            context.IL.Emit(OpCodes.Call, writeLineMethod);
                        }
                    }
                    else if (argType == typeof(string))
                    {
                        var writeLineMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
                        if (writeLineMethod != null)
                        {
                            context.IL.Emit(OpCodes.Call, writeLineMethod);
                        }
                    }
                    else
                    {
                        // Для других типов используем object
                        var writeLineMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(object) });
                        if (writeLineMethod != null)
                        {
                            context.IL.Emit(OpCodes.Call, writeLineMethod);
                        }
                    }
                }
                // Console.WriteLine возвращает void, поэтому ничего не оставляем на стеке
                return;
            }
            
            if (call.Callee is MemberAccessExpr memberAccess)
            {
                // Проверяем, является ли это вызовом конструктора класса
                // Если Target - это ThisExpr и Member - это имя класса, то это конструктор
                if (memberAccess.Target is ThisExpr && context.Classes.ContainsKey(memberAccess.Member))
                {
                    // Это конструктор класса - обрабатываем как NewExpr
                    var typeBuilder = context.Classes[memberAccess.Member];
                    var argTypes = new List<Type>();
                    foreach (var arg in call.Arguments)
                    {
                        EmitExpression(context, arg);
                        argTypes.Add(InferType(arg, context));
                    }
                    
                    var ctor = context.FindConstructor(typeBuilder, argTypes);
                    if (ctor != null)
                    {
                        context.IL.Emit(OpCodes.Newobj, ctor);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Constructor with {call.Arguments.Count} parameters not found in class '{memberAccess.Member}'");
                    }
                    return;
                }
                
                // Определяем тип target'а до эмиссии
                Type targetType;
                if (memberAccess.Target is IdentifierExpr targetId)
                {
                    targetType = InferIdentifierType(targetId, context);
                }
                else
                {
                    targetType = InferType(memberAccess.Target, context);
                }

                // Проверяем, является ли это вызовом метода массива
                if (targetType == typeof(object[]) || IsArrayRealType(memberAccess.Target, context))
                {
                    EmitArrayMethodCall(context, memberAccess, call);
                    return;
                }

                // Проверяем, является ли это вызовом метода списка
                if (targetType == typeof(System.Collections.Generic.List<object>) || IsListRealType(memberAccess.Target, context))
                {
                    EmitListMethodCall(context, memberAccess, call);
                    return;
                }

                EmitExpression(context, memberAccess.Target);

                if (call.Arguments.Count > 0)
                {
                    var argType = InferType(call.Arguments[0], context);
                    if (targetType == typeof(int) && argType == typeof(double))
                    {
                        context.IL.Emit(OpCodes.Conv_R8);
                        EmitExpression(context, call.Arguments[0]);
                        EmitRealMethod(context, memberAccess.Member);
                        return;
                    }
                    else if (targetType == typeof(double) && argType == typeof(int))
                    {
                        EmitExpression(context, call.Arguments[0]);
                        context.IL.Emit(OpCodes.Conv_R8);
                        EmitRealMethod(context, memberAccess.Member);
                        return;
                    }
                    else
                    {
                        foreach (var arg in call.Arguments)
                        {
                            EmitExpression(context, arg);
                        }
                    }
                }

                if (targetType == typeof(int))
                {
                    if (memberAccess.Member == "toReal")
                    {
                        // toReal() не требует аргументов
                        if (call.Arguments.Count == 0)
                        {
                            context.IL.Emit(OpCodes.Conv_R8);
                        }
                        else
                        {
                            EmitIntegerMethod(context, memberAccess.Member);
                        }
                    }
                    else
                    {
                        EmitIntegerMethod(context, memberAccess.Member);
                    }
                }
                else if (targetType == typeof(double))
                {
                    EmitRealMethod(context, memberAccess.Member);
                }
                else if (targetType == typeof(bool))
                {
                    EmitBooleanMethod(context, memberAccess.Member);
                }
                else if (targetType is TypeBuilder typeBuilder)
                {
                    // Вызов метода на пользовательском классе
                    // Аргументы уже загружены в стек выше
                    var argTypes = new List<Type>();
                    foreach (var arg in call.Arguments)
                    {
                        argTypes.Add(InferType(arg, context));
                    }
                    var method = context.FindMethod(typeBuilder, memberAccess.Member, argTypes);
                    if (method != null)
                    {
                        context.IL.Emit(OpCodes.Callvirt, method);
                    }
                    else
                    {
                        // Попробуем найти метод в базовом классе
                        var baseMethod = FindMethodInHierarchy(context, typeBuilder, memberAccess.Member, argTypes);
                        if (baseMethod != null)
                        {
                            context.IL.Emit(OpCodes.Callvirt, baseMethod);
                        }
                        else
                        {
                            var argTypesStr = string.Join(", ", argTypes.Select(t => t.Name));
                            throw new InvalidOperationException($"Method '{memberAccess.Member}({argTypesStr})' not found in class '{typeBuilder.Name}'. Check method name and argument count/types.");
                        }
                    }
                }
                else if (targetType == typeof(object))
                {
                    // targetType может быть object для поля/локальной переменной пользовательского класса
                    // Попробуем найти реальный тип напрямую через TryGetFieldRealType или TryGetLocalRealType
                    Type realTargetType = null;
                    if (memberAccess.Target is IdentifierExpr targetIdExpr)
                    {
                        // Сначала проверяем локальные переменные (они более вероятны внутри методов)
                        if (context.TryGetLocalRealType(targetIdExpr.Name, out var localRealType))
                        {
                            realTargetType = localRealType;
                        }
                        // Затем проверяем поля
                        else if (context.TryGetFieldRealType(targetIdExpr.Name, out var fieldRealType))
                        {
                            realTargetType = fieldRealType;
                        }
                        // Если не нашли реальный тип, пробуем через InferIdentifierType
                        else
                        {
                            realTargetType = InferIdentifierType(targetIdExpr, context);
                        }
                    }
                    
                    if (realTargetType != null && realTargetType is TypeBuilder realTypeBuilder)
                    {
                        // Вызов метода на пользовательском классе
                        var argTypes = new List<Type>();
                        foreach (var arg in call.Arguments)
                        {
                            argTypes.Add(InferType(arg, context));
                        }
                        var method = context.FindMethod(realTypeBuilder, memberAccess.Member, argTypes);
                        if (method != null)
                        {
                            context.IL.Emit(OpCodes.Callvirt, method);
                        }
                        else
                        {
                            // Попробуем найти метод в базовом классе
                            var baseMethod = FindMethodInHierarchy(context, realTypeBuilder, memberAccess.Member, argTypes);
                            if (baseMethod != null)
                            {
                                context.IL.Emit(OpCodes.Callvirt, baseMethod);
                            }
                            else
                            {
                                throw new InvalidOperationException($"Method '{memberAccess.Member}' not found in class '{realTypeBuilder.Name}'");
                            }
                        }
                        return;
                    }
                    // Если не удалось определить тип, выбрасываем исключение
                    throw new InvalidOperationException($"Cannot call method '{memberAccess.Member}' on object type. Target: {memberAccess.Target}, realType: {realTargetType}");
                }
            }
            else if (call.Callee is IdentifierExpr identifierExpr)
            {
                // Проверяем, является ли это вызовом конструктора класса
                if (context.Classes.ContainsKey(identifierExpr.Name))
                {
                    // Это конструктор класса - обрабатываем как NewExpr
                    var typeBuilder = context.Classes[identifierExpr.Name];
                    var argTypes = new List<Type>();
                    foreach (var arg in call.Arguments)
                    {
                        EmitExpression(context, arg);
                        argTypes.Add(InferType(arg, context));
                    }
                    
                    var ctor = context.FindConstructor(typeBuilder, argTypes);
                    if (ctor != null)
                    {
                        context.IL.Emit(OpCodes.Newobj, ctor);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Constructor with {call.Arguments.Count} parameters not found in class '{identifierExpr.Name}'");
                    }
                }
                else
                {
                    // Вызов метода на this (неявный)
                    // Это должно быть обработано как вызов метода на текущем классе
                    // Загружаем this
                    context.IL.Emit(OpCodes.Ldarg_0);
                    // Загружаем аргументы
                    var argTypes = new List<Type>();
                    foreach (var arg in call.Arguments)
                    {
                        EmitExpression(context, arg);
                        argTypes.Add(InferType(arg, context));
                    }
                    var method = context.FindMethod(context.CurrentType, identifierExpr.Name, argTypes);
                    if (method != null)
                    {
                        context.IL.Emit(OpCodes.Callvirt, method);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Method '{identifierExpr.Name}' not found in class '{context.CurrentType.Name}'");
                    }
                }
            }
        }

        private static bool IsArrayRealType(ExprNode expr, BuildContext context)
        {
            if (expr is IdentifierExpr id)
            {
                if (context.TryGetLocalRealType(id.Name, out var localRealType))
                {
                    return localRealType == typeof(object[]);
                }
                if (context.TryGetFieldRealType(id.Name, out var fieldRealType))
                {
                    return fieldRealType == typeof(object[]);
                }
            }
            return false;
        }

        private static void EmitArrayMethodCall(BuildContext context, MemberAccessExpr memberAccess, CallExpr call)
        {
            switch (memberAccess.Member)
            {
                case "set":
                    // animals.set(Integer(0), dog)
                    // Стек: array, index, value -> вызов stelem.ref
                    EmitExpression(context, memberAccess.Target);  // массив
                    EmitExpression(context, call.Arguments[0]);     // индекс
                    EmitExpression(context, call.Arguments[1]);     // значение
                    // Для примитивных типов нужно сделать box перед stelem.ref
                    if (memberAccess.Target is IdentifierExpr arrayIdSet)
                    {
                        Type elementType = null;
                        if (context.TryGetArrayElementType(arrayIdSet.Name, out elementType) ||
                            context.TryGetParameterRealType(arrayIdSet.Name, out elementType))
                        {
                            if (elementType == typeof(int))
                            {
                                context.IL.Emit(OpCodes.Box, typeof(int));
                            }
                            else if (elementType == typeof(double))
                            {
                                context.IL.Emit(OpCodes.Box, typeof(double));
                            }
                            else if (elementType == typeof(bool))
                            {
                                context.IL.Emit(OpCodes.Box, typeof(bool));
                            }
                        }
                    }
                    context.IL.Emit(OpCodes.Stelem_Ref);
                    break;
                    
                case "get":
                    // animals.get(i)
                    // Стек: array, index -> ldelem.ref
                    EmitExpression(context, memberAccess.Target);  // массив
                    EmitExpression(context, call.Arguments[0]);     // индекс
                    context.IL.Emit(OpCodes.Ldelem_Ref);
                    // Для примитивных типов нужно сделать unbox
                    if (memberAccess.Target is IdentifierExpr arrayIdGet)
                    {
                        Type elementType = null;
                        if (context.TryGetArrayElementType(arrayIdGet.Name, out elementType) ||
                            context.TryGetParameterRealType(arrayIdGet.Name, out elementType))
                        {
                            if (elementType == typeof(int))
                            {
                                context.IL.Emit(OpCodes.Unbox_Any, typeof(int));
                            }
                            else if (elementType == typeof(double))
                            {
                                context.IL.Emit(OpCodes.Unbox_Any, typeof(double));
                            }
                            else if (elementType == typeof(bool))
                            {
                                context.IL.Emit(OpCodes.Unbox_Any, typeof(bool));
                            }
                        }
                    }
                    break;
                    
                case "Length":
                    // animals.Length()
                    EmitExpression(context, memberAccess.Target);  // массив
                    context.IL.Emit(OpCodes.Ldlen);
                    context.IL.Emit(OpCodes.Conv_I4);
                    break;
                    
                default:
                    throw new InvalidOperationException($"Unknown array method: {memberAccess.Member}");
            }
        }

        private static bool IsListRealType(ExprNode expr, BuildContext context)
        {
            if (expr is IdentifierExpr id)
            {
                if (context.TryGetLocalRealType(id.Name, out var localRealType))
                {
                    return localRealType == typeof(System.Collections.Generic.List<object>);
                }
                if (context.TryGetFieldRealType(id.Name, out var fieldRealType))
                {
                    return fieldRealType == typeof(System.Collections.Generic.List<object>);
                }
            }
            return false;
        }

        private static void EmitListMethodCall(BuildContext context, MemberAccessExpr memberAccess, CallExpr call)
        {
            var listType = typeof(System.Collections.Generic.List<object>);
            
            switch (memberAccess.Member)
            {
                case "append":
                    // list.append(value) : List
                    // Создаём новый список, копируем элементы, добавляем новый
                    // Для простоты: добавляем элемент в существующий список и возвращаем его
                    EmitExpression(context, memberAccess.Target);  // список
                    context.IL.Emit(OpCodes.Dup);  // дублируем ссылку для возврата
                    EmitExpression(context, call.Arguments[0]);     // значение
                    // Box если примитивный тип
                    var argType = InferType(call.Arguments[0], context);
                    if (argType == typeof(int))
                    {
                        context.IL.Emit(OpCodes.Box, typeof(int));
                    }
                    else if (argType == typeof(double))
                    {
                        context.IL.Emit(OpCodes.Box, typeof(double));
                    }
                    else if (argType == typeof(bool))
                    {
                        context.IL.Emit(OpCodes.Box, typeof(bool));
                    }
                    var addMethod = listType.GetMethod("Add", new[] { typeof(object) });
                    context.IL.Emit(OpCodes.Callvirt, addMethod);
                    // На стеке остаётся ссылка на список (от Dup)
                    break;
                    
                case "head":
                    // list.head() : T
                    // Возвращает первый элемент: list[0]
                    EmitExpression(context, memberAccess.Target);  // список
                    context.IL.Emit(OpCodes.Ldc_I4_0);  // индекс 0
                    var getItemMethod = listType.GetMethod("get_Item", new[] { typeof(int) });
                    context.IL.Emit(OpCodes.Callvirt, getItemMethod);
                    // Unbox если примитивный тип
                    if (memberAccess.Target is IdentifierExpr listIdHead)
                    {
                        Type elementType = null;
                        if (context.TryGetArrayElementType(listIdHead.Name, out elementType) ||
                            context.TryGetParameterRealType(listIdHead.Name, out elementType))
                        {
                            if (elementType == typeof(int))
                            {
                                context.IL.Emit(OpCodes.Unbox_Any, typeof(int));
                            }
                            else if (elementType == typeof(double))
                            {
                                context.IL.Emit(OpCodes.Unbox_Any, typeof(double));
                            }
                            else if (elementType == typeof(bool))
                            {
                                context.IL.Emit(OpCodes.Unbox_Any, typeof(bool));
                            }
                        }
                    }
                    break;
                    
                case "tail":
                    // list.tail() : List
                    // Возвращает список без первого элемента
                    // Создаём новый список GetRange(1, Count-1)
                    EmitExpression(context, memberAccess.Target);  // список
                    context.IL.Emit(OpCodes.Ldc_I4_1);  // начальный индекс = 1
                    EmitExpression(context, memberAccess.Target);  // список (для Count)
                    var countProperty = listType.GetProperty("Count");
                    context.IL.Emit(OpCodes.Callvirt, countProperty.GetGetMethod());
                    context.IL.Emit(OpCodes.Ldc_I4_1);
                    context.IL.Emit(OpCodes.Sub);  // Count - 1
                    var getRangeMethod = listType.GetMethod("GetRange", new[] { typeof(int), typeof(int) });
                    context.IL.Emit(OpCodes.Callvirt, getRangeMethod);
                    break;
                    
                case "Length":
                    // list.Length() : Integer
                    EmitExpression(context, memberAccess.Target);  // список
                    var countProp = listType.GetProperty("Count");
                    context.IL.Emit(OpCodes.Callvirt, countProp.GetGetMethod());
                    break;
                    
                case "get":
                    // list.get(index) : T
                    // Возвращает элемент по индексу
                    EmitExpression(context, memberAccess.Target);  // список
                    EmitExpression(context, call.Arguments[0]);     // индекс
                    var getItemMethodGet = listType.GetMethod("get_Item", new[] { typeof(int) });
                    context.IL.Emit(OpCodes.Callvirt, getItemMethodGet);
                    // Unbox если примитивный тип
                    if (memberAccess.Target is IdentifierExpr listIdGet)
                    {
                        Type elementType = null;
                        if (context.TryGetArrayElementType(listIdGet.Name, out elementType) ||
                            context.TryGetParameterRealType(listIdGet.Name, out elementType))
                        {
                            if (elementType == typeof(int))
                            {
                                context.IL.Emit(OpCodes.Unbox_Any, typeof(int));
                            }
                            else if (elementType == typeof(double))
                            {
                                context.IL.Emit(OpCodes.Unbox_Any, typeof(double));
                            }
                            else if (elementType == typeof(bool))
                            {
                                context.IL.Emit(OpCodes.Unbox_Any, typeof(bool));
                            }
                        }
                    }
                    break;
                    
                default:
                    throw new InvalidOperationException($"Unknown list method: {memberAccess.Member}");
            }
        }

        private static MethodBuilder FindMethodInHierarchy(BuildContext context, TypeBuilder typeBuilder, string methodName, List<Type> argTypes)
        {
            // Сначала ищем в текущем классе
            var method = context.FindMethod(typeBuilder, methodName, argTypes);
            if (method != null)
            {
                return method;
            }

            // Если не нашли, ищем в базовом классе
            var baseType = typeBuilder.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                if (baseType is TypeBuilder baseTypeBuilder)
                {
                    method = context.FindMethod(baseTypeBuilder, methodName, argTypes);
                    if (method != null)
                    {
                        return method;
                    }
                }
                baseType = baseType.BaseType;
            }
            
            return null;
        }

        private static void EmitIntegerMethod(BuildContext context, string methodName)
        {
            switch (methodName)
            {
                case "Plus":
                    context.IL.Emit(OpCodes.Add);
                    break;
                case "Minus":
                    context.IL.Emit(OpCodes.Sub);
                    break;
                case "Mult":
                    context.IL.Emit(OpCodes.Mul);
                    break;
                case "Div":
                    context.IL.Emit(OpCodes.Div);
                    break;
                case "Rem":
                    context.IL.Emit(OpCodes.Rem);
                    break;
                case "UnaryMinus":
                    context.IL.Emit(OpCodes.Neg);
                    break;
                case "Less":
                    context.IL.Emit(OpCodes.Clt);
                    break;
                case "LessEqual":
                    // a <= b эквивалентно !(a > b)
                    context.IL.Emit(OpCodes.Cgt);
                    context.IL.Emit(OpCodes.Ldc_I4_0);
                    context.IL.Emit(OpCodes.Ceq);
                    break;
                case "Greater":
                    context.IL.Emit(OpCodes.Cgt);
                    break;
                case "GreaterEqual":
                    // a >= b эквивалентно !(a < b)
                    context.IL.Emit(OpCodes.Clt);
                    context.IL.Emit(OpCodes.Ldc_I4_0);
                    context.IL.Emit(OpCodes.Ceq);
                    break;
                case "Equal":
                    context.IL.Emit(OpCodes.Ceq);
                    break;
            }
        }

        private static void EmitRealMethod(BuildContext context, string methodName)
        {
            switch (methodName)
            {
                case "Plus":
                    context.IL.Emit(OpCodes.Add);
                    break;
                case "Minus":
                    context.IL.Emit(OpCodes.Sub);
                    break;
                case "Mult":
                    context.IL.Emit(OpCodes.Mul);
                    break;
                case "Div":
                    context.IL.Emit(OpCodes.Div);
                    break;
                case "Less":
                    context.IL.Emit(OpCodes.Clt);
                    break;
                case "LessEqual":
                    // a <= b эквивалентно !(a > b)
                    context.IL.Emit(OpCodes.Cgt);
                    context.IL.Emit(OpCodes.Ldc_I4_0);
                    context.IL.Emit(OpCodes.Ceq);
                    break;
                case "Greater":
                    context.IL.Emit(OpCodes.Cgt);
                    break;
                case "GreaterEqual":
                    // a >= b эквивалентно !(a < b)
                    context.IL.Emit(OpCodes.Clt);
                    context.IL.Emit(OpCodes.Ldc_I4_0);
                    context.IL.Emit(OpCodes.Ceq);
                    break;
                case "Equal":
                    context.IL.Emit(OpCodes.Ceq);
                    break;
                case "toInteger":
                    context.IL.Emit(OpCodes.Conv_I4);
                    break;
            }
        }

        private static void EmitBooleanMethod(BuildContext context, string methodName)
        {
            switch (methodName)
            {
                case "And":
                    context.IL.Emit(OpCodes.And);
                    break;
                case "Or":
                    context.IL.Emit(OpCodes.Or);
                    break;
                case "Xor":
                    context.IL.Emit(OpCodes.Xor);
                    break;
                case "Not":
                    context.IL.Emit(OpCodes.Ldc_I4_0);
                    context.IL.Emit(OpCodes.Ceq);
                    break;
            }
        }

        private static bool IsComparisonMethod(string methodName)
        {
            return methodName == "Less" || 
                   methodName == "LessEqual" || 
                   methodName == "Greater" || 
                   methodName == "GreaterEqual" || 
                   methodName == "Equal";
        }

        public static Type InferType(ExprNode expr, BuildContext context)
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
            if (expr is ThisExpr)
            {
                return context.CurrentType;
            }
            if (expr is IdentifierExpr id)
            {
                return InferIdentifierType(id, context);
            }
            if (expr is NewExpr newExpr)
            {
                // Проверяем, является ли это созданием массива
                if (IsArrayType(newExpr.ClassName, out _))
                {
                    return typeof(object[]);
                }
                // Проверяем, является ли это созданием списка
                if (IsListType(newExpr.ClassName, out _))
                {
                    return typeof(System.Collections.Generic.List<object>);
                }
                return context.ResolveType(newExpr.ClassName);
            }
            if (expr is MemberAccessExpr memberAccess)
            {
                // Определяем тип target'а
                Type targetType;
                if (memberAccess.Target is IdentifierExpr targetId)
                {
                    targetType = InferIdentifierType(targetId, context);
                }
                else
                {
                    targetType = InferType(memberAccess.Target, context);
                }
                
                // Определяем тип результата на основе типа target и имени члена
                if (targetType == typeof(object[]))
                {
                    if (memberAccess.Member == "Length")
                    {
                        return typeof(int);
                    }
                }
                if (targetType == typeof(System.Collections.Generic.List<object>))
                {
                    if (memberAccess.Member == "Length" || memberAccess.Member == "Count")
                    {
                        return typeof(int);
                    }
                }
                if (targetType == typeof(int))
                {
                    if (memberAccess.Member == "toReal")
                    {
                        return typeof(double);
                    }
                    if (memberAccess.Member == "toBoolean")
                    {
                        return typeof(bool);
                    }
                    // UnaryMinus возвращает int
                    return typeof(int);
                }
                if (targetType == typeof(double))
                {
                    if (memberAccess.Member == "toInteger")
                    {
                        return typeof(int);
                    }
                    return typeof(double);
                }
                if (targetType == typeof(bool))
                {
                    if (memberAccess.Member == "toInteger")
                    {
                        return typeof(int);
                    }
                    return typeof(bool);
                }
                // Для пользовательских классов ищем тип возвращаемого значения метода
                if (targetType is TypeBuilder typeBuilder)
                {
                    var method = context.FindMethod(typeBuilder, memberAccess.Member, new List<Type>());
                    if (method != null)
                    {
                        return method.ReturnType;
                    }
                    var baseMethod = FindMethodInHierarchy(context, typeBuilder, memberAccess.Member, new List<Type>());
                    if (baseMethod != null)
                    {
                        return baseMethod.ReturnType;
                    }
                    // Возможно это поле - ищем тип поля
                    var field = context.FindField(typeBuilder, memberAccess.Member);
                    if (field != null)
                    {
                        return field.FieldType;
                    }
                    return targetType;
                }
                // Для object (локальная переменная из массива) проверяем реальный тип
                if (targetType == typeof(object))
                {
                    Type realTargetType = null;
                    if (memberAccess.Target is IdentifierExpr targetIdExpr)
                    {
                        if (context.TryGetLocalRealType(targetIdExpr.Name, out var localRealType))
                        {
                            realTargetType = localRealType;
                        }
                        else if (context.TryGetFieldRealType(targetIdExpr.Name, out var fieldRealType))
                        {
                            realTargetType = fieldRealType;
                        }
                    }
                    
                    if (realTargetType is TypeBuilder realTypeBuilder)
                    {
                        var method = context.FindMethod(realTypeBuilder, memberAccess.Member, new List<Type>());
                        if (method != null)
                        {
                            return method.ReturnType;
                        }
                        var baseMethod = FindMethodInHierarchy(context, realTypeBuilder, memberAccess.Member, new List<Type>());
                        if (baseMethod != null)
                        {
                            return baseMethod.ReturnType;
                        }
                        // Возможно это поле
                        var field = context.FindField(realTypeBuilder, memberAccess.Member);
                        if (field != null)
                        {
                            return field.FieldType;
                        }
                    }
                }
                return targetType;
            }
            if (expr is CallExpr call)
            {
                return InferCallType(call, context);
            }
            return typeof(object);
        }

        private static Type InferIdentifierType(IdentifierExpr id, BuildContext context)
        {
            if (context.TryGetLocal(id.Name, out var local))
            {
                // Если есть реальный тип (для пользовательских классов), используем его
                if (context.TryGetLocalRealType(id.Name, out var realType))
                {
                    return realType;
                }
                return local.LocalType;
            }
            if (context.TryGetParameter(id.Name, out _, out var paramType))
            {
                // Если есть реальный тип (для массивов), используем его
                if (context.TryGetParameterRealType(id.Name, out var realType))
                {
                    // Для массивов возвращаем object[], реальный тип элемента хранится отдельно
                    return paramType;
                }
                return paramType;
            }
            if (context.TryGetField(id.Name, out var field))
            {
                // Если есть реальный тип (для пользовательских классов), используем его
                if (context.TryGetFieldRealType(id.Name, out var realType))
                {
                    return realType;
                }
                return field.FieldType;
            }
            return typeof(object);
        }

        private static Type InferCallType(CallExpr call, BuildContext context)
        {
            // Обработка встроенной функции print
            bool isPrint = false;
            if (call.Callee is IdentifierExpr id && id.Name == "print")
            {
                isPrint = true;
            }
            else if (call.Callee is MemberAccessExpr printAccess && printAccess.Member == "print")
            {
                isPrint = true;
            }
            
            if (isPrint)
            {
                // print возвращает void
                return typeof(void);
            }
            
            if (call.Callee is MemberAccessExpr memberAccess)
            {
                // Проверяем, является ли это вызовом конструктора класса
                // Target = ThisExpr, Member = имя класса
                if (memberAccess.Target is ThisExpr && context.Classes.ContainsKey(memberAccess.Member))
                {
                    return context.Classes[memberAccess.Member];
                }

                // Проверяем методы массива
                var targetType = InferType(memberAccess.Target, context);
                if (targetType == typeof(object[]))
                {
                    switch (memberAccess.Member)
                    {
                        case "get":
                            // Попробуем получить тип элемента массива
                            if (memberAccess.Target is IdentifierExpr arrayId)
                            {
                                // Проверяем локальные переменные
                                if (context.TryGetArrayElementType(arrayId.Name, out var elementType))
                                {
                                    return elementType;
                                }
                                // Проверяем параметры
                                if (context.TryGetParameterRealType(arrayId.Name, out var paramElementType))
                                {
                                    return paramElementType;
                                }
                            }
                            return typeof(object);
                        case "Length":
                            return typeof(int);
                        case "set":
                            return typeof(void);
                    }
                }

                // Проверяем методы списка
                if (targetType == typeof(System.Collections.Generic.List<object>))
                {
                    switch (memberAccess.Member)
                    {
                        case "head":
                            // Попробуем получить тип элемента списка
                            if (memberAccess.Target is IdentifierExpr listId)
                            {
                                // Проверяем локальные переменные
                                if (context.TryGetArrayElementType(listId.Name, out var elementType))
                                {
                                    return elementType;
                                }
                                // Проверяем параметры
                                if (context.TryGetParameterRealType(listId.Name, out var paramElementType))
                                {
                                    return paramElementType;
                                }
                            }
                            return typeof(object);
                        case "tail":
                        case "append":
                            return typeof(System.Collections.Generic.List<object>);
                        case "Length":
                            return typeof(int);
                    }
                }
                
                if (targetType == typeof(int))
                {
                    if (memberAccess.Member == "toReal")
                    {
                        return typeof(double);
                    }
                    // Операции сравнения возвращают bool
                    if (IsComparisonMethod(memberAccess.Member))
                    {
                        return typeof(bool);
                    }
                    // Арифметические операции возвращают тот же тип
                    return targetType;
                }
                if (targetType == typeof(double))
                {
                    // toInteger возвращает int
                    if (memberAccess.Member == "toInteger")
                    {
                        return typeof(int);
                    }
                    // Операции сравнения возвращают bool
                    if (IsComparisonMethod(memberAccess.Member))
                    {
                        return typeof(bool);
                    }
                    // Арифметические операции возвращают тот же тип
                    return targetType;
                }
                if (targetType == typeof(bool))
                {
                    // Логические операции возвращают bool
                    return typeof(bool);
                }
                if (targetType is TypeBuilder typeBuilder)
                {
                    // Вызов метода на пользовательском классе
                    var argTypes = new List<Type>();
                    foreach (var arg in call.Arguments)
                    {
                        argTypes.Add(InferType(arg, context));
                    }
                    var method = context.FindMethod(typeBuilder, memberAccess.Member, argTypes);
                    if (method != null)
                    {
                        return method.ReturnType;
                    }
                    // Попробуем найти метод в базовом классе
                    var baseMethod = FindMethodInHierarchy(context, typeBuilder, memberAccess.Member, argTypes);
                    if (baseMethod != null)
                    {
                        return baseMethod.ReturnType;
                    }
                }
                else if (targetType == typeof(object))
                {
                    // targetType может быть object для поля/локальной переменной пользовательского класса
                    // Попробуем найти реальный тип
                    if (memberAccess.Target is IdentifierExpr targetId)
                    {
                        var realTargetType = InferIdentifierType(targetId, context);
                        if (realTargetType is TypeBuilder realTypeBuilder)
                        {
                            // Вызов метода на пользовательском классе
                            var argTypes = new List<Type>();
                            foreach (var arg in call.Arguments)
                            {
                                argTypes.Add(InferType(arg, context));
                            }
                            var method = context.FindMethod(realTypeBuilder, memberAccess.Member, argTypes);
                            if (method != null)
                            {
                                return method.ReturnType;
                            }
                            // Попробуем найти метод в базовом классе
                            var baseMethod = FindMethodInHierarchy(context, realTypeBuilder, memberAccess.Member, argTypes);
                            if (baseMethod != null)
                            {
                                return baseMethod.ReturnType;
                            }
                        }
                    }
                }
            }
            else if (call.Callee is IdentifierExpr identifierExpr)
            {
                // Проверяем, является ли это вызовом конструктора класса
                if (context.Classes.ContainsKey(identifierExpr.Name))
                {
                    return context.Classes[identifierExpr.Name];
                }
                
                // Вызов метода на this (неявный)
                var argTypes = new List<Type>();
                foreach (var arg in call.Arguments)
                {
                    argTypes.Add(InferType(arg, context));
                }
                var method = context.FindMethod(context.CurrentType, identifierExpr.Name, argTypes);
                if (method != null)
                {
                    return method.ReturnType;
                }
            }
            return typeof(object);
        }
    }
}
