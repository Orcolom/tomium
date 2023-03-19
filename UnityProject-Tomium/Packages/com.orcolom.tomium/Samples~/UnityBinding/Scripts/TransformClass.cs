using UnityEngine;
using Tomium.Builder;

namespace Tomium.Samples.UnityBinding
{
	public class TransformClass : UnityModule.Class<Transform>
	{
		public TransformClass() : base(nameof(Transform), UnityComponentClass.WrenName)
		{
			Add(new Method(Signature.Create(MethodType.Method, nameof(GetPosition)), new ForeignMethod(GetPosition)));
			Add(new Method(Signature.Create(MethodType.Method, nameof(SetPosition), 1), new ForeignMethod(SetPosition)));
			
			Add(new Method(Signature.Create(MethodType.Method, nameof(GetRotation)), new ForeignMethod(GetRotation)));
			Add(new Method(Signature.Create(MethodType.Method, nameof(SetRotation), 1), new ForeignMethod(SetRotation)));
			
			Add(new Method(Signature.Create(MethodType.Method, nameof(GetPositionAndRotation)), new ForeignMethod(GetPositionAndRotation)));
			Add(new Method(Signature.Create(MethodType.Method, nameof(SetPositionAndRotation), 2), new ForeignMethod(SetPositionAndRotation)));
		}
		
		private void GetPosition(Vm vm)
		{
			vm.EnsureSlots(1);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Transform> self) == false) return;
			if (UnityModule.TryGetId(vm, typeof(Vector3), out var type) == false) return;
			
			var position = self.Value.position;
			UnityModule.SetNewForeignObject(vm, vm.Slot0, type, position);
		}

		private void SetPosition(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Transform> self) == false) return;
			if (Vector3Class.Expect(vm.Slot1, out var position) == false) return;
			
			self.Value.position = position.Value;
		}

		private void GetRotation(Vm vm)
		{
			vm.EnsureSlots(1);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Transform> self) == false) return;
			if (UnityModule.TryGetId(vm, typeof(Quaternion), out var type) == false) return;
			
			var rotation = self.Value.rotation;
			UnityModule.SetNewForeignObject(vm, vm.Slot0, type, rotation);
		}

		private void SetRotation(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Transform> self) == false) return;
			if (QuaternionClass.Expect(vm.Slot1, out var rotation) == false) return;
			
			self.Value.rotation = rotation.Value;
		}
		
		private void SetPositionAndRotation(Vm vm)
		{
			vm.EnsureSlots(3);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Transform> self) == false) return;
			if (Vector3Class.Expect(vm.Slot1, out var position) == false) return;
			if (QuaternionClass.Expect(vm.Slot1, out var rotation) == false) return;

			self.Value.SetPositionAndRotation(position.Value, rotation.Value);
		}
		
		private void GetPositionAndRotation(Vm vm)
		{
			vm.EnsureSlots(3);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Transform> self) == false) return;
			if (UnityModule.TryGetId(vm, typeof(Vector3), out var vectorType) == false) return;
			if (UnityModule.TryGetId(vm, typeof(Quaternion), out var quadType) == false) return;

			var rotation = self.Value.rotation;
			UnityModule.SetNewForeignObject(vm, vm.Slot1, quadType, rotation);
			
			var position = self.Value.position;
			UnityModule.SetNewForeignObject(vm, vm.Slot2, vectorType, position);
			
			vm.Slot0.SetNewList();
			vm.Slot0.AddToList(vm.Slot1);
			vm.Slot0.AddToList(vm.Slot2);
		}
	}
}
