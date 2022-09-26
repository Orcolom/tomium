using UnityEngine;
using Wrench;
using Wrench.Builder;

namespace Binding
{
	public class UnityBinding : Module
	{
		public UnityBinding() : base("unity") { }
	}

	public class GameObjectBinding : Class
	{
		public GameObjectBinding() : base("GameObject", null, ForeignClass.DefaultManagedAlloc<GameObject>())
		{
			this.Method(Signature.Create(MethodType.Construct, "new"), (in Vm vm) =>
			{
				if (Expected.ManagedForeignType(vm, vm.Slot0, out ManagedForeignObject<GameObject> foreign)) return;
				foreign.Value = new GameObject();
			});

			this.Method(Signature.Create(MethodType.Construct, "new", 1), (in Vm vm) =>
			{
				vm.EnsureSlots(2);
				if (Expected.ManagedForeignType<GameObject>(vm, vm.Slot0, out var foreign)) return;
				if (Expected.String(vm, vm.Slot1, out var nameStr)) return;
				foreign.Value = new GameObject(nameStr);
			});

			this.Field(nameof(GameObject.name), false,
				get: (in Vm vm, ManagedForeignObject<GameObject> fo) => { vm.Slot0.SetString(fo.Value.name); },
				set: (in Vm vm, in Slot s1, ManagedForeignObject<GameObject> fo) =>
				{
					if (Expected.String(vm, s1, out var nameStr)) return;
					fo.Value.name = nameStr;
				}
			);

			this.Method(Signature.Create(MethodType.Method, nameof(GameObject.GetComponent), 1), (in Vm vm) =>
			{
				vm.EnsureSlots(2);
				if (Expected.ManagedForeignType<GameObject>(vm, vm.Slot0, out var foreign)) return;
				if (Expected.String(vm, vm.Slot1, out var nameStr)) return;
				foreign.Value = new GameObject(nameStr);
			});
		}
	}

	public class TransformBinding : Class
	{
		public TransformBinding() : base("Transform", null, ForeignClass.DefaultManagedAlloc<Transform>())
		{
			// this.
		}
	}
}
