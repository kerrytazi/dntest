using dnlib.DotNet;
using dnlib.DotNet.Emit;


var mod = new ModuleDefUser("TestGenLib.dll");

var asm = new AssemblyDefUser("TestGenLib", new Version(1, 0, 0, 0));

asm.Modules.Add(mod);

var classDef = new TypeDefUser("TestGenLib", "TestGenLibClass", mod.CorLibTypes.Object.TypeDefOrRef);
classDef.Attributes = TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.Class | TypeAttributes.AnsiClass;

mod.Types.Add(classDef);

var methodDef = new MethodDefUser("Test", MethodSig.CreateInstance(mod.CorLibTypes.Void));
methodDef.Attributes = MethodAttributes.Public | MethodAttributes.HideBySig;
methodDef.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;

classDef.Methods.Add(methodDef);

var consoleRef = new TypeRefUser(mod, "System", "Console", mod.CorLibTypes.AssemblyRef);
var consoleWrite = new MemberRefUser(mod, "WriteLine", MethodSig.CreateStatic(mod.CorLibTypes.Void, mod.CorLibTypes.Int32), consoleRef);

ModuleContext libModCtx = ModuleDef.CreateModuleContext();
ModuleDefMD libMod = ModuleDefMD.Load(@"TestLib.dll", libModCtx);

var libClassDef = libMod.Types.FirstOrDefault(t => t.Name == "TestLibClass")!;
var libCtorDef = libClassDef.Methods.FirstOrDefault(m => m.Name == ".ctor")!;
var libMethodDef = libClassDef.Methods.FirstOrDefault(m => m.Name == "AddA")!;

var libClassRef = new TypeRefUser(mod, libClassDef.Namespace, libClassDef.Name, libMod.Assembly.ToAssemblyRef());
var libCtorRef = new MemberRefUser(mod, libCtorDef.Name, libCtorDef.MethodSig, libClassRef);
var libMethodRef = new MemberRefUser(mod, libMethodDef.Name, libMethodDef.MethodSig, libClassRef);

/*
0    0000    ldc.i4.3
1    0001    newobj     instance void [TestLib]TestLib.TestLibClass::.ctor(int32)
2    0006    ldc.i4.4
3    0007    callvirt   instance int32 [TestLib]TestLib.TestLibClass::AddA(int32)
4    000C    call       void [System.Console]System.Console::WriteLine(int32)
5    0011    ret
*/

var methodBody = new CilBody();
methodDef.Body = methodBody;
methodBody.Instructions.Add(OpCodes.Ldc_I4_3.ToInstruction());
methodBody.Instructions.Add(OpCodes.Newobj.ToInstruction(libCtorRef));
methodBody.Instructions.Add(OpCodes.Ldc_I4_4.ToInstruction());
methodBody.Instructions.Add(OpCodes.Callvirt.ToInstruction(libMethodRef));
methodBody.Instructions.Add(OpCodes.Call.ToInstruction(consoleWrite));
methodBody.Instructions.Add(OpCodes.Ret.ToInstruction());

mod.Write(@"NewTestGenLib.dll");
