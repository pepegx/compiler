.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_polymorphic_calls'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_polymorphic_calls.exe // GUID = {E861C4D1-297C-408C-A878-1FA0C7F4C2BE}


  .class public auto ansi Shape
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
    } // end of method Shape::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default float64 getArea ()  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 3 (0x3)
	.maxstack 8
	IL_0000:  ldc.i4.0 
	IL_0001:  conv.r8 
	IL_0002:  ret 
    } // end of method Shape::getArea

  } // end of class Shape

  .class public auto ansi Circle
  	extends Shape
  {
    .field  public  float64 radius

    // method line 3
    .method public specialname rtspecialname 
           instance default void '.ctor' (float64 A_1)  cil managed 
    {
        // Method begins at RVA 0x20f8
	// Code size 34 (0x22)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class Shape::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.r8 1.
	IL_0010:  stfld float64 Circle::radius
	IL_0015:  ldarg.0 
	IL_0016:  ldarg 1

	IL_001a:  nop 
	IL_001b:  nop 
	IL_001c:  stfld float64 Circle::radius
	IL_0021:  ret 
    } // end of method Circle::.ctor

    // method line 4
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x211c
	// Code size 22 (0x16)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class Shape::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.r8 1.
	IL_0010:  stfld float64 Circle::radius
	IL_0015:  ret 
    } // end of method Circle::.ctor

    // method line 5
    .method public virtual hidebysig 
           instance default float64 getArea ()  cil managed 
    {
        // Method begins at RVA 0x2134
	// Code size 26 (0x1a)
	.maxstack 3
	.locals init (
		float64	V_0)
	IL_0000:  ldc.r8 3.1415899999999999
	IL_0009:  stloc.0 
	IL_000a:  ldarg.0 
	IL_000b:  ldfld float64 Circle::radius
	IL_0010:  ldarg.0 
	IL_0011:  ldfld float64 Circle::radius
	IL_0016:  mul 
	IL_0017:  ldloc.0 
	IL_0018:  mul 
	IL_0019:  ret 
    } // end of method Circle::getArea

  } // end of class Circle

  .class public auto ansi Square
  	extends Shape
  {
    .field  public  float64 side

    // method line 6
    .method public specialname rtspecialname 
           instance default void '.ctor' (float64 A_1)  cil managed 
    {
        // Method begins at RVA 0x215c
	// Code size 34 (0x22)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class Shape::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.r8 1.
	IL_0010:  stfld float64 Square::side
	IL_0015:  ldarg.0 
	IL_0016:  ldarg 1

	IL_001a:  nop 
	IL_001b:  nop 
	IL_001c:  stfld float64 Square::side
	IL_0021:  ret 
    } // end of method Square::.ctor

    // method line 7
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2180
	// Code size 22 (0x16)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class Shape::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.r8 1.
	IL_0010:  stfld float64 Square::side
	IL_0015:  ret 
    } // end of method Square::.ctor

    // method line 8
    .method public virtual hidebysig 
           instance default float64 getArea ()  cil managed 
    {
        // Method begins at RVA 0x2198
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld float64 Square::side
	IL_0006:  ldarg.0 
	IL_0007:  ldfld float64 Square::side
	IL_000c:  mul 
	IL_000d:  ret 
    } // end of method Square::getArea

  } // end of class Square

  .class public auto ansi Rectangle
  	extends Square
  {
    .field  public  float64 width

    // method line 9
    .method public specialname rtspecialname 
           instance default void '.ctor' (float64 A_1, float64 A_2)  cil managed 
    {
        // Method begins at RVA 0x21a8
	// Code size 46 (0x2e)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class Square::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.r8 1.
	IL_0010:  stfld float64 Rectangle::width
	IL_0015:  ldarg.0 
	IL_0016:  ldarg 2

	IL_001a:  nop 
	IL_001b:  nop 
	IL_001c:  stfld float64 Square::side
	IL_0021:  ldarg.0 
	IL_0022:  ldarg 1

	IL_0026:  nop 
	IL_0027:  nop 
	IL_0028:  stfld float64 Rectangle::width
	IL_002d:  ret 
    } // end of method Rectangle::.ctor

    // method line 10
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x21d8
	// Code size 22 (0x16)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class Square::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.r8 1.
	IL_0010:  stfld float64 Rectangle::width
	IL_0015:  ret 
    } // end of method Rectangle::.ctor

    // method line 11
    .method public virtual hidebysig 
           instance default float64 getArea ()  cil managed 
    {
        // Method begins at RVA 0x21f0
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld float64 Square::side
	IL_0006:  ldarg.0 
	IL_0007:  ldfld float64 Rectangle::width
	IL_000c:  mul 
	IL_000d:  ret 
    } // end of method Rectangle::getArea

  } // end of class Rectangle

  .class public auto ansi PolymorphicTest
  	extends [mscorlib]System.Object
  {

    // method line 12
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2200
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method PolymorphicTest::.ctor

    // method line 13
    .method public virtual hidebysig newslot 
           instance default int32 testPolymorphism ()  cil managed 
    {
        // Method begins at RVA 0x2208
	// Code size 111 (0x6f)
	.maxstack 10
	.locals init (
		object	V_0,
		object	V_1,
		object	V_2,
		object	V_3,
		object	V_4,
		object	V_5,
		float64	V_6,
		float64	V_7,
		float64	V_8)
	IL_0000:  ldc.r8 2.
	IL_0009:  newobj instance void class Circle::'.ctor'(float64)
	IL_000e:  stloc.0 
	IL_000f:  ldc.r8 3.
	IL_0018:  newobj instance void class Square::'.ctor'(float64)
	IL_001d:  stloc.1 
	IL_001e:  ldc.r8 4.
	IL_0027:  ldc.r8 5.
	IL_0030:  newobj instance void class Rectangle::'.ctor'(float64, float64)
	IL_0035:  stloc.2 
	IL_0036:  ldloc.0 
	IL_0037:  stloc.3 
	IL_0038:  ldloc.1 
	IL_0039:  stloc.s 4
	IL_003b:  ldloc.2 
	IL_003c:  stloc.s 5
	IL_003e:  ldloc.3 
	IL_003f:  callvirt instance float64 class Circle::getArea()
	IL_0044:  stloc.s 6
	IL_0046:  ldloc.s 4
	IL_0048:  callvirt instance float64 class Square::getArea()
	IL_004d:  stloc.s 7
	IL_004f:  ldloc.s 5
	IL_0051:  callvirt instance float64 class Rectangle::getArea()
	IL_0056:  stloc.s 8
	IL_0058:  ldloc.s 6
	IL_005a:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_005f:  ldloc.s 7
	IL_0061:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0066:  ldloc.s 8
	IL_0068:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_006d:  ldc.i4.0 
	IL_006e:  ret 
    } // end of method PolymorphicTest::testPolymorphism

    // method line 14
    .method public virtual hidebysig newslot 
           instance default int32 testDirectCalls ()  cil managed 
    {
        // Method begins at RVA 0x2284
	// Code size 99 (0x63)
	.maxstack 10
	.locals init (
		object	V_0,
		object	V_1,
		object	V_2,
		float64	V_3,
		float64	V_4,
		float64	V_5)
	IL_0000:  ldc.r8 1.5
	IL_0009:  newobj instance void class Circle::'.ctor'(float64)
	IL_000e:  stloc.0 
	IL_000f:  ldc.r8 2.5
	IL_0018:  newobj instance void class Square::'.ctor'(float64)
	IL_001d:  stloc.1 
	IL_001e:  ldc.r8 3.
	IL_0027:  ldc.r8 4.
	IL_0030:  newobj instance void class Rectangle::'.ctor'(float64, float64)
	IL_0035:  stloc.2 
	IL_0036:  ldloc.0 
	IL_0037:  callvirt instance float64 class Circle::getArea()
	IL_003c:  stloc.3 
	IL_003d:  ldloc.1 
	IL_003e:  callvirt instance float64 class Square::getArea()
	IL_0043:  stloc.s 4
	IL_0045:  ldloc.2 
	IL_0046:  callvirt instance float64 class Rectangle::getArea()
	IL_004b:  stloc.s 5
	IL_004d:  ldloc.3 
	IL_004e:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0053:  ldloc.s 4
	IL_0055:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_005a:  ldloc.s 5
	IL_005c:  call void class [mscorlib]System.Console::WriteLine(float64)
	IL_0061:  ldc.i4.0 
	IL_0062:  ret 
    } // end of method PolymorphicTest::testDirectCalls

    // method line 15
    .method public virtual hidebysig newslot 
           instance default int32 test ()  cil managed 
    {
        // Method begins at RVA 0x22f4
	// Code size 2 (0x2)
	.maxstack 8
	IL_0000:  ldc.i4.0 
	IL_0001:  ret 
    } // end of method PolymorphicTest::test

  } // end of class PolymorphicTest

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 16
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x22f8
	// Code size 26 (0x1a)
	.maxstack 6
	.locals init (
		object	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class PolymorphicTest::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class PolymorphicTest::test()
	IL_0012:  stloc.1 
	IL_0013:  ldloc.1 
	IL_0014:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0019:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 17
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2320
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method <EntryPoint>::.ctor

    // method line 18
    .method public static 
           default void Main (string[] A_0)  cil managed 
    {
        // Method begins at RVA 0x2328
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

