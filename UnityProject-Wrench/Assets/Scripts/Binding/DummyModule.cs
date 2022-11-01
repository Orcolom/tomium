using UnityEngine;
using Wrench;
using Wrench.Builder;

namespace Binding
{
	/// <summary>
	/// dummy classes help with visualizing the IL code we want to generate 
	/// </summary>
	public class DummyModule : Module
	{
		public DummyModule() : base("~/dummy")
		{
			Add(new DummyClass());
		}
	}

	public class DummyClass : Class
	{
		public DummyClass() : base("Dummy")
		{
			Add(new Method(Signature.Create(MethodType.Construct, "new", 16), new ForeignMethod(Create_Example)));
			Add(new Method(Signature.Create(MethodType.StaticMethod, "injected"), Inject_Create()));
		}

		private void Create(Vm vm, Slot a0, Slot a1, string a2, Slot a3, byte[] a4, Slot a5, int a6, Slot a7,
			ForeignObject<GameObject> a8, Slot a9,
			Slot a10, Slot a11, Slot a12, Slot a13, Slot a14, Slot a15, Slot a16)
		{
			var foreign = a0.GetForeign<GameObject>();
			foreign.Value = new GameObject();
		}

		private static MethodBody Inject_Create()
		{
			return new MethodBody
			{
				Token.DangerousInsert(@"
return Dummy.new(0,0,0,0,0,0,0,0,0,0,0,0,0,0,)
"),
			};
		}

		
		private static void Wrench__New__1531160386(Vm vm)
		{
			vm.EnsureSlots(2);
			Slot slot0 = vm.Slot0;
			Slot slot1 = vm.Slot1;
			New(vm, slot0, slot1);
		}

		[WrenchMethod(MethodType.Construct)]
		private static void New(Vm vm, Slot self, Slot name)
		{
			var v = self.GetForeign<GameObject>();
			v.Value = new GameObject(name.GetString());
		}


		private void Create_Example(Vm vm)
		{
			vm.EnsureSlots(16);

			var arg0 = vm.Slot0;
			var arg1 = vm.Slot1;

			if (ExpectValue.ExpectString(vm, vm.Slot2, out string arg2) == false) return;

			var arg3 = vm.Slot2;

			if (ExpectValue.ExpectByteArray(vm, vm.Slot4, out byte[] arg4) == false) return;

			var arg5 = vm.Slot5;

			if (ExpectValue.ExpectInt(vm, vm.Slot3, out int arg6) == false) return;
			
			var arg7 = vm.Slot7;

			if (UnityModule.ExpectObject<GameObject>(vm, vm.Slot3, out var arg8) == false) return;
			
			var arg9 = vm.Slot9;
			var arg10 = vm.Slot10;
			var arg11 = vm.Slot11;
			var arg12 = vm.Slot12;
			var arg13 = vm.Slot13;
			var arg14 = vm.Slot14;
			var arg15 = vm.Slot15;
			var arg16 = vm.Slot16;

			Create(vm, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15,
				arg16);
		}
	}
}
