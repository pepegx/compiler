.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_edge_cases'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_edge_cases.exe // GUID = {240BD892-8D0F-405C-80C9-0F3D00896A96}


  .class public auto ansi EdgeCases
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
    } // end of method EdgeCases::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default int32 handleZero (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 13 (0xd)
	.maxstack 3
	.locals init (
		int32	V_0,
		int32	V_1)
	IL_0000:  ldc.i4.0 
	IL_0001:  stloc.0 
	IL_0002:  ldarg 1

	IL_0006:  nop 
	IL_0007:  nop 
	IL_0008:  ldloc.0 
	IL_0009:  add 
	IL_000a:  stloc.1 
	IL_000b:  ldloc.1 
	IL_000c:  ret 
    } // end of method EdgeCases::handleZero

    // method line 3
    .method public virtual hidebysig newslot 
           instance default bool compareWithZero (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x2110
	// Code size 12 (0xc)
	.maxstack 3
	.locals init (
		int32	V_0)
	IL_0000:  ldc.i4.0 
	IL_0001:  stloc.0 
	IL_0002:  ldarg 1

	IL_0006:  nop 
	IL_0007:  nop 
	IL_0008:  ldloc.0 
	IL_0009:  ceq 
	IL_000b:  ret 
    } // end of method EdgeCases::compareWithZero

    // method line 4
    .method public virtual hidebysig newslot 
           instance default int32 divideByOne (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x2128
	// Code size 11 (0xb)
	.maxstack 3
	.locals init (
		int32	V_0)
	IL_0000:  ldc.i4.1 
	IL_0001:  stloc.0 
	IL_0002:  ldarg 1

	IL_0006:  nop 
	IL_0007:  nop 
	IL_0008:  ldloc.0 
	IL_0009:  div 
	IL_000a:  ret 
    } // end of method EdgeCases::divideByOne

    // method line 5
    .method public virtual hidebysig newslot 
           instance default int32 multiplyByZero (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x2140
	// Code size 11 (0xb)
	.maxstack 3
	.locals init (
		int32	V_0)
	IL_0000:  ldc.i4.0 
	IL_0001:  stloc.0 
	IL_0002:  ldarg 1

	IL_0006:  nop 
	IL_0007:  nop 
	IL_0008:  ldloc.0 
	IL_0009:  mul 
	IL_000a:  ret 
    } // end of method EdgeCases::multiplyByZero

    // method line 6
    .method public virtual hidebysig newslot 
           instance default int32 checkMinMax ()  cil managed 
    {
        // Method begins at RVA 0x2158
	// Code size 18 (0x12)
	.maxstack 3
	.locals init (
		int32	V_0,
		int32	V_1,
		int32	V_2)
	IL_0000:  ldc.i4 -2147483648
	IL_0005:  stloc.0 
	IL_0006:  ldc.i4 2147483647
	IL_000b:  stloc.1 
	IL_000c:  ldloc.1 
	IL_000d:  ldloc.0 
	IL_000e:  sub 
	IL_000f:  stloc.2 
	IL_0010:  ldloc.2 
	IL_0011:  ret 
    } // end of method EdgeCases::checkMinMax

    // method line 7
    .method public virtual hidebysig newslot 
           instance default int32 testMinMaxValues ()  cil managed 
    {
        // Method begins at RVA 0x2178
	// Code size 50 (0x32)
	.maxstack 5
	.locals init (
		int32	V_0,
		int32	V_1,
		int32	V_2,
		int32	V_3,
		int32	V_4)
	IL_0000:  ldc.i4 -2147483648
	IL_0005:  stloc.0 
	IL_0006:  ldc.i4 2147483647
	IL_000b:  stloc.1 
	IL_000c:  ldc.i4.0 
	IL_000d:  stloc.2 
	IL_000e:  ldloc.0 
	IL_000f:  ldc.i4.1 
	IL_0010:  add 
	IL_0011:  stloc.3 
	IL_0012:  ldloc.1 
	IL_0013:  ldc.i4.1 
	IL_0014:  sub 
	IL_0015:  stloc.s 4
	IL_0017:  ldloc.0 
	IL_0018:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_001d:  ldloc.1 
	IL_001e:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0023:  ldloc.3 
	IL_0024:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0029:  ldloc.s 4
	IL_002b:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0030:  ldc.i4.0 
	IL_0031:  ret 
    } // end of method EdgeCases::testMinMaxValues

    // method line 8
    .method public virtual hidebysig newslot 
           instance default int32 testZeroOperations ()  cil managed 
    {
        // Method begins at RVA 0x21b8
	// Code size 55 (0x37)
	.maxstack 5
	.locals init (
		int32	V_0,
		int32	V_1,
		int32	V_2,
		int32	V_3,
		int32	V_4,
		int32	V_5,
		int32	V_6)
	IL_0000:  ldc.i4.0 
	IL_0001:  stloc.0 
	IL_0002:  ldc.i4.1 
	IL_0003:  stloc.1 
	IL_0004:  ldc.i4.s 0x0a
	IL_0006:  stloc.2 
	IL_0007:  ldloc.0 
	IL_0008:  ldloc.0 
	IL_0009:  add 
	IL_000a:  stloc.3 
	IL_000b:  ldloc.0 
	IL_000c:  ldloc.2 
	IL_000d:  mul 
	IL_000e:  stloc.s 4
	IL_0010:  ldloc.2 
	IL_0011:  ldloc.0 
	IL_0012:  mul 
	IL_0013:  stloc.s 5
	IL_0015:  ldloc.0 
	IL_0016:  ldloc.1 
	IL_0017:  div 
	IL_0018:  stloc.s 6
	IL_001a:  ldloc.3 
	IL_001b:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0020:  ldloc.s 4
	IL_0022:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0027:  ldloc.s 5
	IL_0029:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_002e:  ldloc.s 6
	IL_0030:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0035:  ldc.i4.0 
	IL_0036:  ret 
    } // end of method EdgeCases::testZeroOperations

    // method line 9
    .method public virtual hidebysig newslot 
           instance default int32 test ()  cil managed 
    {
        // Method begins at RVA 0x21fc
	// Code size 98 (0x62)
	.maxstack 19
	.locals init (
		int32	V_0,
		int32	V_1,
		bool	V_2,
		int32	V_3,
		int32	V_4,
		int32	V_5,
		int32	V_6,
		bool	V_7)
	IL_0000:  ldc.i4.s 0x2a
	IL_0002:  stloc.0 
	IL_0003:  ldarg.0 
	IL_0004:  ldloc.0 
	IL_0005:  callvirt instance int32 class EdgeCases::handleZero(int32)
	IL_000a:  stloc.1 
	IL_000b:  ldarg.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance bool class EdgeCases::compareWithZero(int32)
	IL_0012:  stloc.2 
	IL_0013:  ldarg.0 
	IL_0014:  ldloc.0 
	IL_0015:  callvirt instance int32 class EdgeCases::divideByOne(int32)
	IL_001a:  stloc.3 
	IL_001b:  ldarg.0 
	IL_001c:  ldloc.0 
	IL_001d:  callvirt instance int32 class EdgeCases::multiplyByZero(int32)
	IL_0022:  stloc.s 4
	IL_0024:  ldarg.0 
	IL_0025:  callvirt instance int32 class EdgeCases::checkMinMax()
	IL_002a:  stloc.s 5
	IL_002c:  ldloc.1 
	IL_002d:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0032:  ldloc.2 
	IL_0033:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_0038:  ldloc.3 
	IL_0039:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_003e:  ldloc.s 4
	IL_0040:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0045:  ldloc.s 5
	IL_0047:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_004c:  ldc.i4.0 
	IL_004d:  stloc.s 6
	IL_004f:  ldarg.0 
	IL_0050:  ldloc.s 6
	IL_0052:  callvirt instance bool class EdgeCases::compareWithZero(int32)
	IL_0057:  stloc.s 7
	IL_0059:  ldloc.s 7
	IL_005b:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_0060:  ldc.i4.0 
	IL_0061:  ret 
    } // end of method EdgeCases::test

  } // end of class EdgeCases

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 10
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x226c
	// Code size 26 (0x1a)
	.maxstack 6
	.locals init (
		object	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class EdgeCases::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class EdgeCases::test()
	IL_0012:  stloc.1 
	IL_0013:  ldloc.1 
	IL_0014:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0019:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 11
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2294
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method <EntryPoint>::.ctor

    // method line 12
    .method public static 
           default void Main (string[] A_0)  cil managed 
    {
        // Method begins at RVA 0x229c
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

