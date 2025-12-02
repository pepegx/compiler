.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_mixed_operations'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_mixed_operations.exe // GUID = {DEBE259C-F802-4655-9EF1-EC0D2524B31C}


  .class public auto ansi MixedOperations
  	extends [mscorlib]System.Object
  {

    // method line 1
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x20ec
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method MixedOperations::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default float64 complexCalc (int32 A_1, float64 A_2)  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 51 (0x33)
	.maxstack 3
	.locals init (
		int32	V_0,
		float64	V_1)
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldc.i4.s 0x05
	IL_0008:  add 
	IL_0009:  stloc.0 
	IL_000a:  ldloc.0 
	IL_000b:  conv.r8 
	IL_000c:  ldarg 2

	IL_0010:  nop 
	IL_0011:  nop 
	IL_0012:  add 
	IL_0013:  stloc.1 
	IL_0014:  ldloc.1 
	IL_0015:  ldc.r8 100.
	IL_001e:  cgt 
	IL_0020:  brfalse IL_0031

	IL_0025:  ldloc.1 
	IL_0026:  ldc.r8 2.
	IL_002f:  div 
	IL_0030:  ret 
	IL_0031:  ldloc.1 
	IL_0032:  ret 
    } // end of method MixedOperations::complexCalc

    // method line 3
    .method public virtual hidebysig newslot 
           instance default float64 mixedArithmetic ()  cil managed 
    {
        // Method begins at RVA 0x2134
	// Code size 63 (0x3f)
	.maxstack 5
	.locals init (
		int32	V_0,
		float64	V_1,
		int32	V_2,
		int32	V_3,
		int32	V_4,
		int32	V_5)
	IL_0000:  ldc.i4.s 0x0a
	IL_0002:  stloc.0 
	IL_0003:  ldc.r8 3.5
	IL_000c:  stloc.1 
	IL_000d:  ldloc.0 
	IL_000e:  conv.r8 
	IL_000f:  ldloc.1 
	IL_0010:  add 
	IL_0011:  stloc.2 
	IL_0012:  ldloc.0 
	IL_0013:  conv.r8 
	IL_0014:  ldloc.1 
	IL_0015:  sub 
	IL_0016:  stloc.3 
	IL_0017:  ldloc.0 
	IL_0018:  conv.r8 
	IL_0019:  ldloc.1 
	IL_001a:  mul 
	IL_001b:  stloc.s 4
	IL_001d:  ldloc.0 
	IL_001e:  conv.r8 
	IL_001f:  ldloc.1 
	IL_0020:  div 
	IL_0021:  stloc.s 5
	IL_0023:  ldloc.2 
	IL_0024:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0029:  ldloc.3 
	IL_002a:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_002f:  ldloc.s 4
	IL_0031:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0036:  ldloc.s 5
	IL_0038:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_003d:  ldloc.2 
	IL_003e:  ret 
    } // end of method MixedOperations::mixedArithmetic

    // method line 4
    .method public virtual hidebysig newslot 
           instance default float64 complexExpression ()  cil managed 
    {
        // Method begins at RVA 0x2180
	// Code size 98 (0x62)
	.maxstack 4
	.locals init (
		int32	V_0,
		float64	V_1,
		int32	V_2,
		int32	V_3,
		float64	V_4,
		float64	V_5,
		float64	V_6)
	IL_0000:  ldc.i4.s 0x05
	IL_0002:  stloc.0 
	IL_0003:  ldc.r8 2.5
	IL_000c:  stloc.1 
	IL_000d:  ldc.i4.s 0x03
	IL_000f:  stloc.2 
	IL_0010:  ldloc.0 
	IL_0011:  ldloc.2 
	IL_0012:  mul 
	IL_0013:  stloc.3 
	IL_0014:  ldloc.3 
	IL_0015:  conv.r8 
	IL_0016:  ldloc.1 
	IL_0017:  add 
	IL_0018:  stloc.s 4
	IL_001a:  ldloc.s 4
	IL_001c:  ldc.r8 2.
	IL_0025:  mul 
	IL_0026:  stloc.s 5
	IL_0028:  ldloc.s 5
	IL_002a:  ldc.r8 5.
	IL_0033:  div 
	IL_0034:  stloc.s 6
	IL_0036:  ldloc.s 6
	IL_0038:  ldc.r8 10.
	IL_0041:  cgt 
	IL_0043:  brfalse IL_0055

	IL_0048:  ldloc.s 6
	IL_004a:  ldc.r8 5.
	IL_0053:  sub 
	IL_0054:  ret 
	IL_0055:  ldloc.s 6
	IL_0057:  ldc.r8 5.
	IL_0060:  add 
	IL_0061:  ret 
    } // end of method MixedOperations::complexExpression

    // method line 5
    .method public virtual hidebysig newslot 
           instance default bool compareMixed ()  cil managed 
    {
        // Method begins at RVA 0x21f0
	// Code size 62 (0x3e)
	.maxstack 4
	.locals init (
		int32	V_0,
		float64	V_1,
		bool	V_2,
		bool	V_3,
		bool	V_4)
	IL_0000:  ldc.i4.s 0x0a
	IL_0002:  stloc.0 
	IL_0003:  ldc.r8 10.5
	IL_000c:  stloc.1 
	IL_000d:  ldloc.0 
	IL_000e:  conv.r8 
	IL_000f:  ldloc.1 
	IL_0010:  clt 
	IL_0012:  stloc.2 
	IL_0013:  ldloc.0 
	IL_0014:  conv.r8 
	IL_0015:  ldc.r8 10.
	IL_001e:  ceq 
	IL_0020:  stloc.3 
	IL_0021:  ldloc.1 
	IL_0022:  conv.i4 
	IL_0023:  ldc.i4.s 0x0a
	IL_0025:  ceq 
	IL_0027:  stloc.s 4
	IL_0029:  ldloc.2 
	IL_002a:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_002f:  ldloc.3 
	IL_0030:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_0035:  ldloc.s 4
	IL_0037:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_003c:  ldloc.2 
	IL_003d:  ret 
    } // end of method MixedOperations::compareMixed

    // method line 6
    .method public virtual hidebysig newslot 
           instance default int32 test ()  cil managed 
    {
        // Method begins at RVA 0x223c
	// Code size 90 (0x5a)
	.maxstack 15
	.locals init (
		float64	V_0,
		float64	V_1,
		float64	V_2,
		bool	V_3,
		float64	V_4)
	IL_0000:  ldarg.0 
	IL_0001:  ldc.i4.s 0x32
	IL_0003:  ldc.r8 60.
	IL_000c:  callvirt instance float64 class MixedOperations::complexCalc(int32, float64)
	IL_0011:  stloc.0 
	IL_0012:  ldarg.0 
	IL_0013:  ldc.i4.s 0x64
	IL_0015:  ldc.r8 10.
	IL_001e:  callvirt instance float64 class MixedOperations::complexCalc(int32, float64)
	IL_0023:  stloc.1 
	IL_0024:  ldarg.0 
	IL_0025:  callvirt instance float64 class MixedOperations::complexExpression()
	IL_002a:  stloc.2 
	IL_002b:  ldarg.0 
	IL_002c:  callvirt instance bool class MixedOperations::compareMixed()
	IL_0031:  stloc.3 
	IL_0032:  ldloc.0 
	IL_0033:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0038:  ldloc.1 
	IL_0039:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_003e:  ldloc.2 
	IL_003f:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0044:  ldloc.3 
	IL_0045:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_004a:  ldloc.0 
	IL_004b:  ldloc.1 
	IL_004c:  add 
	IL_004d:  ldloc.2 
	IL_004e:  add 
	IL_004f:  stloc.s 4
	IL_0051:  ldloc.s 4
	IL_0053:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0058:  ldc.i4.0 
	IL_0059:  ret 
    } // end of method MixedOperations::test

  } // end of class MixedOperations

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 7
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x22a4
	// Code size 26 (0x1a)
	.maxstack 6
	.locals init (
		object	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class MixedOperations::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class MixedOperations::test()
	IL_0012:  stloc.1 
	IL_0013:  ldloc.1 
	IL_0014:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0019:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 8
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x22cc
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method <EntryPoint>::.ctor

    // method line 9
    .method public static 
           default void Main (string[] A_0)  cil managed 
    {
        // Method begins at RVA 0x22d4
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

