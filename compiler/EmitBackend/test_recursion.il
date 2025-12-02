.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_recursion'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_recursion.exe // GUID = {91A1023D-5995-4D88-8C62-3D7BF89E891B}


  .class public auto ansi RecursionMath
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
    } // end of method RecursionMath::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default int32 factorial (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 43 (0x2b)
	.maxstack 6
	.locals init (
		int32	V_0)
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldc.i4.1 
	IL_0007:  cgt 
	IL_0009:  ldc.i4.0 
	IL_000a:  ceq 
	IL_000c:  brfalse IL_0013

	IL_0011:  ldc.i4.1 
	IL_0012:  ret 
	IL_0013:  ldarg.0 
	IL_0014:  ldarg 1

	IL_0018:  nop 
	IL_0019:  nop 
	IL_001a:  ldc.i4.1 
	IL_001b:  sub 
	IL_001c:  callvirt instance int32 class RecursionMath::factorial(int32)
	IL_0021:  stloc.0 
	IL_0022:  ldarg 1

	IL_0026:  nop 
	IL_0027:  nop 
	IL_0028:  ldloc.0 
	IL_0029:  mul 
	IL_002a:  ret 
    } // end of method RecursionMath::factorial

    // method line 3
    .method public virtual hidebysig newslot 
           instance default int32 fibonacci (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x212c
	// Code size 59 (0x3b)
	.maxstack 8
	.locals init (
		int32	V_0,
		int32	V_1)
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldc.i4.1 
	IL_0007:  cgt 
	IL_0009:  ldc.i4.0 
	IL_000a:  ceq 
	IL_000c:  brfalse IL_0018

	IL_0011:  ldarg 1

	IL_0015:  nop 
	IL_0016:  nop 
	IL_0017:  ret 
	IL_0018:  ldarg.0 
	IL_0019:  ldarg 1

	IL_001d:  nop 
	IL_001e:  nop 
	IL_001f:  ldc.i4.1 
	IL_0020:  sub 
	IL_0021:  callvirt instance int32 class RecursionMath::fibonacci(int32)
	IL_0026:  stloc.0 
	IL_0027:  ldarg.0 
	IL_0028:  ldarg 1

	IL_002c:  nop 
	IL_002d:  nop 
	IL_002e:  ldc.i4.s 0x02
	IL_0030:  sub 
	IL_0031:  callvirt instance int32 class RecursionMath::fibonacci(int32)
	IL_0036:  stloc.1 
	IL_0037:  ldloc.0 
	IL_0038:  ldloc.1 
	IL_0039:  add 
	IL_003a:  ret 
    } // end of method RecursionMath::fibonacci

    // method line 4
    .method public virtual hidebysig newslot 
           instance default int32 powerOf2 (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x2174
	// Code size 36 (0x24)
	.maxstack 6
	.locals init (
		int32	V_0)
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldc.i4.0 
	IL_0007:  ceq 
	IL_0009:  brfalse IL_0010

	IL_000e:  ldc.i4.1 
	IL_000f:  ret 
	IL_0010:  ldarg.0 
	IL_0011:  ldarg 1

	IL_0015:  nop 
	IL_0016:  nop 
	IL_0017:  ldc.i4.1 
	IL_0018:  sub 
	IL_0019:  callvirt instance int32 class RecursionMath::powerOf2(int32)
	IL_001e:  stloc.0 
	IL_001f:  ldc.i4.s 0x02
	IL_0021:  ldloc.0 
	IL_0022:  mul 
	IL_0023:  ret 
    } // end of method RecursionMath::powerOf2

  } // end of class RecursionMath

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 5
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x21a4
	// Code size 109 (0x6d)
	.maxstack 21
	.locals init (
		object	V_0,
		int32	V_1,
		int32	V_2,
		int32	V_3,
		int32	V_4,
		int32	V_5,
		int32	V_6)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class RecursionMath::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  ldc.i4.s 0x05
	IL_000f:  callvirt instance int32 class RecursionMath::factorial(int32)
	IL_0014:  stloc.1 
	IL_0015:  ldloc.1 
	IL_0016:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_001b:  ldloc.0 
	IL_001c:  ldc.i4.s 0x03
	IL_001e:  callvirt instance int32 class RecursionMath::factorial(int32)
	IL_0023:  stloc.2 
	IL_0024:  ldloc.2 
	IL_0025:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_002a:  ldloc.0 
	IL_002b:  ldc.i4.s 0x06
	IL_002d:  callvirt instance int32 class RecursionMath::fibonacci(int32)
	IL_0032:  stloc.3 
	IL_0033:  ldloc.3 
	IL_0034:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0039:  ldloc.0 
	IL_003a:  ldc.i4.s 0x04
	IL_003c:  callvirt instance int32 class RecursionMath::fibonacci(int32)
	IL_0041:  stloc.s 4
	IL_0043:  ldloc.s 4
	IL_0045:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_004a:  ldloc.0 
	IL_004b:  ldc.i4.s 0x03
	IL_004d:  callvirt instance int32 class RecursionMath::powerOf2(int32)
	IL_0052:  stloc.s 5
	IL_0054:  ldloc.s 5
	IL_0056:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_005b:  ldloc.0 
	IL_005c:  ldc.i4.s 0x05
	IL_005e:  callvirt instance int32 class RecursionMath::powerOf2(int32)
	IL_0063:  stloc.s 6
	IL_0065:  ldloc.s 6
	IL_0067:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_006c:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 6
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2220
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method <EntryPoint>::.ctor

    // method line 7
    .method public static 
           default void Main (string[] A_0)  cil managed 
    {
        // Method begins at RVA 0x2228
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

