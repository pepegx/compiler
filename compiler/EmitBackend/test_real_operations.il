.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_real_operations'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_real_operations.exe // GUID = {31210B90-CCEF-4DC9-8467-9C62C4B0CB29}


  .class public auto ansi RealOperations
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
    } // end of method RealOperations::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default float64 addReals (float64 A_1, float64 A_2)  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldarg 2

	IL_000a:  nop 
	IL_000b:  nop 
	IL_000c:  add 
	IL_000d:  ret 
    } // end of method RealOperations::addReals

    // method line 3
    .method public virtual hidebysig newslot 
           instance default float64 multiplyReals (float64 A_1, float64 A_2)  cil managed 
    {
        // Method begins at RVA 0x2104
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldarg 2

	IL_000a:  nop 
	IL_000b:  nop 
	IL_000c:  mul 
	IL_000d:  ret 
    } // end of method RealOperations::multiplyReals

    // method line 4
    .method public virtual hidebysig newslot 
           instance default float64 divideReals (float64 A_1, float64 A_2)  cil managed 
    {
        // Method begins at RVA 0x2114
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldarg 2

	IL_000a:  nop 
	IL_000b:  nop 
	IL_000c:  div 
	IL_000d:  ret 
    } // end of method RealOperations::divideReals

    // method line 5
    .method public virtual hidebysig newslot 
           instance default bool compareReals (float64 A_1, float64 A_2)  cil managed 
    {
        // Method begins at RVA 0x2124
	// Code size 15 (0xf)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldarg 2

	IL_000a:  nop 
	IL_000b:  nop 
	IL_000c:  cgt 
	IL_000e:  ret 
    } // end of method RealOperations::compareReals

    // method line 6
    .method public virtual hidebysig newslot 
           instance default float64 checkEpsilon ()  cil managed 
    {
        // Method begins at RVA 0x2134
	// Code size 12 (0xc)
	.maxstack 1
	.locals init (
		float64	V_0)
	IL_0000:  ldc.r8 4.9406564584124654e-324
	IL_0009:  stloc.0 
	IL_000a:  ldloc.0 
	IL_000b:  ret 
    } // end of method RealOperations::checkEpsilon

    // method line 7
    .method public virtual hidebysig newslot 
           instance default int32 testRealArithmetic ()  cil managed 
    {
        // Method begins at RVA 0x214c
	// Code size 69 (0x45)
	.maxstack 10
	.locals init (
		float64	V_0,
		float64	V_1,
		float64	V_2,
		float64	V_3,
		float64	V_4)
	IL_0000:  ldc.r8 3.5
	IL_0009:  stloc.0 
	IL_000a:  ldc.r8 2.25
	IL_0013:  stloc.1 
	IL_0014:  ldarg.0 
	IL_0015:  ldloc.0 
	IL_0016:  ldloc.1 
	IL_0017:  callvirt instance float64 class RealOperations::addReals(float64, float64)
	IL_001c:  stloc.2 
	IL_001d:  ldarg.0 
	IL_001e:  ldloc.0 
	IL_001f:  ldloc.1 
	IL_0020:  callvirt instance float64 class RealOperations::multiplyReals(float64, float64)
	IL_0025:  stloc.3 
	IL_0026:  ldarg.0 
	IL_0027:  ldloc.0 
	IL_0028:  ldloc.1 
	IL_0029:  callvirt instance float64 class RealOperations::divideReals(float64, float64)
	IL_002e:  stloc.s 4
	IL_0030:  ldloc.2 
	IL_0031:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0036:  ldloc.3 
	IL_0037:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_003c:  ldloc.s 4
	IL_003e:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0043:  ldc.i4.0 
	IL_0044:  ret 
    } // end of method RealOperations::testRealArithmetic

    // method line 8
    .method public virtual hidebysig newslot 
           instance default int32 testRealComparisons ()  cil managed 
    {
        // Method begins at RVA 0x21a0
	// Code size 90 (0x5a)
	.maxstack 9
	.locals init (
		float64	V_0,
		float64	V_1,
		float64	V_2,
		bool	V_3,
		bool	V_4,
		bool	V_5,
		bool	V_6)
	IL_0000:  ldc.r8 5.7000000000000002
	IL_0009:  stloc.0 
	IL_000a:  ldc.r8 3.2000000000000002
	IL_0013:  stloc.1 
	IL_0014:  ldc.r8 5.7000000000000002
	IL_001d:  stloc.2 
	IL_001e:  ldarg.0 
	IL_001f:  ldloc.0 
	IL_0020:  ldloc.1 
	IL_0021:  callvirt instance bool class RealOperations::compareReals(float64, float64)
	IL_0026:  stloc.3 
	IL_0027:  ldarg.0 
	IL_0028:  ldloc.1 
	IL_0029:  ldloc.0 
	IL_002a:  callvirt instance bool class RealOperations::compareReals(float64, float64)
	IL_002f:  stloc.s 4
	IL_0031:  ldloc.0 
	IL_0032:  ldloc.2 
	IL_0033:  ceq 
	IL_0035:  stloc.s 5
	IL_0037:  ldloc.1 
	IL_0038:  ldloc.0 
	IL_0039:  clt 
	IL_003b:  stloc.s 6
	IL_003d:  ldloc.3 
	IL_003e:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_0043:  ldloc.s 4
	IL_0045:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_004a:  ldloc.s 5
	IL_004c:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_0051:  ldloc.s 6
	IL_0053:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_0058:  ldc.i4.0 
	IL_0059:  ret 
    } // end of method RealOperations::testRealComparisons

    // method line 9
    .method public virtual hidebysig newslot 
           instance default int32 testRealConstants ()  cil managed 
    {
        // Method begins at RVA 0x2208
	// Code size 60 (0x3c)
	.maxstack 6
	.locals init (
		float64	V_0,
		float64	V_1,
		float64	V_2,
		float64	V_3)
	IL_0000:  ldc.r8 -1.7976931348623157e+308
	IL_0009:  stloc.0 
	IL_000a:  ldc.r8 1.7976931348623157e+308
	IL_0013:  stloc.1 
	IL_0014:  ldc.r8 4.9406564584124654e-324
	IL_001d:  stloc.2 
	IL_001e:  ldloc.0 
	IL_001f:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0024:  ldloc.1 
	IL_0025:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_002a:  ldloc.2 
	IL_002b:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0030:  ldloc.0 
	IL_0031:  ldloc.2 
	IL_0032:  add 
	IL_0033:  stloc.3 
	IL_0034:  ldloc.3 
	IL_0035:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_003a:  ldc.i4.0 
	IL_003b:  ret 
    } // end of method RealOperations::testRealConstants

    // method line 10
    .method public virtual hidebysig newslot 
           instance default int32 testRealPrecision ()  cil managed 
    {
        // Method begins at RVA 0x2250
	// Code size 96 (0x60)
	.maxstack 5
	.locals init (
		float64	V_0,
		float64	V_1,
		float64	V_2,
		float64	V_3,
		bool	V_4,
		float64	V_5,
		float64	V_6)
	IL_0000:  ldc.r8 0.10000000000000001
	IL_0009:  stloc.0 
	IL_000a:  ldc.r8 0.20000000000000001
	IL_0013:  stloc.1 
	IL_0014:  ldc.r8 0.29999999999999999
	IL_001d:  stloc.2 
	IL_001e:  ldloc.0 
	IL_001f:  ldloc.1 
	IL_0020:  add 
	IL_0021:  stloc.3 
	IL_0022:  ldloc.3 
	IL_0023:  ldloc.2 
	IL_0024:  ceq 
	IL_0026:  stloc.s 4
	IL_0028:  ldloc.3 
	IL_0029:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_002e:  ldloc.s 4
	IL_0030:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_0035:  ldloc.3 
	IL_0036:  ldloc.2 
	IL_0037:  sub 
	IL_0038:  stloc.s 5
	IL_003a:  ldloc.s 5
	IL_003c:  stloc.s 6
	IL_003e:  ldloc.s 5
	IL_0040:  ldc.i4.0 
	IL_0041:  conv.r8 
	IL_0042:  clt 
	IL_0044:  brfalse IL_0053

	IL_0049:  ldloc.s 5
	IL_004b:  neg 
	IL_004c:  stloc.s 6
	IL_004e:  br IL_0057

	IL_0053:  ldloc.s 5
	IL_0055:  stloc.s 6
	IL_0057:  ldloc.s 6
	IL_0059:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_005e:  ldc.i4.0 
	IL_005f:  ret 
    } // end of method RealOperations::testRealPrecision

    // method line 11
    .method public virtual hidebysig newslot 
           instance default int32 test ()  cil managed 
    {
        // Method begins at RVA 0x22bc
	// Code size 15 (0xf)
	.maxstack 4
	.locals init (
		float64	V_0)
	IL_0000:  ldarg.0 
	IL_0001:  callvirt instance float64 class RealOperations::checkEpsilon()
	IL_0006:  stloc.0 
	IL_0007:  ldloc.0 
	IL_0008:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_000d:  ldc.i4.0 
	IL_000e:  ret 
    } // end of method RealOperations::test

  } // end of class RealOperations

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 12
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x22d8
	// Code size 26 (0x1a)
	.maxstack 6
	.locals init (
		object	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class RealOperations::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class RealOperations::test()
	IL_0012:  stloc.1 
	IL_0013:  ldloc.1 
	IL_0014:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0019:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 13
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2300
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method <EntryPoint>::.ctor

    // method line 14
    .method public static 
           default void Main (string[] A_0)  cil managed 
    {
        // Method begins at RVA 0x2308
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

