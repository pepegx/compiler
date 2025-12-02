.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_loops_complex'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_loops_complex.exe // GUID = {DF3D507E-FEDE-4CA5-977C-CE9BB3A81FCF}


  .class public auto ansi LoopOperations
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
    } // end of method LoopOperations::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default int32 factorial (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 36 (0x24)
	.maxstack 3
	.locals init (
		int32	V_0,
		int32	V_1)
	IL_0000:  ldc.i4.1 
	IL_0001:  stloc.0 
	IL_0002:  ldc.i4.1 
	IL_0003:  stloc.1 
	IL_0004:  ldloc.1 
	IL_0005:  ldarg 1

	IL_0009:  nop 
	IL_000a:  nop 
	IL_000b:  cgt 
	IL_000d:  ldc.i4.0 
	IL_000e:  ceq 
	IL_0010:  brfalse IL_0022

	IL_0015:  ldloc.0 
	IL_0016:  ldloc.1 
	IL_0017:  mul 
	IL_0018:  stloc.0 
	IL_0019:  ldloc.1 
	IL_001a:  ldc.i4.1 
	IL_001b:  add 
	IL_001c:  stloc.1 
	IL_001d:  br IL_0004

	IL_0022:  ldloc.0 
	IL_0023:  ret 
    } // end of method LoopOperations::factorial

    // method line 3
    .method public virtual hidebysig newslot 
           instance default int32 sumNumbers (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x2124
	// Code size 36 (0x24)
	.maxstack 3
	.locals init (
		int32	V_0,
		int32	V_1)
	IL_0000:  ldc.i4.0 
	IL_0001:  stloc.0 
	IL_0002:  ldc.i4.1 
	IL_0003:  stloc.1 
	IL_0004:  ldloc.1 
	IL_0005:  ldarg 1

	IL_0009:  nop 
	IL_000a:  nop 
	IL_000b:  cgt 
	IL_000d:  ldc.i4.0 
	IL_000e:  ceq 
	IL_0010:  brfalse IL_0022

	IL_0015:  ldloc.0 
	IL_0016:  ldloc.1 
	IL_0017:  add 
	IL_0018:  stloc.0 
	IL_0019:  ldloc.1 
	IL_001a:  ldc.i4.1 
	IL_001b:  add 
	IL_001c:  stloc.1 
	IL_001d:  br IL_0004

	IL_0022:  ldloc.0 
	IL_0023:  ret 
    } // end of method LoopOperations::sumNumbers

    // method line 4
    .method public virtual hidebysig newslot 
           instance default int32 nestedLoops ()  cil managed 
    {
        // Method begins at RVA 0x2154
	// Code size 58 (0x3a)
	.maxstack 4
	.locals init (
		int32	V_0,
		int32	V_1,
		int32	V_2)
	IL_0000:  ldc.i4.0 
	IL_0001:  stloc.0 
	IL_0002:  ldc.i4.1 
	IL_0003:  stloc.1 
	IL_0004:  ldloc.1 
	IL_0005:  ldc.i4.s 0x03
	IL_0007:  cgt 
	IL_0009:  ldc.i4.0 
	IL_000a:  ceq 
	IL_000c:  brfalse IL_0038

	IL_0011:  ldc.i4.1 
	IL_0012:  stloc.2 
	IL_0013:  ldloc.2 
	IL_0014:  ldc.i4.s 0x02
	IL_0016:  cgt 
	IL_0018:  ldc.i4.0 
	IL_0019:  ceq 
	IL_001b:  brfalse IL_002f

	IL_0020:  ldloc.0 
	IL_0021:  ldloc.1 
	IL_0022:  ldloc.2 
	IL_0023:  mul 
	IL_0024:  add 
	IL_0025:  stloc.0 
	IL_0026:  ldloc.2 
	IL_0027:  ldc.i4.1 
	IL_0028:  add 
	IL_0029:  stloc.2 
	IL_002a:  br IL_0013

	IL_002f:  ldloc.1 
	IL_0030:  ldc.i4.1 
	IL_0031:  add 
	IL_0032:  stloc.1 
	IL_0033:  br IL_0004

	IL_0038:  ldloc.0 
	IL_0039:  ret 
    } // end of method LoopOperations::nestedLoops

    // method line 5
    .method public virtual hidebysig newslot 
           instance default int32 test ()  cil managed 
    {
        // Method begins at RVA 0x219c
	// Code size 60 (0x3c)
	.maxstack 13
	.locals init (
		int32	V_0,
		int32	V_1,
		int32	V_2,
		int32	V_3)
	IL_0000:  ldarg.0 
	IL_0001:  ldc.i4.s 0x05
	IL_0003:  callvirt instance int32 class LoopOperations::factorial(int32)
	IL_0008:  stloc.0 
	IL_0009:  ldarg.0 
	IL_000a:  ldc.i4.s 0x03
	IL_000c:  callvirt instance int32 class LoopOperations::factorial(int32)
	IL_0011:  stloc.1 
	IL_0012:  ldarg.0 
	IL_0013:  ldc.i4.s 0x0a
	IL_0015:  callvirt instance int32 class LoopOperations::sumNumbers(int32)
	IL_001a:  stloc.2 
	IL_001b:  ldarg.0 
	IL_001c:  callvirt instance int32 class LoopOperations::nestedLoops()
	IL_0021:  stloc.3 
	IL_0022:  ldloc.0 
	IL_0023:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0028:  ldloc.1 
	IL_0029:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_002e:  ldloc.2 
	IL_002f:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0034:  ldloc.3 
	IL_0035:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_003a:  ldc.i4.0 
	IL_003b:  ret 
    } // end of method LoopOperations::test

  } // end of class LoopOperations

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 6
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x21e4
	// Code size 26 (0x1a)
	.maxstack 6
	.locals init (
		object	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class LoopOperations::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class LoopOperations::test()
	IL_0012:  stloc.1 
	IL_0013:  ldloc.1 
	IL_0014:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0019:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 7
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x220c
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method <EntryPoint>::.ctor

    // method line 8
    .method public static 
           default void Main (string[] A_0)  cil managed 
    {
        // Method begins at RVA 0x2214
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

