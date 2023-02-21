using UnityEngine;
using Tomia;
using Tomia.Builder;

namespace Binding.UnityBinding
{
	public class TransformBinding : UnityModule.Class<Transform>
	{
		public TransformBinding() : base(nameof(Transform), UnityComponentBinding.WrenName)
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
			UnityModule.SetNewForeign(vm, vm.Slot0, type, position);
		}

		private void SetPosition(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Transform> self) == false) return;
			if (Vector3Binding.Expect(vm.Slot1, out var position) == false) return;
			
			self.Value.position = position.Value;
		}

		private void GetRotation(Vm vm)
		{
			vm.EnsureSlots(1);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Transform> self) == false) return;
			if (UnityModule.TryGetId(vm, typeof(Quaternion), out var type) == false) return;
			
			var rotation = self.Value.rotation;
			UnityModule.SetNewForeign(vm, vm.Slot0, type, rotation);
		}

		private void SetRotation(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Transform> self) == false) return;
			if (QuaternionBinding.Expect(vm.Slot1, out var rotation) == false) return;
			
			self.Value.rotation = rotation.Value;
		}
		
		private void SetPositionAndRotation(Vm vm)
		{
			vm.EnsureSlots(3);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Transform> self) == false) return;
			if (Vector3Binding.Expect(vm.Slot1, out var position) == false) return;
			if (QuaternionBinding.Expect(vm.Slot1, out var rotation) == false) return;

			self.Value.SetPositionAndRotation(position.Value, rotation.Value);
		}
		
		private void GetPositionAndRotation(Vm vm)
		{
			vm.EnsureSlots(3);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Transform> self) == false) return;
			if (UnityModule.TryGetId(vm, typeof(Vector3), out var vectorType) == false) return;
			if (UnityModule.TryGetId(vm, typeof(Quaternion), out var quadType) == false) return;

			var rotation = self.Value.rotation;
			UnityModule.SetNewForeign(vm, vm.Slot1, quadType, rotation);
			
			var position = self.Value.position;
			UnityModule.SetNewForeign(vm, vm.Slot2, vectorType, position);
			
			vm.Slot0.SetNewList();
			vm.Slot0.AddToList(vm.Slot1);
			vm.Slot0.AddToList(vm.Slot2);
		}
	}

// 	[TomiaClass(typeof(UnityModule), nameof(MeshFilter), typeof(MeshFilter), UnityComponentBinding.WrenName)]
// 	public class MeshFilterBinding : Class
// 	{
// 		[TomiaMethod(MethodType.FieldGetter)]
// 		private static void mesh(Vm vm, ForeignObject<MeshFilter> self)
// 		{
// 			if (UnityModule.ExpectId(vm, typeof(Mesh), out var type) == false) return;
// 			UnityModule.SetNewForeign(vm, vm.Slot0, type, self.Value.mesh);
// 		}
// 		
// 		[TomiaMethod(MethodType.FieldSetter)]
// 		private static void mesh(Vm vm, ForeignObject<MeshFilter> self, ForeignObject<Mesh> mesh)
// 		{
// 			self.Value.mesh = mesh.Value;
// 		}
// 	}
// 	
// 	[TomiaClass(typeof(UnityModule), nameof(MeshRenderer), typeof(MeshRenderer), UnityComponentBinding.WrenName)]
// 	public class MeshRendererBinding : Class
// 	{
// 		[TomiaMethod(MethodType.FieldGetter)]
// 		private static void materials(Vm vm, ForeignObject<MeshRenderer> self)
// 		{
// 			vm.EnsureSlots(2);
// 			if (UnityModule.ExpectId(vm, typeof(Mesh), out var type) == false) return;
//
// 			var list = self.Value.materials;
// 			vm.Slot0.SetNewList();
// 			for (int i = 0; i < list.Length; i++)
// 			{
// 				UnityModule.SetNewForeign(vm, vm.Slot1, type, list[i]);
// 				vm.Slot0.AddToList(vm.Slot1);
// 			}
// 		}
// 		
// 		[TomiaMethod(MethodType.FieldSetter)]
// 		private static void materials(Vm vm, ForeignObject<MeshRenderer> self, Slot materialList)
// 		{
// 			vm.EnsureSlots(3);
// 			if (ExpectValue.IsOfValueType(vm, materialList, ValueType.List, true) == false) return;
// 			
// 			int count = materialList.GetCount();
// 			Material[] materials = new Material[count];
// 			
// 			for (int i = 0; i < count; i++)
// 			{
// 				materialList.GetListElement(i, vm.Slot2);
// 				if (MaterialBinding.Expect(vm, vm.Slot2, out var material) == false) return;
// 				materials[i] = material.Value;
// 			}
// 		}
// 	}
}
