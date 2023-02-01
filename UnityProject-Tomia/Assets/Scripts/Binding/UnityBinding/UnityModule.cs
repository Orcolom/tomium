using System;
using System.Collections.Generic;
using Binding.UnityBinding;
using UnityEngine;
using Tomia;
using Tomia.Builder;
using Tomia.Builder.Tokens;
using MethodBody = Tomia.Builder.MethodBody;
using Module = Tomia.Builder.Module;
using Object = UnityEngine.Object;
// ReSharper disable InconsistentNaming

using GoRelay = Binding.UnityGameObjectBinding;

namespace Binding
{
	public class UnityModule : Module
	{
		public class Class : Tomia.Builder.Class
		{
			public readonly Type ValueType;
			
			protected Class(string name, string inherits = null, Type type = null, ClassBody body = null,
				Attributes attributes = null)
				: base(name, inherits, type != null ? ForeignClass.DefaultAlloc() : default, body, attributes)
			{
				if (type == null) return;
				ValueType = type;
				TypesById.Add(name, this);
				IdByType.Add(type, this);
			}
		}
		
		public static readonly BasicToken f_AddComponentToken = Token.DangerousInsert(@$"
var isWren = {UtilityBinding.WrenName}.{UtilityBinding.MetaClassDerivesFrom__MetaClass_MetaClass}(arg0, {WrenComponentBinding.WrenName})
if (isWren) {{
	var instance = arg0.New()
	instance.SetGameObject_(this)
	f_RegisterAddComponent(""%(arg0)"", instance)
	instance.Awake()
	return instance
}} else {{
	return f_AddComponent(""%(arg0)"")
}}");

		public static bool ExpectObject<T>(Slot slot, out ForeignObject<T> value) where T : Object
		{
			if (ExpectValue.ExpectForeign(slot, out value, true) == false) return false;
			// TODO: unity null check?
			return true;
		}

		private static readonly Dictionary<string, UnityModule.Class> TypesById = new Dictionary<string, UnityModule.Class>();
		private static readonly Dictionary<Type, UnityModule.Class> IdByType = new Dictionary<Type, UnityModule.Class>();

		public UnityModule() : base("Unity")
		{
			Add(new Import(UtilityBinding.WrenName, new ImportVariable(UtilityBinding.WrenName)));
			Add(new GameObjectBinding());
			Add(new UnityGameObjectBinding());
			Add(new WrenGameObjectBinding());
			Add(new ComponentBinding());
			Add(new UnityComponentBinding());
			Add(new WrenComponentBinding());
			Add(new Vector3Binding());
			Add(new QuaternionBinding());
			Add(new TransformBinding());
		}

		public static bool TryGetId(Vm vm, Type type, out UnityModule.Class id)
		{
			if (IdByType.TryGetValue(type, out id)) return true;

			vm.Slot0.SetString($"{type.Name} is not a component");
			vm.Abort(vm.Slot0);
			return false;
		}

		public static bool ExpectType(Vm vm, string id, out UnityModule.Class type)
		{
			if (TypesById.TryGetValue(id, out type)) return true;
			
			vm.Slot0.SetString($"{id} is not a component");
			vm.Abort(vm.Slot0);
			return false;
		}

		public static void SetNewForeign<T>(Vm vm, Slot slot, UnityModule.Class type, T data = default)
		{
			slot.GetVariable(type.Module.Path, type.Name.Text); // TODO: does this work for external types? 
			slot.SetNewForeign(slot, data);
		}
	}

	public class GameObjectBinding : UnityModule.Class
	{
		public const string WrenName = "GameObject";

		public GameObjectBinding() : base(nameof(GameObject))
		{
			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(GoRelay.New)), new MethodBody
			{
				Token.DangerousInsert($"return {GoRelay.WrenName}.{nameof(GoRelay.New)}()"),
			}));

			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(GoRelay.New), 1), new MethodBody
			{
				Token.DangerousInsert($"return {GoRelay.WrenName}.{nameof(GoRelay.New)}(arg0)"),
			}));
			
			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(GoRelay.CreatePrimitiveCube)), new MethodBody
			{
				Token.DangerousInsert($"return {GoRelay.WrenName}.{nameof(GoRelay.CreatePrimitiveCube)}()"),
			}));

			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(GoRelay.CreatePrimitiveCapsule)), new MethodBody
			{
				Token.DangerousInsert($"return {GoRelay.WrenName}.{nameof(GoRelay.CreatePrimitiveCapsule)}()"),
			}));
			
			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(GoRelay.CreatePrimitiveCylinder)), new MethodBody
			{
				Token.DangerousInsert($"return {GoRelay.WrenName}.{nameof(GoRelay.CreatePrimitiveCylinder)}()"),
			}));
			
			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(GoRelay.CreatePrimitivePlane)), new MethodBody
			{
				Token.DangerousInsert($"return {GoRelay.WrenName}.{nameof(GoRelay.CreatePrimitivePlane)}()"),
			}));
			
			Add(new Method(Signature.Create(MethodType.StaticMethod, nameof(GoRelay.CreatePrimitiveSphere)), new MethodBody
			{
				Token.DangerousInsert($"return {GoRelay.WrenName}.{nameof(GoRelay.CreatePrimitiveSphere)}()"),
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

			UnityModule.SetNewForeign(vm, vm.Slot0, type, component);
		}

		public static void f_AddComponent(Vm vm, GameObject self, string typeId)
		{
			if (UnityModule.ExpectType(vm, typeId, out var type) == false) return;
			var component = self.AddComponent(type.ValueType);

			UnityModule.SetNewForeign(vm, vm.Slot0, type, component);
		}
	}

	public class UnityGameObjectBinding : UnityModule.Class
	{
		public const string WrenName = "UnityGameObject";

		public UnityGameObjectBinding() : base(WrenName, GameObjectBinding.WrenName, typeof(GameObject))
		{
			Add(new Method(Signature.Create(MethodType.Method, "GetComponent", 1), new MethodBody
			{
				Token.DangerousInsert($"return {nameof(f_GetComponent_)}(\"%(arg0)\", arg0 is {WrenComponentBinding.WrenName})"),
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
			
			GameObjectBinding.name(vm, self.Value);
		}

		private static void SetName(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<GameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string name) == false) return;
			
			GameObjectBinding.name(vm, self.Value, name);
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

			GameObjectBinding.f_GetComponent_(vm, self.Value, typeId);
		}

		private static void f_AddComponent_(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<GameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string typeId) == false) return;
			
			GameObjectBinding.f_AddComponent(vm, self.Value, typeId);
		}

		private static void f_RegisterAddComponent_(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<GameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string typeId) == false) return;
			if (ExpectValue.ExpectHandle(vm.Slot2, out Handle instance) == false) return;

			var go = self.Value.AddComponent<WrenGameObject>();
			go.Init(vm);
			go.RegisterAddComponent(typeId, instance);
		}
	}

	public class WrenGameObjectBinding : UnityModule.Class
	{
		public WrenGameObjectBinding() : base("WrenGameObject", GameObjectBinding.WrenName)
		{
			// Add(new Method(Signature.Create(MethodType.StaticMethod, "type"), new MethodBody
			// {
			// 	Token.DangerousInsert($"return {GameObjectBinding.WrenName}.type"),
			// }));

			Add(new Method(Signature.Create(MethodType.Method, "GetComponent", 1), new MethodBody
			{
				Token.DangerousInsert($"return {nameof(f_GetComponent_)}(\"%(arg0)\", arg0 is {WrenComponentBinding.WrenName})"),
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
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<WrenGameObject> self) == false) return;
			
			GameObjectBinding.name(vm, self.Value.gameObject);
		}

		private static void SetName(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<WrenGameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string name) == false) return;

			GameObjectBinding.name(vm, self.Value.gameObject, name);
		}

		private static void f_GetComponent_(Vm vm)
		{
			vm.EnsureSlots(3);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<WrenGameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string typeId) == false) return;
			if (ExpectValue.ExpectBool(vm.Slot2, out bool isWrenComponent) == false) return;

			
			if (isWrenComponent) self.Value.f_GetComponent(vm, typeId);
			else GameObjectBinding.f_GetComponent_(vm, self.Value.gameObject, typeId);
		}

		private static void f_AddComponent_(Vm vm)
		{
			vm.EnsureSlots(2);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<WrenGameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string typeId) == false) return;
			
			GameObjectBinding.f_AddComponent(vm, self.Value.gameObject, typeId);
		}

		private static void f_RegisterAddComponent_(Vm vm)
		{
			vm.EnsureSlots(3);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<WrenGameObject> self) == false) return;
			if (ExpectValue.ExpectString(vm.Slot1, out string typeId) == false) return;
			if (ExpectValue.ExpectHandle(vm.Slot2, out Handle instance) == false) return;

			self.Value.RegisterAddComponent(typeId, instance);
		}
	}

	public class ComponentBinding : UnityModule.Class
	{
		public const string WrenName = "Component";
		public ComponentBinding() : base(WrenName){}
	}

	public class UnityComponentBinding : UnityModule.Class
	{
		public const string WrenName = "UnityComponent";

		public UnityComponentBinding() : base(WrenName, ComponentBinding.WrenName)
		{
			Add(new Method(Signature.Create(MethodType.FieldGetter, nameof(gameObject)), new ForeignMethod(gameObject)));
		}

		public static void gameObject(Vm vm)
		{
			vm.EnsureSlots(1);
			if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Component> self) == false) return;
			
			if (UnityModule.TryGetId(vm, typeof(GameObject), out var typeId) == false) return;
			UnityModule.SetNewForeign(vm, vm.Slot0, typeId, self.Value.gameObject);
		}
	}

	public class WrenComponentBinding : UnityModule.Class
	{
		public const string WrenName = "WrenComponent";

		public WrenComponentBinding() : base(WrenName, ComponentBinding.WrenName)
		{
			Add(new Method(Signature.Create(MethodType.FieldGetter, nameof(UnityComponentBinding.gameObject)), new MethodBody
			{
				Token.DangerousInsert("return _gameObject"),
			}));

			Add(new Method(Signature.Create(MethodType.Method, "SetGameObject_", 1), new MethodBody
			{
				Token.DangerousInsert("_gameObject = arg0"),
			}));
		}
	}
}
