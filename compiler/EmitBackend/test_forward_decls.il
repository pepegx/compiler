.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_forward_decls'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_forward_decls.exe // GUID = {4E23E094-DF86-443B-A4C8-13FDFA21F4B8}


  .class public auto ansi ForwardTest
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
    } // end of method ForwardTest::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default int32 B (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 18 (0x12)
	.maxstack 4
	.locals init (
		int32	V_0)
	IL_0000:  ldarg.0 
	IL_0001:  ldarg 1

	IL_0005:  nop 
	IL_0006:  nop 
	IL_0007:  ldc.i4.s 0x05
	IL_0009:  add 
	IL_000a:  callvirt instance int32 class ForwardTest::A(int32)
	IL_000f:  stloc.0 
	IL_0010:  ldloc.0 
	IL_0011:  ret 
    } // end of method ForwardTest::B

    // method line 3
    .method public virtual hidebysig newslot 
           instance default int32 A (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x2114
	// Code size 10 (0xa)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldc.i4.s 0x02
	IL_0008:  mul 
	IL_0009:  ret 
    } // end of method ForwardTest::A

    // method line 4
    .method public virtual hidebysig newslot 
           instance default int32 C (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x2120
	// Code size 30 (0x1e)
	.maxstack 7
	.locals init (
		int32	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  ldarg 1

	IL_0005:  nop 
	IL_0006:  nop 
	IL_0007:  callvirt instance int32 class ForwardTest::A(int32)
	IL_000c:  stloc.0 
	IL_000d:  ldarg.0 
	IL_000e:  ldarg 1

	IL_0012:  nop 
	IL_0013:  nop 
	IL_0014:  callvirt instance int32 class ForwardTest::B(int32)
	IL_0019:  stloc.1 
	IL_001a:  ldloc.0 
	IL_001b:  ldloc.1 
	IL_001c:  add 
	IL_001d:  ret 
    } // end of method ForwardTest::C

  } // end of class ForwardTest

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 5
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x214c
	// Code size 58 (0x3a)
	.maxstack 12
	.locals init (
		object	V_0,
		int32	V_1,
		int32	V_2,
		int32	V_3)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class ForwardTest::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  ldc.i4.s 0x0a
	IL_000f:  callvirt instance int32 class ForwardTest::A(int32)
	IL_0014:  stloc.1 
	IL_0015:  ldloc.1 
	IL_0016:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_001b:  ldloc.0 
	IL_001c:  ldc.i4.s 0x05
	IL_001e:  callvirt instance int32 class ForwardTest::B(int32)
	IL_0023:  stloc.2 
	IL_0024:  ldloc.2 
	IL_0025:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_002a:  ldloc.0 
	IL_002b:  ldc.i4.s 0x03
	IL_002d:  callvirt instance int32 class ForwardTest::C(int32)
	IL_0032:  stloc.3 
	IL_0033:  ldloc.3 
	IL_0034:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0039:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 6
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2194
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
        // Method begins at RVA 0x219c
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

