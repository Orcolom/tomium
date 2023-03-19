using Binding;
using UnityEngine;
using Tomium.Builder;
using MethodBody = Tomium.Builder.MethodBody;
// ReSharper disable InconsistentNaming

namespace Tomium.Samples.UnityBinding
{
	public class GameObjectClass : UnityModule.Class
	{
		public const string WrenName = "GameObject";

		public GameObjectClass() : base(nameof(GameObject))
		{
			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(UnityGameObjectClass.New)), new MethodBody
			{
				Token.DangerousInsert($"return {UnityGameObjectClass.WrenName}.{nameof(UnityGameObjectClass.New)}()"),
			}));

			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(UnityGameObjectClass.New), 1), new MethodBody
			{
				Token.DangerousInsert($"return {UnityGameObjectClass.WrenName}.{nameof(UnityGameObjectClass.New)}(arg0)"),
			}));
			
			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(UnityGameObjectClass.CreatePrimitiveCube)), new MethodBody
			{
				Token.DangerousInsert($"return {UnityGameObjectClass.WrenName}.{nameof(UnityGameObjectClass.CreatePrimitiveCube)}()"),
			}));

			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(UnityGameObjectClass.CreatePrimitiveCapsule)), new MethodBody
			{
				Token.DangerousInsert($"return {UnityGameObjectClass.WrenName}.{nameof(UnityGameObjectClass.CreatePrimitiveCapsule)}()"),
			}));
			
			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(UnityGameObjectClass.CreatePrimitiveCylinder)), new MethodBody
			{
				Token.DangerousInsert($"return {UnityGameObjectClass.WrenName}.{nameof(UnityGameObjectClass.CreatePrimitiveCylinder)}()"),
			}));
			
			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(UnityGameObjectClass.CreatePrimitivePlane)), new MethodBody
			{
				Token.DangerousInsert($"return {UnityGameObjectClass.WrenName}.{nameof(UnityGameObjectClass.CreatePrimitivePlane)}()"),
			}));
			
			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(UnityGameObjectClass.CreatePrimitiveSphere)), new MethodBody
			{
				Token.DangerousInsert($"return {UnityGameObjectClass.WrenName}.{nameof(UnityGameObjectClass.CreatePrimitiveSphere)}()"),
			}));
		}

		public static void name(Vm vm, GameObject self)
		{
			vm.Slot0.SetString(self.name);
		}

		public static void name(Vm vm, GameObject self, string name)
		{
			self.name = name;
		}

		public static void f_GetComponent_(Vm vm, GameObject self, string typeId)
		{
			if (UnityModule.ExpectType(vm, typeId, out var type) == false) return;

			if (self.TryGetComponent(type.ValueType, out var component) == false)
			{
				vm.Slot0.SetNull();
				return;
			}

			UnityModule.SetNewForeignObject(vm, vm.Slot0, type, component);
		}

		public static void f_AddComponent(Vm vm, GameObject self, string typeId)
		{
			if (UnityModule.ExpectType(vm, typeId, out var type) == false) return;
			var component = self.AddComponent(type.ValueType);

			UnityModule.SetNewForeignObject(vm, vm.Slot0, type, component);
		}
	}

	public class UnityGameObjectClass : UnityModule.Class<GameObject>
	{
		public const string WrenName = "UnityGameObject";

		public UnityGameObjectClass() : base(WrenName, GameObjectClass.WrenName)
		{
			Add(new Method(Signature.Create(MethodType.Method, "GetComponent", 1), new MethodBody
			{
				Token.DangerousInsert($"return {nameof(f_GetComponent_)}(\"%(arg0)\", arg0 is {WrenComponentClass.WrenName})"),
			}));

			Add(new Method(Signature.Create(MethodType.Method, "AddComponent", 1), new MethodBody
			{
				UnityModule.f_AddComponentToken,
			}));
			
			const string @new = "New"; 
			Add(new Method(Signature.Create(MethodType.Construct, @new), new ForeignMethod(New)));
			Add(new Method(Signature.Create(MethodType.Construct, @new, 1), new ForeignMethod(New__Name)));
			
			Add(new Method(Signature.Create(MethodType.Construct, nameof(CreatePrimitiveCube)), new ForeignMethod(CreatePrimitiveCube)));
			Add(new Method(Signature.Create(MethodType.Construct, nameof(CreatePrimitivePlane)), new ForeignMethod(CreatePrimitivePlane)));
			Add(new Method(Signature.Create(MethodType.Construct, nameof(CreatePrimitiveSphere)), new ForeignMethod(CreatePrimitiveSphere)));
			Add(new Method(Signature.Create(MethodType.Construct, nameof(CreatePrimitiveCapsule)), new ForeignMethod(CreatePrimitiveCapsule)));
			Add(new Method(Signature.Create(MethodType.Construct, nameof(CreatePrimitiveCylinder)), new ForeignMethod(CreatePrimitiveCylinder)));

			const string name = "name"; 
			Add(new Method(Signature.Create(MethodType.FieldGetter, name), new ForeignMethod(GetName)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, name), new ForeignMethod(SetName)));
			
			Add(new Method(Signature.Create(MethodType.Method, nameof(f_GetComponent_), 2), new ForeignMethod(f_GetComponent_)));
			Add(new Method(Signature.Create(MethodType.Method, nameof(f_AddComponent_), 1), new ForeignMethod(f_AddComponent_)));
			Add(new Method(Signature.Create(MethodType.Method, nameof(f_RegisterAddComponent_), 2), new ForeignMethod(f_RegisterAddComponent_)));
		}

		#region New

		internal static void New(Vm vm)
		{
			vm.EnsureSlots(1);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<GameObject> self) == false) return;

			self.Value = new GameObject();
		}

		private static void New__Name(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<GameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out var name) == false) return;

			self.Value = new GameObject(name);
		}

		#endregion

		#region Primitives

		internal static void CreatePrimitiveCube(Vm vm) => CreatePrimitive(vm, PrimitiveType.Cube);
		internal static void CreatePrimitivePlane(Vm vm) => CreatePrimitive(vm, PrimitiveType.Plane);
		internal static void CreatePrimitiveSphere(Vm vm) => CreatePrimitive(vm, PrimitiveType.Sphere);
		internal static void CreatePrimitiveCapsule(Vm vm) => CreatePrimitive(vm, PrimitiveType.Capsule);
		internal static void CreatePrimitiveCylinder(Vm vm) => CreatePrimitive(vm, PrimitiveType.Cylinder);
		private static void CreatePrimitive(Vm vm, PrimitiveType type)
		{
			vm.EnsureSlots(1);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<GameObject> self) == false) return;
			
			self.Value = GameObject.CreatePrimitive(type);
		}

		#endregion

		private static void GetName(Vm vm)
		{
			vm.EnsureSlots(1);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<GameObject> self) == false) return;
			
			GameObjectClass.name(vm, self.Value);
		}

		private static void SetName(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<GameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string name) == false) return;
			
			GameObjectClass.name(vm, self.Value, name);
		}

		private static void f_GetComponent_(Vm vm)
		{
			vm.EnsureSlots(3);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<GameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string typeId) == false) return;
			if (ExpectValue.ExpectBool(vm.Slot2, out bool isWrenComponent) == false) return;

			if (isWrenComponent)
			{
				vm.Slot0.SetNull();
				return;
			}

			GameObjectClass.f_GetComponent_(vm, self.Value, typeId);
		}

		private static void f_AddComponent_(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<GameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string typeId) == false) return;
			
			GameObjectClass.f_AddComponent(vm, self.Value, typeId);
		}

		private static void f_RegisterAddComponent_(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<GameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string typeId) == false) return;
			if (ExpectValue.ExpectHandle(vm.Slot2, out Handle instance) == false) return;

			var go = self.Value.AddComponent<WrenComponentHandler>();
			go.Init(vm);
			go.RegisterAddComponent(typeId, instance);
		}
	}

	public class WrenGameObjectClass : UnityModule.Class
	{
		public WrenGameObjectClass() : base("WrenGameObject", GameObjectClass.WrenName)
		{
			Add(new Method(Signature.Create(MethodType.Method, "GetComponent", 1), new MethodBody
			{
				Token.DangerousInsert($"return {nameof(f_GetComponent_)}(\"%(arg0)\", arg0 is {WrenComponentClass.WrenName})"),
			}));

			Add(new Method(Signature.Create(MethodType.Method, "AddComponent", 1), new MethodBody
			{
				UnityModule.f_AddComponentToken,
			}));
			
			const string name = "name"; 
			Add(new Method(Signature.Create(MethodType.FieldGetter, name), new ForeignMethod(GetName)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, name), new ForeignMethod(SetName)));
			
			Add(new Method(Signature.Create(MethodType.Method, nameof(f_GetComponent_), 2), new ForeignMethod(f_GetComponent_)));
			Add(new Method(Signature.Create(MethodType.Method, nameof(f_AddComponent_), 1), new ForeignMethod(f_AddComponent_)));
			Add(new Method(Signature.Create(MethodType.Method, nameof(f_RegisterAddComponent_), 2), new ForeignMethod(f_RegisterAddComponent_)));
		}

		private static void GetName(Vm vm)
		{
			vm.EnsureSlots(1);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<WrenComponentHandler> self) == false) return;
			
			GameObjectClass.name(vm, self.Value.gameObject);
		}

		private static void SetName(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<WrenComponentHandler> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string name) == false) return;

			GameObjectClass.name(vm, self.Value.gameObject, name);
		}

		private static void f_GetComponent_(Vm vm)
		{
			vm.EnsureSlots(3);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<WrenComponentHandler> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string typeId) == false) return;
			if (ExpectValue.ExpectBool(vm.Slot2, out bool isWrenComponent) == false) return;

			
			if (isWrenComponent) self.Value.f_GetComponent(vm, typeId);
			else GameObjectClass.f_GetComponent_(vm, self.Value.gameObject, typeId);
		}

		private static void f_AddComponent_(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<WrenComponentHandler> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string typeId) == false) return;
			
			GameObjectClass.f_AddComponent(vm, self.Value.gameObject, typeId);
		}

		private static void f_RegisterAddComponent_(Vm vm)
		{
			vm.EnsureSlots(3);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<WrenComponentHandler> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string typeId) == false) return;
			if (ExpectValue.ExpectHandle(vm.Slot2, out Handle instance) == false) return;

			self.Value.RegisterAddComponent(typeId, instance);
		}
	}

}