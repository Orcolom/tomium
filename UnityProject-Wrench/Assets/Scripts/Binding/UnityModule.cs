using System;
using UnityEngine;
using Wrench;
using Wrench.Builder;
using Object = UnityEngine.Object;
using ValueType = Wrench.ValueType;

namespace Binding
{
	public class ExampleModule : Module
	{
		public ExampleModule() : base("/~/path")
		{
			Add(new ExampleClass());
		}
	}

	public class ExampleClass : Class
	{
		public ExampleClass() : base("Example")
		{
			Add(new Method(Signature.Create(MethodType.Construct, "new", 16), new ForeignMethod(Create_Example)));
		}

		// [WrenchMethod(MethodType.Construct)]
		private void Create(Vm vm, Slot a0, Slot a1, string a2, Slot a3, byte[] a4, Slot a5, int a6, Slot a7,
			ForeignObject<GameObject> a8, Slot a9,
			Slot a10, Slot a11, Slot a12, Slot a13, Slot a14, Slot a15, Slot a16)
		{
			var foreign = a0.GetForeign<GameObject>();
			foreign.Value = new GameObject();
		}

		private void Create_Example(in Vm vm)
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

			var arg8 = vm.Slot8.GetForeign<GameObject>();

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

	[WrenchModule("Unity")]
	public class UnityModule : Module
	{
		[WrenchExpect(typeof(ForeignObject<Object>), true)]
		public static bool ExpectObject<T>(in Vm vm, in Slot slot, out ForeignObject<T> value) where T : Object
		{
			value = new ForeignObject<T>();
			if (ExpectValue.IsOfValueType(vm, slot, ValueType.Foreign, true) == false) return false;
			value = slot.GetForeign<T>();
			return true;
		}
	}

	[WrenchClass(typeof(UnityModule), nameof(GameObject))]
	public class GameObjectBinding : Class
	{
		[WrenchMethod(MethodType.Construct)]
		private void Create(Vm vm, Slot self)
		{
			var foreign = self.GetForeign<GameObject>();
			foreign.Value = new GameObject();
		}

		[WrenchMethod(MethodType.Construct)]
		private void Create(Vm vm, Slot a0, Slot a1, Slot a2, Slot a3, Slot a4, Slot a5, Slot a6, Slot a7, Slot a8, Slot a9,
			Slot a10, Slot a11, Slot a12, Slot a13, Slot a14, Slot a15, Slot a16)
		{
			var foreign = a0.GetForeign<GameObject>();
			foreign.Value = new GameObject();
		}

		[WrenchMethod(MethodType.Construct)]
		private static void Create(in Vm vm, ForeignObject<GameObject> self, string name)
		{
			self.Value = new GameObject(name);
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private string Name(Vm vm, ForeignObject<GameObject> self)
		{
			return self.Value.name;
		}

		[WrenchMethod(MethodType.FieldSetter)]
		private void Name(Vm vm, ForeignObject<GameObject> self, string name)
		{
			self.Value.name = name;
		}
	}
}
