.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_inheritance_complex'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_inheritance_complex.exe // GUID = {4C57C90A-1114-446C-8B60-48E9B77B8092}


  .class public auto ansi A
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
    } // end of method A::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default int32 getValue ()  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 2 (0x2)
	.maxstack 8
	IL_0000:  ldc.i4.1 
	IL_0001:  ret 
    } // end of method A::getValue

    // method line 3
    .method public virtual hidebysig newslot 
           instance default int32 getBaseValue ()  cil managed 
    {
        // Method begins at RVA 0x20f8
	// Code size 3 (0x3)
	.maxstack 8
	IL_0000:  ldc.i4.s 0x0a
	IL_0002:  ret 
    } // end of method A::getBaseValue

  } // end of class A

  .class public auto ansi B
  	extends A
  {

    // method line 4
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x20fc
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class A::'.ctor'()
	IL_0006:  ret 
    } // end of method B::.ctor

    // method line 5
    .method public virtual hidebysig 
           instance default int32 getValue ()  cil managed 
    {
        // Method begins at RVA 0x2104
	// Code size 3 (0x3)
	.maxstack 8
	IL_0000:  ldc.i4.s 0x02
	IL_0002:  ret 
    } // end of method B::getValue

    // method line 6
    .method public virtual hidebysig newslot 
           instance default int32 getMiddleValue ()  cil managed 
    {
        // Method begins at RVA 0x2108
	// Code size 3 (0x3)
	.maxstack 8
	IL_0000:  ldc.i4.s 0x14
	IL_0002:  ret 
    } // end of method B::getMiddleValue

  } // end of class B

  .class public auto ansi C
  	extends B
  {

    // method line 7
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x210c
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class B::'.ctor'()
	IL_0006:  ret 
    } // end of method C::.ctor

    // method line 8
    .method public virtual hidebysig 
           instance default int32 getValue ()  cil managed 
    {
        // Method begins at RVA 0x2114
	// Code size 3 (0x3)
	.maxstack 8
	IL_0000:  ldc.i4.s 0x03
	IL_0002:  ret 
    } // end of method C::getValue

    // method line 9
    .method public virtual hidebysig newslot 
           instance default int32 getDerivedValue ()  cil managed 
    {
        // Method begins at RVA 0x2118
	// Code size 3 (0x3)
	.maxstack 8
	IL_0000:  ldc.i4.s 0x1e
	IL_0002:  ret 
    } // end of method C::getDerivedValue

  } // end of class C

  .class public auto ansi InheritanceTest
  	extends [mscorlib]System.Object
  {

    // method line 10
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x211c
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method InheritanceTest::.ctor

    // method line 11
    .method public virtual hidebysig newslot 
           instance default int32 test ()  cil managed 
    {
        // Method begins at RVA 0x2124
	// Code size 153 (0x99)
	.maxstack 28
	.locals init (
		object	V_0,
		object	V_1,
		object	V_2,
		int32	V_3,
		int32	V_4,
		int32	V_5,
		int32	V_6,
		int32	V_7,
		int32	V_8,
		int32	V_9,
		int32	V_10,
		int32	V_11)
	IL_0000:  newobj instance void class A::'.ctor'()
	IL_0005:  stloc.0 
	IL_0006:  newobj instance void class B::'.ctor'()
	IL_000b:  stloc.1 
	IL_000c:  newobj instance void class C::'.ctor'()
	IL_0011:  stloc.2 
	IL_0012:  ldloc.0 
	IL_0013:  callvirt instance int32 class A::getValue()
	IL_0018:  stloc.3 
	IL_0019:  ldloc.1 
	IL_001a:  callvirt instance int32 class B::getValue()
	IL_001f:  stloc.s 4
	IL_0021:  ldloc.2 
	IL_0022:  callvirt instance int32 class C::getValue()
	IL_0027:  stloc.s 5
	IL_0029:  ldloc.0 
	IL_002a:  callvirt instance int32 class A::getBaseValue()
	IL_002f:  stloc.s 6
	IL_0031:  ldloc.1 
	IL_0032:  callvirt instance int32 class A::getBaseValue()
	IL_0037:  stloc.s 7
	IL_0039:  ldloc.1 
	IL_003a:  callvirt instance int32 class B::getMiddleValue()
	IL_003f:  stloc.s 8
	IL_0041:  ldloc.2 
	IL_0042:  callvirt instance int32 class A::getBaseValue()
	IL_0047:  stloc.s 9
	IL_0049:  ldloc.2 
	IL_004a:  callvirt instance int32 class B::getMiddleValue()
	IL_004f:  stloc.s 10
	IL_0051:  ldloc.2 
	IL_0052:  callvirt instance int32 class C::getDerivedValue()
	IL_0057:  stloc.s 11
	IL_0059:  ldloc.3 
	IL_005a:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_005f:  ldloc.s 4
	IL_0061:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0066:  ldloc.s 5
	IL_0068:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_006d:  ldloc.s 6
	IL_006f:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0074:  ldloc.s 7
	IL_0076:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_007b:  ldloc.s 8
	IL_007d:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0082:  ldloc.s 9
	IL_0084:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0089:  ldloc.s 10
	IL_008b:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0090:  ldloc.s 11
	IL_0092:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0097:  ldc.i4.0 
	IL_0098:  ret 
    } // end of method InheritanceTest::test

  } // end of class InheritanceTest

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 12
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x21cc
	// Code size 26 (0x1a)
	.maxstack 6
	.locals init (
		object	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class InheritanceTest::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class InheritanceTest::test()
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
        // Method begins at RVA 0x21f4
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
        // Method begins at RVA 0x21fc
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

