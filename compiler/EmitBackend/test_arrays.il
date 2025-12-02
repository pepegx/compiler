.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_arrays'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_arrays.exe // GUID = {6CA78AEB-1EDE-4D20-A594-BD180C45382B}


  .class public auto ansi ArrayOperations
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
    } // end of method ArrayOperations::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default int32 testIntegerArray ()  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 121 (0x79)
	.maxstack 5
	.locals init (
		object[]	V_0,
		int32	V_1,
		int32	V_2,
		int32	V_3,
		int32	V_4)
	IL_0000:  ldc.i4.s 0x05
	IL_0002:  newarr [mscorlib]System.Object
	IL_0007:  stloc.0 
	IL_0008:  ldloc.0 
	IL_0009:  ldc.i4.0 
	IL_000a:  ldc.i4.s 0x0a
	IL_000c:  box [mscorlib]System.Int32
	IL_0011:  stelem.ref 
	IL_0012:  ldloc.0 
	IL_0013:  ldc.i4.1 
	IL_0014:  ldc.i4.s 0x14
	IL_0016:  box [mscorlib]System.Int32
	IL_001b:  stelem.ref 
	IL_001c:  ldloc.0 
	IL_001d:  ldc.i4.s 0x02
	IL_001f:  ldc.i4.s 0x1e
	IL_0021:  box [mscorlib]System.Int32
	IL_0026:  stelem.ref 
	IL_0027:  ldloc.0 
	IL_0028:  ldc.i4.s 0x03
	IL_002a:  ldc.i4.s 0x28
	IL_002c:  box [mscorlib]System.Int32
	IL_0031:  stelem.ref 
	IL_0032:  ldloc.0 
	IL_0033:  ldc.i4.s 0x04
	IL_0035:  ldc.i4.s 0x32
	IL_0037:  box [mscorlib]System.Int32
	IL_003c:  stelem.ref 
	IL_003d:  ldloc.0 
	IL_003e:  ldc.i4.0 
	IL_003f:  ldelem.ref 
	IL_0040:  unbox.any [mscorlib]System.Int32
	IL_0045:  stloc.1 
	IL_0046:  ldloc.0 
	IL_0047:  ldc.i4.1 
	IL_0048:  ldelem.ref 
	IL_0049:  unbox.any [mscorlib]System.Int32
	IL_004e:  stloc.2 
	IL_004f:  ldloc.0 
	IL_0050:  ldc.i4.s 0x02
	IL_0052:  ldelem.ref 
	IL_0053:  unbox.any [mscorlib]System.Int32
	IL_0058:  stloc.3 
	IL_0059:  ldloc.0 
	IL_005a:  ldlen 
	IL_005b:  conv.i4 
	IL_005c:  stloc.s 4
	IL_005e:  ldloc.1 
	IL_005f:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0064:  ldloc.2 
	IL_0065:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_006a:  ldloc.3 
	IL_006b:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0070:  ldloc.s 4
	IL_0072:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0077:  ldc.i4.0 
	IL_0078:  ret 
    } // end of method ArrayOperations::testIntegerArray

    // method line 3
    .method public virtual hidebysig newslot 
           instance default int32 testRealArray ()  cil managed 
    {
        // Method begins at RVA 0x217c
	// Code size 102 (0x66)
	.maxstack 4
	.locals init (
		object[]	V_0,
		float64	V_1,
		float64	V_2,
		int32	V_3)
	IL_0000:  ldc.i4.s 0x03
	IL_0002:  newarr [mscorlib]System.Object
	IL_0007:  stloc.0 
	IL_0008:  ldloc.0 
	IL_0009:  ldc.i4.0 
	IL_000a:  ldc.r8 1.5
	IL_0013:  box [mscorlib]System.Double
	IL_0018:  stelem.ref 
	IL_0019:  ldloc.0 
	IL_001a:  ldc.i4.1 
	IL_001b:  ldc.r8 2.7000000000000002
	IL_0024:  box [mscorlib]System.Double
	IL_0029:  stelem.ref 
	IL_002a:  ldloc.0 
	IL_002b:  ldc.i4.s 0x02
	IL_002d:  ldc.r8 3.8999999999999999
	IL_0036:  box [mscorlib]System.Double
	IL_003b:  stelem.ref 
	IL_003c:  ldloc.0 
	IL_003d:  ldc.i4.0 
	IL_003e:  ldelem.ref 
	IL_003f:  unbox.any [mscorlib]System.Double
	IL_0044:  stloc.1 
	IL_0045:  ldloc.0 
	IL_0046:  ldc.i4.1 
	IL_0047:  ldelem.ref 
	IL_0048:  unbox.any [mscorlib]System.Double
	IL_004d:  stloc.2 
	IL_004e:  ldloc.0 
	IL_004f:  ldlen 
	IL_0050:  conv.i4 
	IL_0051:  stloc.3 
	IL_0052:  ldloc.1 
	IL_0053:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0058:  ldloc.2 
	IL_0059:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_005e:  ldloc.3 
	IL_005f:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0064:  ldc.i4.0 
	IL_0065:  ret 
    } // end of method ArrayOperations::testRealArray

    // method line 4
    .method public virtual hidebysig newslot 
           instance default int32 testArrayModification ()  cil managed 
    {
        // Method begins at RVA 0x21f0
	// Code size 93 (0x5d)
	.maxstack 5
	.locals init (
		object[]	V_0,
		int32	V_1,
		int32	V_2,
		int32	V_3)
	IL_0000:  ldc.i4.s 0x04
	IL_0002:  newarr [mscorlib]System.Object
	IL_0007:  stloc.0 
	IL_0008:  ldc.i4.0 
	IL_0009:  stloc.1 
	IL_000a:  ldloc.1 
	IL_000b:  ldloc.0 
	IL_000c:  ldlen 
	IL_000d:  conv.i4 
	IL_000e:  clt 
	IL_0010:  brfalse IL_002a

	IL_0015:  ldloc.0 
	IL_0016:  ldloc.1 
	IL_0017:  ldloc.1 
	IL_0018:  ldc.i4.s 0x05
	IL_001a:  mul 
	IL_001b:  box [mscorlib]System.Int32
	IL_0020:  stelem.ref 
	IL_0021:  ldloc.1 
	IL_0022:  ldc.i4.1 
	IL_0023:  add 
	IL_0024:  stloc.1 
	IL_0025:  br IL_000a

	IL_002a:  ldc.i4.0 
	IL_002b:  stloc.2 
	IL_002c:  ldc.i4.0 
	IL_002d:  stloc.1 
	IL_002e:  ldloc.1 
	IL_002f:  ldloc.0 
	IL_0030:  ldlen 
	IL_0031:  conv.i4 
	IL_0032:  clt 
	IL_0034:  brfalse IL_0055

	IL_0039:  ldloc.0 
	IL_003a:  ldloc.1 
	IL_003b:  ldelem.ref 
	IL_003c:  unbox.any [mscorlib]System.Int32
	IL_0041:  stloc.3 
	IL_0042:  ldloc.2 
	IL_0043:  ldloc.3 
	IL_0044:  add 
	IL_0045:  stloc.2 
	IL_0046:  ldloc.3 
	IL_0047:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_004c:  ldloc.1 
	IL_004d:  ldc.i4.1 
	IL_004e:  add 
	IL_004f:  stloc.1 
	IL_0050:  br IL_002e

	IL_0055:  ldloc.2 
	IL_0056:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_005b:  ldc.i4.0 
	IL_005c:  ret 
    } // end of method ArrayOperations::testArrayModification

    // method line 5
    .method public virtual hidebysig newslot 
           instance default int32 test ()  cil managed 
    {
        // Method begins at RVA 0x225c
	// Code size 29 (0x1d)
	.maxstack 9
	.locals init (
		int32	V_0,
		int32	V_1,
		int32	V_2,
		int32	V_3)
	IL_0000:  ldarg.0 
	IL_0001:  callvirt instance int32 class ArrayOperations::testIntegerArray()
	IL_0006:  stloc.0 
	IL_0007:  ldarg.0 
	IL_0008:  callvirt instance int32 class ArrayOperations::testRealArray()
	IL_000d:  stloc.1 
	IL_000e:  ldarg.0 
	IL_000f:  callvirt instance int32 class ArrayOperations::testArrayModification()
	IL_0014:  stloc.2 
	IL_0015:  ldloc.0 
	IL_0016:  ldloc.1 
	IL_0017:  add 
	IL_0018:  ldloc.2 
	IL_0019:  add 
	IL_001a:  stloc.3 
	IL_001b:  ldloc.3 
	IL_001c:  ret 
    } // end of method ArrayOperations::test

  } // end of class ArrayOperations

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 6
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2288
	// Code size 26 (0x1a)
	.maxstack 6
	.locals init (
		object	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class ArrayOperations::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class ArrayOperations::test()
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
        // Method begins at RVA 0x22b0
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
        // Method begins at RVA 0x22b8
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

