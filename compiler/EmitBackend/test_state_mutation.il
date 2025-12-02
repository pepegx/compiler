.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_state_mutation'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_state_mutation.exe // GUID = {6B4E514F-A1A1-4B99-8DD5-85DDE95A3EE6}


  .class public auto ansi Counter
  	extends [mscorlib]System.Object
  {
    .field  public  int32 'value'

    // method line 1
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x20ec
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.i4.0 
	IL_0008:  stfld int32 Counter::'value'
	IL_000d:  ret 
    } // end of method Counter::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default void increment ()  cil managed 
    {
        // Method begins at RVA 0x20fc
	// Code size 15 (0xf)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.0 
	IL_0002:  ldfld int32 Counter::'value'
	IL_0007:  ldc.i4.1 
	IL_0008:  add 
	IL_0009:  stfld int32 Counter::'value'
	IL_000e:  ret 
    } // end of method Counter::increment

    // method line 3
    .method public virtual hidebysig newslot 
           instance default void decrement ()  cil managed 
    {
        // Method begins at RVA 0x210c
	// Code size 15 (0xf)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.0 
	IL_0002:  ldfld int32 Counter::'value'
	IL_0007:  ldc.i4.1 
	IL_0008:  sub 
	IL_0009:  stfld int32 Counter::'value'
	IL_000e:  ret 
    } // end of method Counter::decrement

    // method line 4
    .method public virtual hidebysig newslot 
           instance default void 'add' (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x211c
	// Code size 20 (0x14)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.0 
	IL_0002:  ldfld int32 Counter::'value'
	IL_0007:  ldarg 1

	IL_000b:  nop 
	IL_000c:  nop 
	IL_000d:  add 
	IL_000e:  stfld int32 Counter::'value'
	IL_0013:  ret 
    } // end of method Counter::add

    // method line 5
    .method public virtual hidebysig newslot 
           instance default void multiply (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x2134
	// Code size 20 (0x14)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldarg.0 
	IL_0002:  ldfld int32 Counter::'value'
	IL_0007:  ldarg 1

	IL_000b:  nop 
	IL_000c:  nop 
	IL_000d:  mul 
	IL_000e:  stfld int32 Counter::'value'
	IL_0013:  ret 
    } // end of method Counter::multiply

    // method line 6
    .method public virtual hidebysig newslot 
           instance default int32 getValue ()  cil managed 
    {
        // Method begins at RVA 0x214c
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld int32 Counter::'value'
	IL_0006:  ret 
    } // end of method Counter::getValue

  } // end of class Counter

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 7
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2154
	// Code size 110 (0x6e)
	.maxstack 26
	.locals init (
		object	V_0,
		int32	V_1,
		int32	V_2,
		int32	V_3,
		int32	V_4,
		int32	V_5)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class Counter::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class Counter::getValue()
	IL_0012:  stloc.1 
	IL_0013:  ldloc.1 
	IL_0014:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0019:  ldloc.0 
	IL_001a:  callvirt instance void class Counter::increment()
	IL_001f:  ldloc.0 
	IL_0020:  callvirt instance int32 class Counter::getValue()
	IL_0025:  stloc.2 
	IL_0026:  ldloc.2 
	IL_0027:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_002c:  ldloc.0 
	IL_002d:  callvirt instance void class Counter::decrement()
	IL_0032:  ldloc.0 
	IL_0033:  callvirt instance int32 class Counter::getValue()
	IL_0038:  stloc.3 
	IL_0039:  ldloc.3 
	IL_003a:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_003f:  ldloc.0 
	IL_0040:  ldc.i4.s 0x05
	IL_0042:  callvirt instance void class Counter::'add'(int32)
	IL_0047:  ldloc.0 
	IL_0048:  callvirt instance int32 class Counter::getValue()
	IL_004d:  stloc.s 4
	IL_004f:  ldloc.s 4
	IL_0051:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0056:  ldloc.0 
	IL_0057:  ldc.i4.s 0x02
	IL_0059:  callvirt instance void class Counter::multiply(int32)
	IL_005e:  ldloc.0 
	IL_005f:  callvirt instance int32 class Counter::getValue()
	IL_0064:  stloc.s 5
	IL_0066:  ldloc.s 5
	IL_0068:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_006d:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 8
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x21d0
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method <EntryPoint>::.ctor

    // method line 9
    .method public static 
           default void Main (string[] A_0)  cil managed 
    {
        // Method begins at RVA 0x21d8
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

