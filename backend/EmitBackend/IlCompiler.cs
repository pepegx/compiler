using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace EmitBackend
{
    public static class IlCompiler
    {
        public static void Compile(IProgramNode program, string outPath, string startClassName, bool makeExe)
        {
            var asmName = new AssemblyName(Path.GetFileNameWithoutExtension(outPath) ?? "OProgram");
#if NET48 || NET481 || NET472 || NET461
            var access = AssemblyBuilderAccess.RunAndSave;
#else
            var access = AssemblyBuilderAccess.Run;
#endif
            var ab = AssemblyBuilder.DefineDynamicAssembly(asmName, access);
#if NET48 || NET481 || NET472 || NET461
            var mb = ab.DefineDynamicModule(asmName.Name, Path.GetFileName(outPath));
#else
            var mb = ab.DefineDynamicModule(asmName.Name);
#endif

            var ctx = new BuildContext(mb);

            foreach (var c in program.Classes)
            {
                var baseType = c.BaseName == null ? typeof(object) : ctx.ResolveOrCreatePlaceholder(c.BaseName);
                var tb = mb.DefineType("OUser." + c.Name, TypeAttributes.Public | TypeAttributes.Class, baseType);
                ctx.RegisterType(c.Name, tb);
            }

            foreach (var c in program.Classes)
            {
                var tb = ctx.GetTypeBuilder(c.Name);

                foreach (var f in c.Fields)
                {
                    var ft = ctx.MapType(f.TypeName);
                    var fb = tb.DefineField(f.Name, ft, FieldAttributes.Public);
                    ctx.RegisterField(c.Name, f.Name, fb);
                }

                int ctorCount = 0;

                foreach (var k in c.Ctors)
                {
                    var map = MapParams(ctx, k.Params);
                    var pNames = map.names;
                    var pTypes = map.types;

                    var cb = tb.DefineConstructor(
                        MethodAttributes.Public | MethodAttributes.HideBySig |
                        MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                        CallingConventions.Standard, pTypes);

                    for (int i = 0; i < pNames.Count; i++)
                        cb.DefineParameter(i + 1, ParameterAttributes.None, pNames[i]);

                    ctx.RegisterCtor(c.Name, cb, pTypes);
                    ctorCount++;
                }

                if (ctorCount == 0)
                {
#if NET48 || NET481 || NET472 || NET461
                    var dflt = tb.DefineDefaultConstructor(MethodAttributes.Public);
                    ctx.RegisterCtor(c.Name, dflt, Type.EmptyTypes);
#else
                    var cb = tb.DefineConstructor(
                        MethodAttributes.Public | MethodAttributes.HideBySig |
                        MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                        CallingConventions.Standard, Type.EmptyTypes);
                    var ilc = cb.GetILGenerator();
                    ilc.Emit(OpCodes.Ldarg_0);
                    var baseCtor = tb.BaseType.GetConstructor(Type.EmptyTypes);
                    if (baseCtor == null) throw new InvalidOperationException("Base type must have parameterless .ctor()");
                    ilc.Emit(OpCodes.Call, baseCtor);
                    ilc.Emit(OpCodes.Ret);
                    ctx.RegisterCtor(c.Name, cb, Type.EmptyTypes);
#endif
                }

                foreach (var m in c.Methods)
                {
                    if (m.Body == null && !m.IsExpressionBodied) continue;

                    var map = MapParams(ctx, m.Params);
                    var pNames = map.names;
                    var pTypes = map.types;
                    var ret = m.ReturnType == null ? typeof(void) : ctx.MapType(m.ReturnType);

                    var attrs = MethodAttributes.Public | MethodAttributes.HideBySig;
                    if (m.IsOverride) attrs |= MethodAttributes.Virtual | MethodAttributes.ReuseSlot;
                    else if (m.IsVirtual) attrs |= MethodAttributes.Virtual | MethodAttributes.NewSlot;

                    var mbuilder = tb.DefineMethod(m.Name, attrs, ret, pTypes);
                    for (int i = 0; i < pNames.Count; i++)
                        mbuilder.DefineParameter(i + 1, ParameterAttributes.None, pNames[i]);

                    ctx.RegisterMethod(c.Name, m.Name, mbuilder, pTypes, ret);
                }
            }

            foreach (var c in program.Classes)
            {
                var tb = ctx.GetTypeBuilder(c.Name);

                foreach (var k in c.Ctors)
                {
                    var sig = k.Params.Select(p => ctx.MapType(p.type)).ToArray();
                    var ctor = ctx.GetCtor(c.Name, sig);
                    if (ctor == null) throw new InvalidOperationException("Ctor not registered for " + c.Name);

                    var il = ctor.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    var baseCtor = tb.BaseType.GetConstructor(Type.EmptyTypes);
                    if (baseCtor == null) throw new InvalidOperationException("Base type must have parameterless .ctor()");
                    il.Emit(OpCodes.Call, baseCtor);

                    var env = new MethodEnv(ctx, tb, il, k.Params, null);
                    EmitBody(env, k.Body);

                    il.Emit(OpCodes.Ret);
                }

                foreach (var m in c.Methods)
                {
                    if (m.Body == null && !m.IsExpressionBodied) continue;

                    var sig = m.Params.Select(p => ctx.MapType(p.type)).ToArray();
                    var meth = ctx.GetMethod(c.Name, m.Name, sig);
                    if (meth == null) throw new InvalidOperationException("Method not registered: " + c.Name + "." + m.Name);

                    var il = meth.GetILGenerator();
                    var env = new MethodEnv(ctx, tb, il, m.Params, meth);

                    if (m.IsExpressionBodied && m.ExprBody != null)
                    {
                        EmitExpr(env, m.ExprBody);
                        il.Emit(OpCodes.Ret);
                    }
                    else
                    {
                        EmitBody(env, m.Body);
                        il.Emit(OpCodes.Ret);
                    }

                    if (m.IsOverride && tb.BaseType != null)
                    {
                        var baseSig = m.Params.Select(p => ctx.MapType(p.type)).ToArray();
                        var baseMethod = tb.BaseType.GetMethod(m.Name, baseSig);
                        if (baseMethod != null)
                            tb.DefineMethodOverride(meth, TypeBuilder.GetMethod(tb.BaseType, baseMethod));
                    }
                }
            }

            if (makeExe)
            {
                var boot = mb.DefineType("OUser.__Bootstrap", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
                var main = boot.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(int), new Type[] { typeof(string[]) });
                var il = main.GetILGenerator();

                if (!ctx.HasType(startClassName))
                {
                    var first = program.Classes.First().Name;
                    Console.Error.WriteLine("[Emit] warning: start class '" + startClassName + "' not found; using '" + first + "'.");
                    startClassName = first;
                }

                var startTb = ctx.GetTypeBuilder(startClassName);
                var objLocal = il.DeclareLocal(startTb);

                ConstructorBuilder p0;
                if (ctx.TryGetCtor(startClassName, Type.EmptyTypes, out p0) && p0 != null)
                {
                    il.Emit(OpCodes.Newobj, p0);
                    il.Emit(OpCodes.Stloc, objLocal);
                }
                else
                {
                    ConstructorBuilder bestCtor;
                    Type[] bestSig;
                    if (ctx.TryGetCheapestCtor(startClassName, out bestCtor, out bestSig))
                    {
                        for (int i = 0; i < bestSig.Length; i++)
                            EmitDefaultFor(il, bestSig[i], ctx, 2);
                        il.Emit(OpCodes.Newobj, bestCtor);
                        il.Emit(OpCodes.Stloc, objLocal);
                    }
                    else
                    {
                        Console.Error.WriteLine("[Emit] warning: start class '" + startClassName + "' has no constructors; returning 0.");
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ret);
                    }
                }

                var mbMain =
                    ctx.FindUserMethodCI(startClassName, "main", Type.EmptyTypes) ??
                    ctx.FindUserMethodCI(startClassName, "run",  Type.EmptyTypes);

                if (mbMain != null)
                {
                    il.Emit(OpCodes.Ldloc, objLocal);
                    il.Emit(OpCodes.Callvirt, mbMain);

                    if (mbMain.ReturnType == typeof(int))
                    {
                        il.Emit(OpCodes.Ret);
                    }
                    else if (mbMain.ReturnType == typeof(bool))
                    {
                        il.Emit(OpCodes.Conv_I4);
                        il.Emit(OpCodes.Ret);
                    }
                    else if (mbMain.ReturnType == typeof(void))
                    {
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ret);
                    }
                    else
                    {
                        il.Emit(OpCodes.Pop);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ret);
                    }
                }
                else
                {
                    Console.Error.WriteLine("[Emit] warning: '" + startClassName + "' has no parameterless main()/run(). Returning 0.");
                    try
                    {
                        var userType = startTb.CreateType();
                        var userMethods = userType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                        foreach (var um in userMethods)
                        {
                            if (um.IsSpecialName) continue;
                            Console.Error.WriteLine("         found: " + um.ReturnType.Name + " " + um.Name + "(" +
                                string.Join(", ", Array.ConvertAll(um.GetParameters(), p => p.ParameterType.Name)) + ")");
                        }
                    }
                    catch { }

                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ret);
                }

                boot.CreateType();
#if NET48 || NET481 || NET472 || NET461
                ab.SetEntryPoint(main, PEFileKinds.ConsoleApplication);
#endif
            }

            foreach (var c in program.Classes)
                ctx.GetTypeBuilder(c.Name).CreateType();

#if NET48 || NET481 || NET472 || NET461
            {
                var fullOut  = Path.GetFullPath(outPath);
                var finalDir = Path.GetDirectoryName(fullOut) ?? ".";
                var finalName = Path.GetFileName(fullOut);
                Directory.CreateDirectory(finalDir);

                var cwd = Environment.CurrentDirectory;
                var tmpPath = Path.Combine(cwd, finalName);

                ab.Save(finalName);

                var destPath = fullOut;
                try
                {
                    if (!string.Equals(tmpPath, destPath, StringComparison.OrdinalIgnoreCase))
                    {
                        if (File.Exists(destPath)) File.Delete(destPath);
                        File.Move(tmpPath, destPath);
                    }
                    Console.Error.WriteLine("[Emit] OK → " + outPath);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("[Emit] Saved to '" + tmpPath + "', but moving to '" + destPath + "' failed: " + ex.Message);
                }
            }
#else
            Console.Error.WriteLine("[Emit] Note: saving dynamic assemblies to disk is not supported on this runtime (net5+). Build with -f net48 for a file.");
#endif
        }

        private static void EmitBody(MethodEnv env, IBody body)
        {
            if (body == null) return;

            foreach (var v in body.Locals)
            {
                var lt = env.Ctx.MapType(v.TypeName);
                var lbLocal = env.IL.DeclareLocal(lt);
                env.Locals[v.Name] = lbLocal;

                EmitExpr(env, v.Init);
                env.IL.Emit(OpCodes.Stloc, lbLocal);
            }

            foreach (var s in body.Statements)
                EmitStmt(env, s);
        }

        private static void EmitStmt(MethodEnv env, IStatement st)
        {
            if (st is IAssignment)
            {
                var a = (IAssignment)st;
                if (a.Target != null && a.Target.StartsWith("this."))
                {
                    var field = a.Target.Substring("this.".Length);
                    env.IL.Emit(OpCodes.Ldarg_0);
                    EmitExpr(env, a.Value);
                    var fb = env.Ctx.GetFieldCascade(env.UserClassName, field);
                    env.IL.Emit(OpCodes.Stfld, fb);
                }
                else if (a.Target != null && env.Locals.TryGetValue(a.Target, out var tmpLocal))
                {
                    EmitExpr(env, a.Value);
                    env.IL.Emit(OpCodes.Stloc, tmpLocal);
                }
                else
                {
                    var fb = env.Ctx.GetFieldCascade(env.UserClassName, a.Target);
                    env.IL.Emit(OpCodes.Ldarg_0);
                    EmitExpr(env, a.Value);
                    env.IL.Emit(OpCodes.Stfld, fb);
                }
                return;
            }

            if (st is IWhile)
            {
                var w = (IWhile)st;
                var Lcond = env.IL.DefineLabel();
                var Lbody = env.IL.DefineLabel();
                var Lend  = env.IL.DefineLabel();
                env.IL.MarkLabel(Lcond);
                EmitExpr(env, w.Cond);
                env.IL.Emit(OpCodes.Brtrue_S, Lbody);
                env.IL.Emit(OpCodes.Br_S, Lend);
                env.IL.MarkLabel(Lbody);
                EmitBody(env, w.Body);
                env.IL.Emit(OpCodes.Br_S, Lcond);
                env.IL.MarkLabel(Lend);
                return;
            }

            if (st is IIf)
            {
                var iff = (IIf)st;
                var Lelse = env.IL.DefineLabel();
                var Lafter= env.IL.DefineLabel();
                EmitExpr(env, iff.Cond);
                env.IL.Emit(OpCodes.Brfalse_S, Lelse);
                EmitBody(env, iff.Then);
                env.IL.Emit(OpCodes.Br_S, Lafter);
                env.IL.MarkLabel(Lelse);
                if (iff.Else != null) EmitBody(env, iff.Else);
                env.IL.MarkLabel(Lafter);
                return;
            }

            if (st is IReturn)
            {
                var r = (IReturn)st;
                if (r.Expr != null) EmitExpr(env, r.Expr);
                env.IL.Emit(OpCodes.Ret);
                return;
            }

            throw new NotSupportedException("Stmt " + st.GetType().Name);
        }

        private static void EmitExpr(MethodEnv env, IExpression e)
        {
            var kind = (e as MyAstAdapter.ExprA)?.Kind ?? string.Empty;

            if (e is MyAstAdapter.ExprA ex)
            {
                if (kind.Contains("literal"))
                {
                    if (kind.Contains("int"))
                    {
                        env.IL.Emit(OpCodes.Ldc_I4, ex.Value);
                        return;
                    }
                    if (kind.Contains("real") || kind.Contains("double"))
                    {
                        env.IL.Emit(OpCodes.Ldc_R8, ((ILiteralReal)ex).Value);
                        return;
                    }
                    if (kind.Contains("bool"))
                    {
                        env.IL.Emit(((ILiteralBool)ex).Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                        return;
                    }
                }
            }

            if (kind.Contains("new") || kind.Contains("ctor"))
            {
                if (TryEmitPrimitiveCtorLike(env, (INew)e))
                    return;
            }

            if (kind.Contains("this"))
            {
                env.IL.Emit(OpCodes.Ldarg_0);
                return;
            }

            if (kind.Contains("ident") || kind.Contains("name"))
            {
                var n = (IName)e;
                LocalBuilder tmpLocal;
                int tmpLbIndex;
                if (env.Locals.TryGetValue(n.Id, out tmpLocal))
                    env.IL.Emit(OpCodes.Ldloc, tmpLocal);
                else if (env.ParamIndex.TryGetValue(n.Id, out tmpLbIndex))
                    Ldarg(env.IL, tmpLbIndex);
                else
                {
                    var fb = env.Ctx.GetFieldCascade(env.UserClassName, n.Id);
                    env.IL.Emit(OpCodes.Ldarg_0);
                    env.IL.Emit(OpCodes.Ldfld, fb);
                }
                return;
            }

            if (kind.Contains("member") || kind.Contains("dot"))
            {
                var d = (IDot)e;
                EmitExpr(env, d.Left);
                Type hostTb = InferTypeOfExpr(env, d.Left) as Type;
                if (hostTb == null) hostTb = env.ThisType;
                var hostName = hostTb.Name.StartsWith("OUser.") ? hostTb.Name.Substring(6) : hostTb.Name;
                var fb = env.Ctx.GetFieldCascade(hostName, d.Member);
                env.IL.Emit(OpCodes.Ldfld, fb);
                return;
            }

            if (kind.Contains("new") || kind.Contains("ctor"))
            {
                var nw = (INew)e;
                var argTypesList = new List<Type>();
                foreach (var a in nw.Args)
                {
                    var t = InferTypeOfExpr(env, a) as Type;
                    if (t == null) t = typeof(object);
                    argTypesList.Add(t);
                }
                var argTypes = argTypesList.ToArray();
                var cb = env.Ctx.GetCtor(nw.ClassName, argTypes);
                if (cb == null) cb = env.Ctx.GetCtor(nw.ClassName, Type.EmptyTypes);
                if (cb == null) throw new InvalidOperationException("No ctor for " + nw.ClassName);

                foreach (var a in nw.Args) EmitExpr(env, a);
                env.IL.Emit(OpCodes.Newobj, cb);
                return;
            }

            if (kind.Contains("call"))
            {
                var c = (ICall)e;
                var dot = c.Target as IDot;
                if (dot != null)
                {
                    if (TryEmitOperator(env, dot.Member, dot.Left, c.Args)) return;

                    EmitExpr(env, dot.Left);
                    foreach (var a in c.Args) EmitExpr(env, a);

                    var owner = InferTypeOfExpr(env, dot.Left) as Type;
                    if (owner == null || owner == typeof(object)) owner = env.ThisType;

                    var sigTypesList = new List<Type>();
                    foreach (var a in c.Args)
                    {
                        var t = InferTypeOfExpr(env, a) as Type;
                        if (t == null) t = typeof(object);
                        sigTypesList.Add(t);
                    }

                    var mb = env.Ctx.FindMethod(owner, dot.Member, sigTypesList.ToArray());
                    env.IL.Emit(OpCodes.Callvirt, mb);
                    return;
                }
                else
                {
                    env.IL.Emit(OpCodes.Ldarg_0);
                    foreach (var a in c.Args) EmitExpr(env, a);
                    var name = (c.Target as IName) != null ? ((IName)c.Target).Id : "call";
                    var sigTypesList = new List<Type>();
                    foreach (var a in c.Args)
                    {
                        var t = InferTypeOfExpr(env, a) as Type;
                        if (t == null) t = typeof(object);
                        sigTypesList.Add(t);
                    }
                    var mb = env.Ctx.FindMethod(env.ThisType, name, sigTypesList.ToArray());
                    env.IL.Emit(OpCodes.Callvirt, mb);
                    return;
                }
            }

            throw new NotSupportedException("Expr " + e.GetType().Name);
        }

        private static bool TryEmitPrimitiveCtorLike(MethodEnv env, INew nw)
        {
            if (nw.Args.Count != 1) return false;

            var arg = nw.Args[0];
            switch (nw.ClassName)
            {
                case "Integer":
                    EmitExpr(env, arg);
                    return true;
                case "Real":
                    EmitExpr(env, arg);
                    if (InferTypeOfExpr(env, arg) as Type != typeof(double))
                        env.IL.Emit(OpCodes.Conv_R8);
                    return true;
                case "Boolean":
                    EmitExpr(env, arg);
                    if (InferTypeOfExpr(env, arg) as Type != typeof(bool))
                        env.IL.Emit(OpCodes.Conv_I1);
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryEmitOperator(MethodEnv env, string name, IExpression left, IReadOnlyList<IExpression> args)
        {
            if (args.Count == 1)
            {
                EmitExpr(env, left);
                EmitExpr(env, args[0]);

                switch (name)
                {
                    case "Plus":  env.IL.Emit(OpCodes.Add); return true;
                    case "Minus": env.IL.Emit(OpCodes.Sub); return true;
                    case "Mult":  env.IL.Emit(OpCodes.Mul); return true;
                    case "Div":   env.IL.Emit(OpCodes.Div); return true;
                    case "Rem":   env.IL.Emit(OpCodes.Rem); return true;

                    case "Equal": env.IL.Emit(OpCodes.Ceq); return true;
                    case "Less":  env.IL.Emit(OpCodes.Clt); return true;
                    case "Greater": env.IL.Emit(OpCodes.Cgt); return true;

                    case "LessEqual":
                        env.IL.Emit(OpCodes.Cgt); env.IL.Emit(OpCodes.Ldc_I4_0); env.IL.Emit(OpCodes.Ceq); return true;
                    case "GreaterEqual":
                        env.IL.Emit(OpCodes.Clt); env.IL.Emit(OpCodes.Ldc_I4_0); env.IL.Emit(OpCodes.Ceq); return true;

                    case "And": env.IL.Emit(OpCodes.And); return true;
                    case "Or":  env.IL.Emit(OpCodes.Or);  return true;
                }
            }

            if (args.Count == 0)
            {
                switch (name)
                {
                    case "UnaryMinus": EmitExpr(env, left); env.IL.Emit(OpCodes.Neg); return true;
                    case "Not":        EmitExpr(env, left); env.IL.Emit(OpCodes.Ldc_I4_0); env.IL.Emit(OpCodes.Ceq); return true;
                }
            }
            return false;
        }

        private static void Ldarg(ILGenerator il, int idx)
        {
            switch (idx)
            {
                case 0: il.Emit(OpCodes.Ldarg_0); break;
                case 1: il.Emit(OpCodes.Ldarg_1); break;
                case 2: il.Emit(OpCodes.Ldarg_2); break;
                case 3: il.Emit(OpCodes.Ldarg_3); break;
                default: il.Emit(OpCodes.Ldarg, idx); break;
            }
        }

        private static object InferTypeOfExpr(MethodEnv env, IExpression e)
        {
            if (e is IThis)         return env.ThisType;
            if (e is ILiteralInt)   return typeof(int);
            if (e is ILiteralReal)  return typeof(double);
            if (e is ILiteralBool)  return typeof(bool);
            if (e is INew)          return env.Ctx.GetTypeBuilder(((INew)e).ClassName);
            if (e is ICall call)
            {
                var argTypesList = new List<Type>();
                foreach (var a in call.Args)
                {
                    var t = InferTypeOfExpr(env, a) as Type;
                    argTypesList.Add(t ?? typeof(object));
                }
                var sig = argTypesList.ToArray();

                if (call.Target is IDot dot)
                {
                    var owner = InferTypeOfExpr(env, dot.Left) as Type;
                    if (owner == null || owner == typeof(object)) owner = env.ThisType;
                    try
                    {
                        var mb = env.Ctx.FindMethod(owner, dot.Member, sig);
                        if (mb != null) return mb.ReturnType;
                    }
                    catch { }
                    return InferOperatorReturn(dot.Member, sig);
                }
                else
                {
                    var name = (call.Target as IName)?.Id ?? "call";
                    try
                    {
                        var mb = env.Ctx.FindMethod(env.ThisType, name, sig);
                        if (mb != null) return mb.ReturnType;
                    }
                    catch { }
                    return InferOperatorReturn(name, sig);
                }
            }
            if (e is IName)
            {
                var n = (IName)e;
                LocalBuilder loc;
                if (env.Locals.TryGetValue(n.Id, out loc)) return loc.LocalType;

                int pidx;
                if (env.ParamIndex.TryGetValue(n.Id, out pidx)) return env.MethodParamTypes[pidx - 1];

                FieldBuilder f;
                if (env.Ctx.TryGetFieldCascade(env.UserClassName, n.Id, out f) && f != null) return f.FieldType;

                return null;
            }
            if (e is IDot)
            {
                var d = (IDot)e;
                var ht = InferTypeOfExpr(env, d.Left) as Type;
                if (ht != null)
                {
                    FieldBuilder f2;
                    if (env.Ctx.TryGetFieldCascade(GetUserName(ht), d.Member, out f2) && f2 != null)
                        return f2.FieldType;
                }
                return null;
            }
            return null;
        }

        private static Type InferOperatorReturn(string name, Type[] args)
        {
            if (name is "Plus" or "Minus" or "Mult" or "Div" or "Rem")
                return args.FirstOrDefault() ?? typeof(int);

            if (name is "Equal" or "Less" or "Greater" or "LessEqual" or "GreaterEqual" or "And" or "Or" or "Not")
                return typeof(bool);

            return null;
        }

        private static string GetUserName(Type t) { return t.Name.StartsWith("OUser.") ? t.Name.Substring(6) : t.Name; }

        private static void EmitDefaultFor(ILGenerator il, Type t, BuildContext ctx, int depth)
        {
            if (t == typeof(int))    { il.Emit(OpCodes.Ldc_I4_0); return; }
            if (t == typeof(double)) { il.Emit(OpCodes.Ldc_R8, 0.0); return; }
            if (t == typeof(bool))   { il.Emit(OpCodes.Ldc_I4_0); return; }

            var tb = t as TypeBuilder;
            if (tb != null)
            {
                var cls = GetUserName(tb);
                ConstructorBuilder p0;
                if (ctx.TryGetCtor(cls, Type.EmptyTypes, out p0) && p0 != null)
                {
                    il.Emit(OpCodes.Newobj, p0);
                    return;
                }

                if (depth > 0)
                {
                    ConstructorBuilder best;
                    Type[] sig;
                    if (ctx.TryGetCheapestCtor(cls, out best, out sig))
                    {
                        for (int i = 0; i < sig.Length; i++)
                            EmitDefaultFor(il, sig[i], ctx, depth - 1);
                        il.Emit(OpCodes.Newobj, best);
                        return;
                    }
                }

                il.Emit(OpCodes.Ldnull);
                return;
            }

            il.Emit(OpCodes.Ldnull);
        }

        private sealed class BuildContext
        {
            private readonly ModuleBuilder _mb;
            private readonly Dictionary<string, TypeBuilder> _types = new Dictionary<string, TypeBuilder>();
            private readonly Dictionary<(string cls, string field), FieldBuilder> _fields = new Dictionary<(string cls, string field), FieldBuilder>();
            private readonly Dictionary<(string cls, string name, string sig), MethodBuilder> _methods = new Dictionary<(string cls, string name, string sig), MethodBuilder>();
            private readonly Dictionary<(string cls, string sig), ConstructorBuilder> _ctors = new Dictionary<(string cls, string sig), ConstructorBuilder>();
            private readonly Dictionary<string, List<(ConstructorBuilder cb, Type[] sig)>> _ctorsByClass = new Dictionary<string, List<(ConstructorBuilder cb, Type[] sig)>>();

            public BuildContext(ModuleBuilder mb) { _mb = mb; }

            public void RegisterType(string name, TypeBuilder tb) { _types[name] = tb; }
            public bool HasType(string name) { return _types.ContainsKey(name); }

            public TypeBuilder GetTypeBuilder(string name)
            {
                TypeBuilder tb;
                if (_types.TryGetValue(name, out tb)) return tb;
                throw new KeyNotFoundException("Type " + name);
            }

            public MethodBuilder FindUserMethodCI(string cls, string name, Type[] paramTypes)
            {
                var m = GetMethod(cls, name, paramTypes);
                if (m != null) return m;

                var sig = Sig(paramTypes);
                foreach (var kv in _methods)
                {
                    if (!string.Equals(kv.Key.cls, cls, StringComparison.OrdinalIgnoreCase)) continue;
                    if (!string.Equals(kv.Key.sig, sig, StringComparison.Ordinal)) continue;
                    if (string.Equals(kv.Key.name, name, StringComparison.OrdinalIgnoreCase))
                        return kv.Value;
                }
                return null;
            }

            public Type MapType(string oType)
            {
                if (oType == "Integer") return typeof(int);
                if (oType == "Real")    return typeof(double);
                if (oType == "Boolean") return typeof(bool);
                TypeBuilder tb;
                if (_types.TryGetValue(oType, out tb)) return tb;
                throw new InvalidOperationException("Unknown type " + oType);
            }

            public Type ResolveOrCreatePlaceholder(string name)
            {
                TypeBuilder tb;
                if (_types.TryGetValue(name, out tb)) return tb;
                tb = _mb.DefineType("OUser." + name, TypeAttributes.Public | TypeAttributes.Class, typeof(object));
                _types[name] = tb;
                return tb;
            }

            public void RegisterField(string cls, string field, FieldBuilder fb) { _fields[(cls, field)] = fb; }

            public FieldBuilder GetField(string cls, string field) { return _fields[(cls, field)]; }

            public bool TryGetField(string cls, string field, out FieldBuilder fb) { return _fields.TryGetValue((cls, field), out fb); }

            public bool TryGetFieldCascade(string cls, string field, out FieldBuilder fb)
            {
                if (_fields.TryGetValue((cls, field), out fb)) return true;

                TypeBuilder tb;
                while (_types.TryGetValue(cls, out tb))
                {
                    var bt = tb.BaseType;
                    if (bt == null) break;

                    if (bt.Name.StartsWith("OUser."))
                    {
                        var baseName = bt.Name.Substring(6);
                        if (_fields.TryGetValue((baseName, field), out fb)) return true;
                        cls = baseName;
                        continue;
                    }
                    break;
                }

                fb = null;
                return false;
            }

            public FieldBuilder GetFieldCascade(string cls, string field)
            {
                FieldBuilder fb;
                if (TryGetFieldCascade(cls, field, out fb) && fb != null) return fb;
                throw new KeyNotFoundException("Field '" + field + "' not found in '" + cls + "' or its base classes.");
            }

            public void RegisterMethod(string cls, string name, MethodBuilder mb, Type[] paramTypes, Type ret)
            {
                _methods[(cls, name, Sig(paramTypes))] = mb;
            }

            public MethodBuilder GetMethod(string cls, string name, Type[] paramTypes)
            {
                MethodBuilder mb;
                if (_methods.TryGetValue((cls, name, Sig(paramTypes)), out mb)) return mb;
                return null;
            }

            public MethodInfo FindMethod(Type owner, string name, Type[] paramTypes)
            {
                var cls = owner.Name.StartsWith("OUser.") ? owner.Name.Substring(6) : owner.Name;
                MethodBuilder mb;
                if (_methods.TryGetValue((cls, name, Sig(paramTypes)), out mb)) return mb;

                var alt = _methods.FirstOrDefault(kv =>
                    string.Equals(kv.Key.cls, cls, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(kv.Key.name, name, StringComparison.OrdinalIgnoreCase) &&
                    SigLength(kv.Key.sig) == paramTypes.Length);
                if (!alt.Equals(default(KeyValuePair<(string cls, string name, string sig), MethodBuilder>)))
                    return alt.Value;

                if (owner is TypeBuilder tb)
                {
                    var baseType = tb.BaseType;
                    if (baseType != null) return FindMethod(baseType, name, paramTypes);
                    throw new MissingMethodException(tb.Name, name);
                }

                var mi = owner.GetMethod(name, paramTypes);
                if (mi == null) throw new MissingMethodException(owner.FullName ?? owner.Name, name);
                return mi;
            }

            public void RegisterCtor(string cls, ConstructorBuilder cb, Type[] paramTypes)
            {
                _ctors[(cls, Sig(paramTypes))] = cb;
                List<(ConstructorBuilder cb, Type[] sig)> list;
                if (!_ctorsByClass.TryGetValue(cls, out list))
                {
                    list = new List<(ConstructorBuilder cb, Type[] sig)>();
                    _ctorsByClass[cls] = list;
                }
                list.Add((cb, paramTypes));
            }

            public ConstructorBuilder GetCtor(string cls, Type[] paramTypes)
            {
                ConstructorBuilder cb;
                if (_ctors.TryGetValue((cls, Sig(paramTypes)), out cb)) return cb;
                return null;
            }

            public bool TryGetCtor(string cls, Type[] paramTypes, out ConstructorBuilder cb)
            {
                cb = GetCtor(cls, paramTypes);
                return cb != null;
            }

            public bool TryGetCheapestCtor(string cls, out ConstructorBuilder cb, out Type[] sig)
            {
                List<(ConstructorBuilder cb, Type[] sig)> list;
                if (_ctorsByClass.TryGetValue(cls, out list) && list.Count > 0)
                {
                    var best = list.OrderBy(pair => pair.sig.Length).First();
                    cb = best.cb; sig = best.sig;
                    return true;
                }
                cb = null;
                sig = new Type[0];
                return false;
            }

            private static string Sig(Type[] ps)
            {
                var arr = new string[ps.Length];
                for (int i = 0; i < ps.Length; i++) arr[i] = ps[i].FullName;
                return string.Join(",", arr);
            }

            private static int SigLength(string sig)
            {
                if (string.IsNullOrEmpty(sig)) return 0;
                return sig.Split(',').Length;
            }
        }

        private sealed class MethodEnv
        {
            public readonly BuildContext Ctx;
            public readonly TypeBuilder ThisType;
            public readonly ILGenerator IL;
            public readonly Dictionary<string, LocalBuilder> Locals = new Dictionary<string, LocalBuilder>();
            public readonly Dictionary<string, int> ParamIndex = new Dictionary<string, int>();
            public readonly string UserClassName;
            public readonly Type[] MethodParamTypes;

            public MethodEnv(BuildContext ctx, TypeBuilder tb, ILGenerator il,
                             IReadOnlyList<(string name, string type)> ps, MethodBuilder mb)
            {
                Ctx = ctx; ThisType = tb; IL = il;
                UserClassName = tb.Name.StartsWith("OUser.") ? tb.Name.Substring(6) : tb.Name;
                var list = new List<Type>();
                for (int i = 0; i < ps.Count; i++) list.Add(ctx.MapType(ps[i].type));
                MethodParamTypes = list.ToArray();
                for (int i = 0; i < ps.Count; i++) ParamIndex[ps[i].name] = 1 + i;
            }
        }

        private static (List<string> names, Type[] types) MapParams(BuildContext ctx, IReadOnlyList<(string name, string type)> p)
        {
            var names = new List<string>();
            var types = new List<Type>();
            for (int i = 0; i < p.Count; i++)
            {
                names.Add(p[i].name);
                types.Add(ctx.MapType(p[i].type));
            }
            return (names, types.ToArray());
        }
    }
}
