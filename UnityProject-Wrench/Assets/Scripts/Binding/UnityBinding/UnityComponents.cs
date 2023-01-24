// using UnityEngine;
// using Wrench;
// using Wrench.Builder;
//
// namespace Binding.UnityBinding
// {
// 	[WrenchClass(typeof(UnityModule), nameof(Transform), typeof(Transform), UnityComponentBinding.WrenName)]
// 	public class TransformBinding : Class
// 	{
// 		[WrenchMethod(MethodType.Method)]
// 		private void GetPosition(Vm vm, ForeignObject<Transform> self)
// 		{
// 			if (UnityModule.ExpectId(vm, typeof(Vector3), out var type) == false) return;
// 			var position = self.Value.position;
// 			UnityModule.SetNewForeign(vm, vm.Slot0, type, position);
// 		}
//
// 		[WrenchMethod(MethodType.Method)]
// 		private void SetPosition(Vm vm, ForeignObject<Transform> self, ForeignObject<Vector3> position)
// 		{
// 			self.Value.position = position.Value;
// 		}
//
// 		[WrenchMethod(MethodType.Method)]
// 		private void GetRotation(Vm vm, ForeignObject<Transform> self)
// 		{
// 			if (UnityModule.ExpectId(vm, typeof(Quaternion), out var type) == false) return;
// 			var rotation = self.Value.rotation;
// 			UnityModule.SetNewForeign(vm, vm.Slot0, type, rotation);
// 		}
//
// 		[WrenchMethod(MethodType.Method)]
// 		private void SetRotation(Vm vm, ForeignObject<Transform> self, ForeignObject<Quaternion> rotation)
// 		{
// 			self.Value.rotation = rotation.Value;
// 		}
// 		
// 		[WrenchMethod(MethodType.Method)]
// 		private void SetPositionAndRotation(Vm vm, ForeignObject<Transform> self, ForeignObject<Vector3> position, ForeignObject<Quaternion> rotation)
// 		{
// 			self.Value.SetPositionAndRotation(position.Value, rotation.Value);
// 		}
// 		
// 		[WrenchMethod(MethodType.Method)]
// 		private void GetPositionAndRotation(Vm vm, ForeignObject<Transform> self)
// 		{
// 			vm.EnsureSlots(3);
// 			
// 			if (UnityModule.ExpectId(vm, typeof(Vector3), out var vectorType) == false) return;
// 			if (UnityModule.ExpectId(vm, typeof(Quaternion), out var quadType) == false) return;
//
// 			var rotation = self.Value.rotation;
// 			UnityModule.SetNewForeign(vm, vm.Slot1, quadType, rotation);
// 			
// 			var position = self.Value.position;
// 			UnityModule.SetNewForeign(vm, vm.Slot2, vectorType, position);
// 			
// 			vm.Slot0.SetNewList();
// 			vm.Slot0.AddToList(vm.Slot1);
// 			vm.Slot0.AddToList(vm.Slot2);
// 		}
// 	}
//
// 	[WrenchClass(typeof(UnityModule), nameof(MeshFilter), typeof(MeshFilter), UnityComponentBinding.WrenName)]
// 	public class MeshFilterBinding : Class
// 	{
// 		[WrenchMethod(MethodType.FieldGetter)]
// 		private static void mesh(Vm vm, ForeignObject<MeshFilter> self)
// 		{
// 			if (UnityModule.ExpectId(vm, typeof(Mesh), out var type) == false) return;
// 			UnityModule.SetNewForeign(vm, vm.Slot0, type, self.Value.mesh);
// 		}
// 		
// 		[WrenchMethod(MethodType.FieldSetter)]
// 		private static void mesh(Vm vm, ForeignObject<MeshFilter> self, ForeignObject<Mesh> mesh)
// 		{
// 			self.Value.mesh = mesh.Value;
// 		}
// 	}
// 	
// 	[WrenchClass(typeof(UnityModule), nameof(MeshRenderer), typeof(MeshRenderer), UnityComponentBinding.WrenName)]
// 	public class MeshRendererBinding : Class
// 	{
// 		[WrenchMethod(MethodType.FieldGetter)]
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
// 		[WrenchMethod(MethodType.FieldSetter)]
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
// }
