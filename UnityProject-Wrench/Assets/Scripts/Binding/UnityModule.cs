using System;
using UnityEngine;
using Wrench;
using Wrench.Builder;
using Object = UnityEngine.Object;
using ValueType = Wrench.ValueType;

namespace Binding
{
	// public class GetUnityObjectValueUtils<T>
	// 	where T : Object,
	// 	IGetValueFromSlot<T>
	// {
	// 	public bool GetValue(in Vm vm, in ISlotManaged slot, out T value)
	// 	{
	// 		value = null;
	// 		if (GetValueUtils.IsOfValueType(vm, slot, ValueType.Foreign) == false) return false;
	//
	// 		var foreign = slot.GetForeign<T>();
	// 		value = foreign.Value;
	// 		return true;
	// 	}
	// }

	public class Example2Module : Module
	{
		public Example2Module() : base("/~~/path")
		{
			Add(new Import("/~/path", new ImportVariable(nameof(ExampleClass))));
		}
	}

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
		private void Create(Vm vm, Slot a0, Slot a1, Slot a2, Slot a3, Slot a4, Slot a5, Slot a6, Slot a7, Slot a8, Slot a9,
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
			var arg2 = vm.Slot2;
			var arg3 = vm.Slot3;
			var arg4 = vm.Slot4;
			var arg5 = vm.Slot5;
			var arg6 = vm.Slot6;
			var arg7 = vm.Slot7;
			var arg8 = vm.Slot8;
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
	public class UnityModule : Module { }

	[WrenchClass(typeof(UnityModule), nameof(GameObject))]
	public class GameObjectBinding : Class
	{
		// public GameObjectBinding() : base(null, "unity__", null) {}

		// 	// public GameObjectBinding() : base("GameObject", null, ForeignClass.DefaultAlloc<GameObject>())
		// 	// {
		// 		// this.Method(Signature.Create(MethodType.Construct, "new"), (in Vm vm) =>
		// 		// {
		// 		// 	if (Expected.ForeignType(vm, vm.Slot0, out ForeignObject<GameObject> foreign)) return;
		// 		// 	foreign.Value = new GameObject();
		// 		// });
		// 		//
		// 		// this.Method(Signature.Create(MethodType.Construct, "new", 1), (in Vm vm) =>
		// 		// {
		// 		// 	vm.EnsureSlots(2);
		// 		// 	if (Expected.ForeignType<GameObject>(vm, vm.Slot0, out var foreign)) return;
		// 		// 	if (Expected.String(vm, vm.Slot1, out var nameStr)) return;
		// 		// 	foreign.Value = new GameObject(nameStr);
		// 		// });
		// 		//
		// 		// this.Field(nameof(GameObject.name), false,
		// 		// 	get: (in Vm vm, ForeignObject<GameObject> fo) => { vm.Slot0.SetString(fo.Value.name); },
		// 		// 	set: (in Vm vm, in Slot s1, ForeignObject<GameObject> fo) =>
		// 		// 	{
		// 		// 		if (Expected.String(vm, s1, out var nameStr)) return;
		// 		// 		fo.Value.name = nameStr;
		// 		// 	}
		// 		// );
		// 		//
		// 		// this.Method(Signature.Create(MethodType.Method, nameof(GameObject.GetComponent), 1), (in Vm vm) =>
		// 		// {
		// 		// 	vm.EnsureSlots(2);
		// 		// 	if (Expected.ForeignType<GameObject>(vm, vm.Slot0, out var foreign)) return;
		// 		// 	if (Expected.String(vm, vm.Slot1, out var nameStr)) return;
		// 		// 	foreign.Value = new GameObject(nameStr);
		// 		// });
		// 	// }
		//

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
		private static void Create(Vm vm, Slot self, Slot name)
		{
			var foreign = self.GetForeign<GameObject>();
			foreign.Value = new GameObject(name.GetString());
		}

		// [WrenchMethod(MethodType.FieldGetter)]
		// private string Name(Vm vm, GameObject self)
		// {
		// 	return self.name;
		// }
		// 	
		// [WrenchMethod(MethodType.FieldSetter)]
		// private void Name(Vm vm, GameObject self, string name)
		// {
		// 	self.name = name;
		// }
	}
}
