using UnityEngine;
using Wrench;
using Wrench.Builder;

namespace Binding.UnityBinding
{
	[WrenchClass(typeof(UnityModule), nameof(Transform), typeof(Transform), UnityComponentBinding.WrenName)]
	public class TransformBinding : Class
	{
		[WrenchMethod(MethodType.FieldGetter)]
		private void position(Vm vm, ForeignObject<Transform> self)
		{
			if (UnityModule.ExpectId(vm, typeof(Vector3), out var type) == false) return;
			var position = self.Value.position;
			UnityModule.SetNewForeign(vm, vm.Slot0, type, position);
		}

		[WrenchMethod(MethodType.FieldSetter)]
		private void position(Vm vm, ForeignObject<Transform> self, ForeignObject<Vector3> position)
		{
			self.Value.position = position.Value;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void rotation(Vm vm, ForeignObject<Transform> self)
		{
			if (UnityModule.ExpectId(vm, typeof(Quaternion), out var type) == false) return;
			var rotation = self.Value.rotation;
			UnityModule.SetNewForeign(vm, vm.Slot0, type, rotation);
		}

		[WrenchMethod(MethodType.FieldSetter)]
		private void rotation(Vm vm, ForeignObject<Transform> self, ForeignObject<Quaternion> rotation)
		{
			self.Value.rotation = rotation.Value;
		}
		
		[WrenchMethod(MethodType.Method)]
		private void SetPositionAndRotation(Vm vm, ForeignObject<Transform> self, ForeignObject<Vector3> position, ForeignObject<Quaternion> rotation)
		{
			self.Value.SetPositionAndRotation(position.Value, rotation.Value);
		}
		
		[WrenchMethod(MethodType.Method)]
		private void GetPositionAndRotation(Vm vm, ForeignObject<Transform> self)
		{
			vm.EnsureSlots(3);
			
			if (UnityModule.ExpectId(vm, typeof(Vector3), out var vectorType) == false) return;
			if (UnityModule.ExpectId(vm, typeof(Quaternion), out var quadType) == false) return;

			var rotation = self.Value.rotation;
			UnityModule.SetNewForeign(vm, vm.Slot1, quadType, rotation);
			
			var position = self.Value.position;
			UnityModule.SetNewForeign(vm, vm.Slot2, vectorType, position);
			
			vm.Slot0.SetNewList();
			vm.Slot0.AddToList(vm.Slot1);
			vm.Slot0.AddToList(vm.Slot2);
		}
	}
}
