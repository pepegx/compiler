.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_conversions'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_conversions.exe // GUID = {C9436DC7-C775-4867-A6E8-FB2087A591D7}


  .class public auto ansi ConversionOperations
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
    } // end of method ConversionOperations::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default float64 intToReal (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  conv.r8 
	IL_0007:  ret 
    } // end of method ConversionOperations::intToReal

    // method line 3
    .method public virtual hidebysig newslot 
           instance default int32 realToInt (float64 A_1)  cil managed 
    {
        // Method begins at RVA 0x2100
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  conv.i4 
	IL_0007:  ret 
    } // end of method ConversionOperations::realToInt

    // method line 4
    .method public virtual hidebysig newslot 
           instance default bool intToBool (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x210c
	// Code size 10 (0xa)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldc.i4.0 
	IL_0007:  cgt.un 
	IL_0009:  ret 
    } // end of method ConversionOperations::intToBool

    // method line 5
    .method public virtual hidebysig newslot 
           instance default int32 boolToInt (bool A_1)  cil managed 
    {
        // Method begins at RVA 0x2118
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ret 
    } // end of method ConversionOperations::boolToInt

    // method line 6
    .method public virtual hidebysig newslot 
           instance default int32 testConversions ()  cil managed 
    {
        // Method begins at RVA 0x2120
	// Code size 144 (0x90)
	.maxstack 22
	.locals init (
		int32	V_0,
		float64	V_1,
		bool	V_2,
		bool	V_3,
		float64	V_4,
		int32	V_5,
		bool	V_6,
		int32	V_7,
		int32	V_8,
		int32	V_9,
		bool	V_10,
		int32	V_11)
	IL_0000:  ldc.i4.s 0x2a
	IL_0002:  stloc.0 
	IL_0003:  ldc.r8 3.1400000000000001
	IL_000c:  stloc.1 
	IL_000d:  ldc.i4 1
	IL_0012:  stloc.2 
	IL_0013:  ldc.i4 0
	IL_0018:  stloc.3 
	IL_0019:  ldarg.0 
	IL_001a:  ldloc.0 
	IL_001b:  callvirt instance float64 class ConversionOperations::intToReal(int32)
	IL_0020:  stloc.s 4
	IL_0022:  ldarg.0 
	IL_0023:  ldloc.1 
	IL_0024:  callvirt instance int32 class ConversionOperations::realToInt(float64)
	IL_0029:  stloc.s 5
	IL_002b:  ldarg.0 
	IL_002c:  ldloc.0 
	IL_002d:  callvirt instance bool class ConversionOperations::intToBool(int32)
	IL_0032:  stloc.s 6
	IL_0034:  ldarg.0 
	IL_0035:  ldloc.2 
	IL_0036:  callvirt instance int32 class ConversionOperations::boolToInt(bool)
	IL_003b:  stloc.s 7
	IL_003d:  ldarg.0 
	IL_003e:  ldloc.3 
	IL_003f:  callvirt instance int32 class ConversionOperations::boolToInt(bool)
	IL_0044:  stloc.s 8
	IL_0046:  ldc.i4.0 
	IL_0047:  stloc.s 9
	IL_0049:  ldarg.0 
	IL_004a:  ldloc.s 9
	IL_004c:  callvirt instance bool class ConversionOperations::intToBool(int32)
	IL_0051:  stloc.s 10
	IL_0053:  ldarg.0 
	IL_0054:  ldloc.s 10
	IL_0056:  callvirt instance int32 class ConversionOperations::boolToInt(bool)
	IL_005b:  stloc.s 11
	IL_005d:  ldloc.s 4
	IL_005f:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0064:  ldloc.s 5
	IL_0066:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_006b:  ldloc.s 6
	IL_006d:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_0072:  ldloc.s 7
	IL_0074:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0079:  ldloc.s 8
	IL_007b:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0080:  ldloc.s 10
	IL_0082:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_0087:  ldloc.s 11
	IL_0089:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_008e:  ldc.i4.0 
	IL_008f:  ret 
    } // end of method ConversionOperations::testConversions

    // method line 7
    .method public virtual hidebysig newslot 
           instance default int32 testWithOperations ()  cil managed 
    {
        // Method begins at RVA 0x21bc
	// Code size 60 (0x3c)
	.maxstack 5
	.locals init (
		int32	V_0,
		float64	V_1,
		float64	V_2,
		int32	V_3,
		bool	V_4,
		int32	V_5)
	IL_0000:  ldc.i4.s 0x0a
	IL_0002:  stloc.0 
	IL_0003:  ldc.r8 2.5
	IL_000c:  stloc.1 
	IL_000d:  ldloc.0 
	IL_000e:  conv.r8 
	IL_000f:  ldloc.1 
	IL_0010:  add 
	IL_0011:  stloc.2 
	IL_0012:  ldloc.2 
	IL_0013:  conv.i4 
	IL_0014:  stloc.3 
	IL_0015:  ldloc.2 
	IL_0016:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_001b:  ldloc.3 
	IL_001c:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0021:  ldloc.0 
	IL_0022:  ldc.i4.s 0x05
	IL_0024:  cgt 
	IL_0026:  stloc.s 4
	IL_0028:  ldloc.s 4
	IL_002a:  stloc.s 5
	IL_002c:  ldloc.s 4
	IL_002e:  call void class [mscorlib]System.Console::WriteLine(bool)
	IL_0033:  ldloc.s 5
	IL_0035:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_003a:  ldc.i4.0 
	IL_003b:  ret 
    } // end of method ConversionOperations::testWithOperations

    // method line 8
    .method public virtual hidebysig newslot 
           instance default int32 test ()  cil managed 
    {
        // Method begins at RVA 0x2204
	// Code size 20 (0x14)
	.maxstack 7
	.locals init (
		int32	V_0,
		int32	V_1,
		int32	V_2)
	IL_0000:  ldarg.0 
	IL_0001:  callvirt instance int32 class ConversionOperations::testConversions()
	IL_0006:  stloc.0 
	IL_0007:  ldarg.0 
	IL_0008:  callvirt instance int32 class ConversionOperations::testWithOperations()
	IL_000d:  stloc.1 
	IL_000e:  ldloc.0 
	IL_000f:  ldloc.1 
	IL_0010:  add 
	IL_0011:  stloc.2 
	IL_0012:  ldloc.2 
	IL_0013:  ret 
    } // end of method ConversionOperations::test

  } // end of class ConversionOperations

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 9
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2224
	// Code size 26 (0x1a)
	.maxstack 6
	.locals init (
		object	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class ConversionOperations::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class ConversionOperations::test()
	IL_0012:  stloc.1 
	IL_0013:  ldloc.1 
	IL_0014:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0019:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 10
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x224c
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method <EntryPoint>::.ctor

    // method line 11
    .method public static 
           default void Main (string[] A_0)  cil managed 
    {
        // Method begins at RVA 0x2254
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

