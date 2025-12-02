.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'test_multiple_classes'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module test_multiple_classes.exe // GUID = {EA92B630-493A-4F8E-95FA-FBDD114B9EBF}


  .class public auto ansi Configuration
  	extends [mscorlib]System.Object
  {
    .field  public  int32 multiplier
    .field  public  int32 offset

    // method line 1
    .method public specialname rtspecialname 
           instance default void '.ctor' (int32 A_1, int32 A_2)  cil managed 
    {
        // Method begins at RVA 0x20ec
	// Code size 45 (0x2d)
	.maxstack 4
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.i4.1 
	IL_0008:  stfld int32 Configuration::multiplier
	IL_000d:  ldarg.0 
	IL_000e:  ldc.i4.0 
	IL_000f:  stfld int32 Configuration::offset
	IL_0014:  ldarg.0 
	IL_0015:  ldarg 1

	IL_0019:  nop 
	IL_001a:  nop 
	IL_001b:  stfld int32 Configuration::multiplier
	IL_0020:  ldarg.0 
	IL_0021:  ldarg 2

	IL_0025:  nop 
	IL_0026:  nop 
	IL_0027:  stfld int32 Configuration::offset
	IL_002c:  ret 
    } // end of method Configuration::.ctor

    // method line 2
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2128
	// Code size 21 (0x15)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldc.i4.1 
	IL_0008:  stfld int32 Configuration::multiplier
	IL_000d:  ldarg.0 
	IL_000e:  ldc.i4.0 
	IL_000f:  stfld int32 Configuration::offset
	IL_0014:  ret 
    } // end of method Configuration::.ctor

    // method line 3
    .method public virtual hidebysig newslot 
           instance default int32 getMultiplier ()  cil managed 
    {
        // Method begins at RVA 0x2140
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld int32 Configuration::multiplier
	IL_0006:  ret 
    } // end of method Configuration::getMultiplier

    // method line 4
    .method public virtual hidebysig newslot 
           instance default int32 getOffset ()  cil managed 
    {
        // Method begins at RVA 0x2148
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld int32 Configuration::offset
	IL_0006:  ret 
    } // end of method Configuration::getOffset

  } // end of class Configuration

  .class public auto ansi Calculator
  	extends [mscorlib]System.Object
  {
    .field  public  object config

    // method line 5
    .method public specialname rtspecialname 
           instance default void '.ctor' (class Configuration A_1)  cil managed 
    {
        // Method begins at RVA 0x2150
	// Code size 26 (0x1a)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldnull 
	IL_0008:  stfld object Calculator::config
	IL_000d:  ldarg.0 
	IL_000e:  ldarg 1

	IL_0012:  nop 
	IL_0013:  nop 
	IL_0014:  stfld object Calculator::config
	IL_0019:  ret 
    } // end of method Calculator::.ctor

    // method line 6
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x216c
	// Code size 14 (0xe)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ldarg.0 
	IL_0007:  ldnull 
	IL_0008:  stfld object Calculator::config
	IL_000d:  ret 
    } // end of method Calculator::.ctor

    // method line 7
    .method public virtual hidebysig newslot 
           instance default int32 calculate (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x217c
	// Code size 49 (0x31)
	.maxstack 7
	.locals init (
		int32	V_0,
		int32	V_1,
		int32	V_2)
	IL_0000:  ldarg.0 
	IL_0001:  ldfld object Calculator::config
	IL_0006:  castclass Configuration
	IL_000b:  callvirt instance int32 class Configuration::getMultiplier()
	IL_0010:  stloc.0 
	IL_0011:  ldarg.0 
	IL_0012:  ldfld object Calculator::config
	IL_0017:  castclass Configuration
	IL_001c:  callvirt instance int32 class Configuration::getOffset()
	IL_0021:  stloc.1 
	IL_0022:  ldarg 1

	IL_0026:  nop 
	IL_0027:  nop 
	IL_0028:  ldloc.0 
	IL_0029:  mul 
	IL_002a:  stloc.2 
	IL_002b:  ldloc.2 
	IL_002c:  ldloc.1 
	IL_002d:  add 
	IL_002e:  stloc.2 
	IL_002f:  ldloc.2 
	IL_0030:  ret 
    } // end of method Calculator::calculate

    // method line 8
    .method public virtual hidebysig newslot 
           instance default int32 calculateDouble (int32 A_1)  cil managed 
    {
        // Method begins at RVA 0x21bc
	// Code size 21 (0x15)
	.maxstack 5
	.locals init (
		int32	V_0)
	IL_0000:  ldarg.0 
	IL_0001:  ldarg 1

	IL_0005:  nop 
	IL_0006:  nop 
	IL_0007:  callvirt instance int32 class Calculator::calculate(int32)
	IL_000c:  stloc.0 
	IL_000d:  ldarg.0 
	IL_000e:  ldloc.0 
	IL_000f:  callvirt instance int32 class Calculator::calculate(int32)
	IL_0014:  ret 
    } // end of method Calculator::calculateDouble

  } // end of class Calculator

  .class public auto ansi Orchestrator
  	extends [mscorlib]System.Object
  {

    // method line 9
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x21e0
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method Orchestrator::.ctor

    // method line 10
    .method public virtual hidebysig newslot 
           instance default int32 run ()  cil managed 
    {
        // Method begins at RVA 0x21e8
	// Code size 66 (0x42)
	.maxstack 10
	.locals init (
		object	V_0,
		object	V_1,
		int32	V_2,
		int32	V_3,
		int32	V_4)
	IL_0000:  ldc.i4.s 0x03
	IL_0002:  ldc.i4.s 0x0a
	IL_0004:  newobj instance void class Configuration::'.ctor'(int32, int32)
	IL_0009:  stloc.0 
	IL_000a:  ldloc.0 
	IL_000b:  newobj instance void class Calculator::'.ctor'(class Configuration)
	IL_0010:  stloc.1 
	IL_0011:  ldloc.1 
	IL_0012:  ldc.i4.s 0x05
	IL_0014:  callvirt instance int32 class Calculator::calculate(int32)
	IL_0019:  stloc.2 
	IL_001a:  ldloc.2 
	IL_001b:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0020:  ldloc.1 
	IL_0021:  ldc.i4.s 0x07
	IL_0023:  callvirt instance int32 class Calculator::calculate(int32)
	IL_0028:  stloc.3 
	IL_0029:  ldloc.3 
	IL_002a:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_002f:  ldloc.1 
	IL_0030:  ldc.i4.s 0x02
	IL_0032:  callvirt instance int32 class Calculator::calculateDouble(int32)
	IL_0037:  stloc.s 4
	IL_0039:  ldloc.s 4
	IL_003b:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0040:  ldc.i4.0 
	IL_0041:  ret 
    } // end of method Orchestrator::run

  } // end of class Orchestrator

  .class public auto ansi TestRunner
  	extends [mscorlib]System.Object
  {

    // method line 11
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2238
	// Code size 26 (0x1a)
	.maxstack 6
	.locals init (
		object	V_0,
		int32	V_1)
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  newobj instance void class Orchestrator::'.ctor'()
	IL_000b:  stloc.0 
	IL_000c:  ldloc.0 
	IL_000d:  callvirt instance int32 class Orchestrator::run()
	IL_0012:  stloc.1 
	IL_0013:  ldloc.1 
	IL_0014:  call void class [mscorlib]System.Console::WriteLine(int32)
	IL_0019:  ret 
    } // end of method TestRunner::.ctor

  } // end of class TestRunner

  .class public auto ansi sealed '<EntryPoint>'
  	extends [mscorlib]System.Object
  {

    // method line 12
    .method public specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2260
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  ret 
    } // end of method <EntryPoint>::.ctor

    // method line 13
    .method public static 
           default void Main (string[] A_0)  cil managed 
    {
        // Method begins at RVA 0x2268
	.entrypoint
	// Code size 7 (0x7)
	.maxstack 8
	IL_0000:  newobj instance void class TestRunner::'.ctor'()
	IL_0005:  pop 
	IL_0006:  ret 
    } // end of method <EntryPoint>::Main

  } // end of class <EntryPoint>

