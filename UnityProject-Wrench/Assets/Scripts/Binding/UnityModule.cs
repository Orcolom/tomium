using System;
using UnityEngine;
using Wrench;
using Wrench.Builder;

namespace Binding
{
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
		
		// [WrenchMethod(MethodType.Construct)]
		// private static GameObject Create(Vm vm, GameObject self)
		// {
		// 	return new GameObject();
		// }
		//
		// [WrenchMethod(MethodType.Construct)]
		// private static GameObject Create(Vm vm, GameObject self, string name)
		// {
		// 	return new GameObject(name);
		// }
		//
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
