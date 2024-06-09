using dnlib.DotNet;
using dnlib.DotNet.Emit;
using TestLib;


var mod = new ModuleDefUser("TestGenLib.dll");

var asm = new AssemblyDefUser("TestGenLib", new Version(1, 0, 0, 0));

asm.Modules.Add(mod);

var classDef = new TypeDefUser("TestGenLib", "TestGenLibClass", mod.CorLibTypes.Object.TypeDefOrRef);
classDef.Attributes = TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.Class | TypeAttributes.AnsiClass;

mod.Types.Add(classDef);

var stdMod = ModuleDefMD.Load(typeof(void).Module);
var stdImporter = new Importer(stdMod);

{
	var methodDef = new MethodDefUser("Test", MethodSig.CreateInstance(mod.CorLibTypes.Void));
	methodDef.Attributes = MethodAttributes.Public | MethodAttributes.HideBySig;
	methodDef.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;

	classDef.Methods.Add(methodDef);

	var consoleDef = stdImporter.Import(typeof(Console));
	var consoleWriteLineDef = stdImporter.Import(typeof(Console).GetMethod(nameof(Console.WriteLine), [ typeof(int) ]));

	var consoleRef = new TypeRefUser(mod, consoleDef.Namespace, consoleDef.Name, mod.CorLibTypes.AssemblyRef);
	var consoleWrite = new MemberRefUser(mod, consoleWriteLineDef.Name, consoleWriteLineDef.MethodSig, consoleRef);

	var libMod = ModuleDefMD.Load(typeof(TestLibClass).Module);
	var libImporter = new Importer(libMod);

	var libClassDef = libImporter.Import(typeof(TestLibClass));
	var libCtorDef = libImporter.Import(typeof(TestLibClass).GetConstructor([ typeof(int) ]));
	var libMethodDef = libImporter.Import(typeof(TestLibClass).GetMethod(nameof(TestLibClass.AddA)));

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

	var body = new CilBody();
	methodDef.Body = body;
	body.Instructions.Add(OpCodes.Ldc_I4_3.ToInstruction());
	body.Instructions.Add(OpCodes.Newobj.ToInstruction(libCtorRef));
	body.Instructions.Add(OpCodes.Ldc_I4_4.ToInstruction());
	body.Instructions.Add(OpCodes.Callvirt.ToInstruction(libMethodRef));
	body.Instructions.Add(OpCodes.Call.ToInstruction(consoleWrite));
	body.Instructions.Add(OpCodes.Ret.ToInstruction());
}

{
	var dictDef = stdImporter.Import(typeof(Dictionary<int, string>));
	var dictRef = new TypeRefUser(mod, dictDef.Namespace, dictDef.Name, mod.CorLibTypes.AssemblyRef);

	var dictGenSig = new GenericInstSig(new ClassSig(dictRef), [ mod.CorLibTypes.Int32, mod.CorLibTypes.String ]);
	var dictSpecRef = new TypeSpecUser(dictGenSig);

	var dictCtorDef = stdImporter.Import(typeof(Dictionary<int, string>).GetConstructor([]));
	var dictCtorRef = new MemberRefUser(mod, dictCtorDef.Name, dictCtorDef.MethodSig, dictSpecRef);

	var fieldDef = new FieldDefUser("dict", new FieldSig(dictSpecRef.ToTypeSig()));
	fieldDef.Attributes = FieldAttributes.Static | FieldAttributes.Private;

	classDef.Fields.Add(fieldDef);

	var ctorDef = new MethodDefUser(".ctor", MethodSig.CreateStatic(mod.CorLibTypes.Void));
	ctorDef.Attributes = MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig;
	ctorDef.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;

	classDef.Methods.Add(ctorDef);

	/*
	0    0000    newobj instance void class [System.Collections] System.Collections.Generic.Dictionary`2 < int32, string>::.ctor()
	1    0005    stsfld	class [System.Collections] System.Collections.Generic.Dictionary`2 < int32, string> TestGenLib.TestGenLibClass::dict
	2    000A    ret
	*/

	var body = new CilBody();
	ctorDef.Body = body;
	body.Instructions.Add(OpCodes.Newobj.ToInstruction(dictCtorRef));
	body.Instructions.Add(OpCodes.Stsfld.ToInstruction(fieldDef));
	body.Instructions.Add(OpCodes.Ret.ToInstruction());
}

mod.Write(@"NewTestGenLib.dll");
