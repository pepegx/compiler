.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_conditionals'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_conditionals.exe // GUID = {2DE0287A-226C-4686-B214-8CB1F15B2506}


  .class public auto ansi ConditionalOperations
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
    } // end of method ConditionalOperations::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default int32 checkValue (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 36 (0x24)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldc.i4.0 
	IL_0007:  clt 
	IL_0009:  brfalse IL_0010

	IL_000e:  ldc.i4.1 
	IL_000f:  ret 
	IL_0010:  ldarg 1

	IL_0014:  nop 
	IL_0015:  nop 
	IL_0016:  ldc.i4.0 
	IL_0017:  ceq 
	IL_0019:  brfalse IL_0021

	IL_001e:  ldc.i4.s 0x02
	IL_0020:  ret 
	IL_0021:  ldc.i4.s 0x03
	IL_0023:  ret 
    } // end of method ConditionalOperations::checkValue

    // method line 3
    .method public virtual hidebysig newslot 
           instance default int32 compareNumbers (int32 A_1, int32 A_2)  cil managed 
    {
        // Method begins at RVA 0x211c
	// Code size 45 (0x2d)
	.maxstack 4
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldarg 2

	IL_000a:  nop 
	IL_000b:  nop 
	IL_000c:  cgt 
	IL_000e:  brfalse IL_0015

	IL_0013:  ldc.i4.1 
	IL_0014:  ret 
	IL_0015:  ldarg 1

	IL_0019:  nop 
	IL_001a:  nop 
	IL_001b:  ldarg 2

	IL_001f:  nop 
	IL_0020:  nop 
	IL_0021:  ceq 
	IL_0023:  brfalse IL_002a

	IL_0028:  ldc.i4.0 
	IL_0029:  ret 
	IL_002a:  ldc.i4.s 0xffffffff
	IL_002c:  ret 
    } // end of method ConditionalOperations::compareNumbers

    // method line 4
    .method public virtual hidebysig newslot 
           instance default int32 simpleCondition (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x2158
	// Code size 35 (0x23)
	.maxstack 4
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldc.i4.0 
	IL_0007:  cgt 
	IL_0009:  brfalse IL_0010

	IL_000e:  ldc.i4.1 
	IL_000f:  ret 
	IL_0010:  ldarg 1

	IL_0014:  nop 
	IL_0015:  nop 
	IL_0016:  ldc.i4.0 
	IL_0017:  ceq 
	IL_0019:  brfalse IL_0020

	IL_001e:  ldc.i4.0 
	IL_001f:  ret 
	IL_0020:  ldc.i4.s 0xffffffff
	IL_0022:  ret 
    } // end of method ConditionalOperations::simpleCondition

    // method line 5
    .method public virtual hidebysig newslot 
           instance default int32 test ()  cil managed 
    {
        // Method begins at RVA 0x2188
	// Code size 151 (0x97)
	.maxstack 28
	.locals init (
		int32	V_0,
		int32	V_1,
		int32	V_2,
		int32	V_3,
		int32	V_4,
		int32	V_5,
		int32	V_6,
		int32	V_7,
		int32	V_8)
	IL_0000:  ldarg.0 
	IL_0001:  ldc.i4.s 0xfffffffb
	IL_0003:  callvirt instance int32 class ConditionalOperations::checkValue(int32)
	IL_0008:  stloc.0 
	IL_0009:  ldarg.0 
	IL_000a:  ldc.i4.0 
	IL_000b:  callvirt instance int32 class ConditionalOperations::checkValue(int32)
	IL_0010:  stloc.1 
	IL_0011:  ldarg.0 
	IL_0012:  ldc.i4.s 0x07
	IL_0014:  callvirt instance int32 class ConditionalOperations::checkValue(int32)
	IL_0019:  stloc.2 
	IL_001a:  ldarg.0 
	IL_001b:  ldc.i4.s 0x0a
	IL_001d:  ldc.i4.s 0x05
	IL_001f:  callvirt instance int32 class ConditionalOperations::compareNumbers(int32, int32)
	IL_0024:  stloc.3 
	IL_0025:  ldarg.0 
	IL_0026:  ldc.i4.s 0x05
	IL_0028:  ldc.i4.s 0x05
	IL_002a:  callvirt instance int32 class ConditionalOperations::compareNumbers(int32, int32)
	IL_002f:  stloc.s 4
	IL_0031:  ldarg.0 
	IL_0032:  ldc.i4.s 0x03
	IL_0034:  ldc.i4.s 0x08
	IL_0036:  callvirt instance int32 class ConditionalOperations::compareNumbers(int32, int32)
	IL_003b:  stloc.s 5
	IL_003d:  ldarg.0 
	IL_003e:  ldc.i4.s 0x05
	IL_0040:  callvirt instance int32 class ConditionalOperations::simpleCondition(int32)
	IL_0045:  stloc.s 6
	IL_0047:  ldarg.0 
	IL_0048:  ldc.i4.0 
	IL_0049:  callvirt instance int32 class ConditionalOperations::simpleCondition(int32)
	IL_004e:  stloc.s 7
	IL_0050:  ldarg.0 
	IL_0051:  ldc.i4.s 0xfffffffd
	IL_0053:  callvirt instance int32 class ConditionalOperations::simpleCondition(int32)
	IL_0058:  stloc.s 8
	IL_005a:  ldloc.0 
	IL_005b:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0060:  ldloc.1 
	IL_0061:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0066:  ldloc.2 
	IL_0067:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_006c:  ldloc.3 
	IL_006d:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0072:  ldloc.s 4
	IL_0074:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0079:  ldloc.s 5
	IL_007b:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0080:  ldloc.s 6
	IL_0082:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0087:  ldloc.s 7
	IL_0089:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_008e:  ldloc.s 8
	IL_0090:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0095:  ldc.i4.0 
	IL_0096:  ret 
    } // end of method ConditionalOperations::test

  } // end of class ConditionalOperations

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 6
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x222c
	// Code size 26 (0x1a)
	.maxstack 6
	.locals init (
		object	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class ConditionalOperations::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class ConditionalOperations::test()
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
        // Method begins at RVA 0x2254
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
        // Method begins at RVA 0x225c
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

