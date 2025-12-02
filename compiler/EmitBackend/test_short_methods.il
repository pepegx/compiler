.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_short_methods'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_short_methods.exe // GUID = {64EE4121-5159-4C91-9F8E-8E5C692B4FB0}


  .class public auto ansi ShortMethods
  	extends [mscorlib]System.Object
  {
    .field  public  int32 x
    .field  public  int32 y

    // method line 1
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x20ec
	// Code size 23 (0x17)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.i4.s 0x0a
	IL_0009:  stfld int32 ShortMethods::x
	IL_000e:  ldarg.0 
	IL_000f:  ldc.i4.s 0x14
	IL_0011:  stfld int32 ShortMethods::y
	IL_0016:  ret 
    } // end of method ShortMethods::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default int32 getX ()  cil managed 
    {
        // Method begins at RVA 0x2104
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld int32 ShortMethods::x
	IL_0006:  ret 
    } // end of method ShortMethods::getX

    // method line 3
    .method public virtual hidebysig newslot 
           instance default int32 getY ()  cil managed 
    {
        // Method begins at RVA 0x210c
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld int32 ShortMethods::y
	IL_0006:  ret 
    } // end of method ShortMethods::getY

    // method line 4
    .method public virtual hidebysig newslot 
           instance default int32 getXLong ()  cil managed 
    {
        // Method begins at RVA 0x2114
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld int32 ShortMethods::x
	IL_0006:  ret 
    } // end of method ShortMethods::getXLong

    // method line 5
    .method public virtual hidebysig newslot 
           instance default int32 getYLong ()  cil managed 
    {
        // Method begins at RVA 0x211c
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld int32 ShortMethods::y
	IL_0006:  ret 
    } // end of method ShortMethods::getYLong

    // method line 6
    .method public virtual hidebysig newslot 
           instance default int32 double (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x2124
	// Code size 10 (0xa)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldc.i4.s 0x02
	IL_0008:  mul 
	IL_0009:  ret 
    } // end of method ShortMethods::double

    // method line 7
    .method public virtual hidebysig newslot 
           instance default int32 doubleLong (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x2130
	// Code size 10 (0xa)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldc.i4.s 0x02
	IL_0008:  mul 
	IL_0009:  ret 
    } // end of method ShortMethods::doubleLong

    // method line 8
    .method public virtual hidebysig newslot 
           instance default int32 'add' (int32 A_1, int32 A_2)  cil managed 
    {
        // Method begins at RVA 0x213c
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
    } // end of method ShortMethods::add

    // method line 9
    .method public virtual hidebysig newslot 
           instance default int32 addLong (int32 A_1, int32 A_2)  cil managed 
    {
        // Method begins at RVA 0x214c
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
    } // end of method ShortMethods::addLong

    // method line 10
    .method public virtual hidebysig newslot 
           instance default int32 square (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x215c
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldarg 1

	IL_000a:  nop 
	IL_000b:  nop 
	IL_000c:  mul 
	IL_000d:  ret 
    } // end of method ShortMethods::square

    // method line 11
    .method public virtual hidebysig newslot 
           instance default int32 squareLong (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x216c
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldarg 1

	IL_000a:  nop 
	IL_000b:  nop 
	IL_000c:  mul 
	IL_000d:  ret 
    } // end of method ShortMethods::squareLong

  } // end of class ShortMethods

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 12
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x217c
	// Code size 173 (0xad)
	.maxstack 33
	.locals init (
		object	V_0,
		int32	V_1,
		int32	V_2,
		int32	V_3,
		int32	V_4,
		int32	V_5,
		int32	V_6,
		int32	V_7,
		int32	V_8,
		int32	V_9,
		int32	V_10)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class ShortMethods::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class ShortMethods::getX()
	IL_0012:  stloc.1 
	IL_0013:  ldloc.0 
	IL_0014:  callvirt instance int32 class ShortMethods::getXLong()
	IL_0019:  stloc.2 
	IL_001a:  ldloc.1 
	IL_001b:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0020:  ldloc.2 
	IL_0021:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0026:  ldloc.0 
	IL_0027:  callvirt instance int32 class ShortMethods::getY()
	IL_002c:  stloc.3 
	IL_002d:  ldloc.0 
	IL_002e:  callvirt instance int32 class ShortMethods::getYLong()
	IL_0033:  stloc.s 4
	IL_0035:  ldloc.3 
	IL_0036:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_003b:  ldloc.s 4
	IL_003d:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0042:  ldloc.0 
	IL_0043:  ldc.i4.s 0x05
	IL_0045:  callvirt instance int32 class ShortMethods::double(int32)
	IL_004a:  stloc.s 5
	IL_004c:  ldloc.0 
	IL_004d:  ldc.i4.s 0x05
	IL_004f:  callvirt instance int32 class ShortMethods::doubleLong(int32)
	IL_0054:  stloc.s 6
	IL_0056:  ldloc.s 5
	IL_0058:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_005d:  ldloc.s 6
	IL_005f:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0064:  ldloc.0 
	IL_0065:  ldc.i4.s 0x03
	IL_0067:  ldc.i4.s 0x04
	IL_0069:  callvirt instance int32 class ShortMethods::'add'(int32, int32)
	IL_006e:  stloc.s 7
	IL_0070:  ldloc.0 
	IL_0071:  ldc.i4.s 0x03
	IL_0073:  ldc.i4.s 0x04
	IL_0075:  callvirt instance int32 class ShortMethods::addLong(int32, int32)
	IL_007a:  stloc.s 8
	IL_007c:  ldloc.s 7
	IL_007e:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0083:  ldloc.s 8
	IL_0085:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_008a:  ldloc.0 
	IL_008b:  ldc.i4.s 0x06
	IL_008d:  callvirt instance int32 class ShortMethods::square(int32)
	IL_0092:  stloc.s 9
	IL_0094:  ldloc.0 
	IL_0095:  ldc.i4.s 0x06
	IL_0097:  callvirt instance int32 class ShortMethods::squareLong(int32)
	IL_009c:  stloc.s 10
	IL_009e:  ldloc.s 9
	IL_00a0:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_00a5:  ldloc.s 10
	IL_00a7:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_00ac:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 13
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2238
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
        // Method begins at RVA 0x2240
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

