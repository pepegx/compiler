.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_lists'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_lists.exe // GUID = {6744DA61-5C88-46FF-B7DD-97015D067AE2}


  .class public auto ansi ListOperations
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
    } // end of method ListOperations::.ctor

    // method line 2
    .method public virtual hidebysig newslot 
           instance default class [mscorlib]System.Collections.Generic.List`1<object> createList ()  cil managed 
    {
        // Method begins at RVA 0x20f4
	// Code size 68 (0x44)
	.maxstack 10
	.locals init (
		class [mscorlib]System.Collections.Generic.List`1<object>	V_0)
	IL_0000:  newobj instance void class [mscorlib]System.Collections.Generic.List`1<object>::'.ctor'()
	IL_0005:  stloc.0 
	IL_0006:  ldloc.0 
	IL_0007:  dup 
	IL_0008:  ldc.i4.s 0x05
	IL_000a:  box [mscorlib]System.Int32
	IL_000f:  callvirt instance void class [mscorlib]System.Collections.Generic.List`1<object>::Add(!0)
	IL_0014:  stloc.0 
	IL_0015:  ldloc.0 
	IL_0016:  dup 
	IL_0017:  ldc.i4.s 0x0a
	IL_0019:  box [mscorlib]System.Int32
	IL_001e:  callvirt instance void class [mscorlib]System.Collections.Generic.List`1<object>::Add(!0)
	IL_0023:  stloc.0 
	IL_0024:  ldloc.0 
	IL_0025:  dup 
	IL_0026:  ldc.i4.s 0x0f
	IL_0028:  box [mscorlib]System.Int32
	IL_002d:  callvirt instance void class [mscorlib]System.Collections.Generic.List`1<object>::Add(!0)
	IL_0032:  stloc.0 
	IL_0033:  ldloc.0 
	IL_0034:  dup 
	IL_0035:  ldc.i4.s 0x14
	IL_0037:  box [mscorlib]System.Int32
	IL_003c:  callvirt instance void class [mscorlib]System.Collections.Generic.List`1<object>::Add(!0)
	IL_0041:  stloc.0 
	IL_0042:  ldloc.0 
	IL_0043:  ret 
    } // end of method ListOperations::createList

    // method line 3
    .method public virtual hidebysig newslot 
           instance default int32 sumList (class [mscorlib]System.Collections.Generic.List`1<object> A_1)  cil managed 
    {
        // Method begins at RVA 0x2144
	// Code size 63 (0x3f)
	.maxstack 11
	.locals init (
		int32	V_0,
		class [mscorlib]System.Collections.Generic.List`1<object>	V_1,
		int32	V_2)
	IL_0000:  ldc.i4.0 
	IL_0001:  stloc.0 
	IL_0002:  ldarg 1

	IL_0006:  nop 
	IL_0007:  nop 
	IL_0008:  stloc.1 
	IL_0009:  ldloc.1 
	IL_000a:  callvirt instance int32 class [mscorlib]System.Collections.Generic.List`1<object>::get_Count()
	IL_000f:  ldc.i4.0 
	IL_0010:  cgt 
	IL_0012:  brfalse IL_003d

	IL_0017:  ldloc.1 
	IL_0018:  ldc.i4.0 
	IL_0019:  callvirt instance !0 class [mscorlib]System.Collections.Generic.List`1<object>::get_Item(int32)
	IL_001e:  unbox.any [mscorlib]System.Int32
	IL_0023:  stloc.2 
	IL_0024:  ldloc.0 
	IL_0025:  ldloc.2 
	IL_0026:  add 
	IL_0027:  stloc.0 
	IL_0028:  ldloc.1 
	IL_0029:  ldc.i4.1 
	IL_002a:  ldloc.1 
	IL_002b:  callvirt instance int32 class [mscorlib]System.Collections.Generic.List`1<object>::get_Count()
	IL_0030:  ldc.i4.1 
	IL_0031:  sub 
	IL_0032:  callvirt instance class [mscorlib]System.Collections.Generic.List`1<!0> class [mscorlib]System.Collections.Generic.List`1<object>::GetRange(int32, int32)
	IL_0037:  stloc.1 
	IL_0038:  br IL_0009

	IL_003d:  ldloc.0 
	IL_003e:  ret 
    } // end of method ListOperations::sumList

    // method line 4
    .method public virtual hidebysig newslot 
           instance default int32 countElements (class [mscorlib]System.Collections.Generic.List`1<object> A_1)  cil managed 
    {
        // Method begins at RVA 0x2190
	// Code size 50 (0x32)
	.maxstack 9
	.locals init (
		int32	V_0,
		class [mscorlib]System.Collections.Generic.List`1<object>	V_1)
	IL_0000:  ldc.i4.0 
	IL_0001:  stloc.0 
	IL_0002:  ldarg 1

	IL_0006:  nop 
	IL_0007:  nop 
	IL_0008:  stloc.1 
	IL_0009:  ldloc.1 
	IL_000a:  callvirt instance int32 class [mscorlib]System.Collections.Generic.List`1<object>::get_Count()
	IL_000f:  ldc.i4.0 
	IL_0010:  cgt 
	IL_0012:  brfalse IL_0030

	IL_0017:  ldloc.0 
	IL_0018:  ldc.i4.1 
	IL_0019:  add 
	IL_001a:  stloc.0 
	IL_001b:  ldloc.1 
	IL_001c:  ldc.i4.1 
	IL_001d:  ldloc.1 
	IL_001e:  callvirt instance int32 class [mscorlib]System.Collections.Generic.List`1<object>::get_Count()
	IL_0023:  ldc.i4.1 
	IL_0024:  sub 
	IL_0025:  callvirt instance class [mscorlib]System.Collections.Generic.List`1<!0> class [mscorlib]System.Collections.Generic.List`1<object>::GetRange(int32, int32)
	IL_002a:  stloc.1 
	IL_002b:  br IL_0009

	IL_0030:  ldloc.0 
	IL_0031:  ret 
    } // end of method ListOperations::countElements

    // method line 5
    .method public virtual hidebysig newslot 
           instance default int32 firstElement (class [mscorlib]System.Collections.Generic.List`1<object> A_1)  cil managed 
    {
        // Method begins at RVA 0x21d0
	// Code size 18 (0x12)
	.maxstack 8
	IL_0000:  ldarg 1

	IL_0004:  nop 
	IL_0005:  nop 
	IL_0006:  ldc.i4.0 
	IL_0007:  callvirt instance !0 class [mscorlib]System.Collections.Generic.List`1<object>::get_Item(int32)
	IL_000c:  unbox.any [mscorlib]System.Int32
	IL_0011:  ret 
    } // end of method ListOperations::firstElement

  } // end of class ListOperations

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 6
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x21e4
	// Code size 159 (0x9f)
	.maxstack 27
	.locals init (
		object	V_0,
		class [mscorlib]System.Collections.Generic.List`1<object>	V_1,
		int32	V_2,
		int32	V_3,
		int32	V_4,
		class [mscorlib]System.Collections.Generic.List`1<object>	V_5,
		int32	V_6,
		int32	V_7,
		int32	V_8)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class ListOperations::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance class [mscorlib]System.Collections.Generic.List`1<object> class ListOperations::createList()
	IL_0012:  stloc.1 
	IL_0013:  ldloc.0 
	IL_0014:  ldloc.1 
	IL_0015:  callvirt instance int32 class ListOperations::sumList(class [mscorlib]System.Collections.Generic.List`1<object>)
	IL_001a:  stloc.2 
	IL_001b:  ldloc.2 
	IL_001c:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0021:  ldloc.0 
	IL_0022:  ldloc.1 
	IL_0023:  callvirt instance int32 class ListOperations::countElements(class [mscorlib]System.Collections.Generic.List`1<object>)
	IL_0028:  stloc.3 
	IL_0029:  ldloc.3 
	IL_002a:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_002f:  ldloc.0 
	IL_0030:  ldloc.1 
	IL_0031:  callvirt instance int32 class ListOperations::firstElement(class [mscorlib]System.Collections.Generic.List`1<object>)
	IL_0036:  stloc.s 4
	IL_0038:  ldloc.s 4
	IL_003a:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_003f:  newobj instance void class [mscorlib]System.Collections.Generic.List`1<object>::'.ctor'()
	IL_0044:  stloc.s 5
	IL_0046:  ldloc.s 5
	IL_0048:  dup 
	IL_0049:  ldc.i4.s 0x64
	IL_004b:  box [mscorlib]System.Int32
	IL_0050:  callvirt instance void class [mscorlib]System.Collections.Generic.List`1<object>::Add(!0)
	IL_0055:  stloc.s 5
	IL_0057:  ldloc.s 5
	IL_0059:  dup 
	IL_005a:  ldc.i4 200
	IL_005f:  box [mscorlib]System.Int32
	IL_0064:  callvirt instance void class [mscorlib]System.Collections.Generic.List`1<object>::Add(!0)
	IL_0069:  stloc.s 5
	IL_006b:  ldloc.0 
	IL_006c:  ldloc.s 5
	IL_006e:  callvirt instance int32 class ListOperations::sumList(class [mscorlib]System.Collections.Generic.List`1<object>)
	IL_0073:  stloc.s 6
	IL_0075:  ldloc.s 6
	IL_0077:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_007c:  ldloc.0 
	IL_007d:  ldloc.s 5
	IL_007f:  callvirt instance int32 class ListOperations::countElements(class [mscorlib]System.Collections.Generic.List`1<object>)
	IL_0084:  stloc.s 7
	IL_0086:  ldloc.s 7
	IL_0088:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_008d:  ldloc.0 
	IL_008e:  ldloc.s 5
	IL_0090:  callvirt instance int32 class ListOperations::firstElement(class [mscorlib]System.Collections.Generic.List`1<object>)
	IL_0095:  stloc.s 8
	IL_0097:  ldloc.s 8
	IL_0099:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_009e:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 7
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2290
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
        // Method begins at RVA 0x2298
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

