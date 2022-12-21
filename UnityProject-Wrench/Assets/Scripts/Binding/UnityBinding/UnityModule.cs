using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Wrench;
using Wrench.Builder;
using Wrench.Builder.Tokens;
using MethodBody = Wrench.Builder.MethodBody;
using Module = Wrench.Builder.Module;
using Object = UnityEngine.Object;
// ReSharper disable InconsistentNaming

namespace Binding
{
	[WrenchModule("Unity"), WrenchImport(typeof(UtilityBinding))]
	public class UnityModule : Module
	{
		public const string ComponentName = "BaseComponent";

		public static readonly BasicToken f_AddComponentToken = Token.DangerousInsert(@$"
var isWren = {UtilityBinding.WrenName}.{UtilityBinding.MetaClassDerivesFrom__MetaClass_MetaClass}(arg0, {WrenComponentBinding.WrenName})
if (isWren) {{
	var instance = arg0.New()
	instance.SetGameObject_(this)
	f_RegisterAddComponent(""%(arg0)"", instance)
	arg0.Awake()
	return instance
}} else {{
	return f_AddComponent(""%(arg0)"")
}}");

		private UtilityBinding _utility;
		
		[WrenchExpect(typeof(ForeignObject<Object>), true)]
		public static bool ExpectObject<T>(Vm vm, Slot slot, out ForeignObject<T> value) where T : Object
		{
			if (ExpectValue.ExpectForeign(vm, slot, out value, true) == false) return false;
			// TODO: unity null check?
			return true;
		}

		public static UnityModule Instance;
		private static Dictionary<string, Type> TypesById = new Dictionary<string, Type>();
		private static Dictionary<Type, string> IdByType = new Dictionary<Type, string>();

		static UnityModule()
		{
			Instance = new UnityModule();

			foreach (var pair in Instance.Classes)
			{
				var type = pair.Value.GetType();
				var wrenchAttribute = type.GetCustomAttribute<WrenchClassAttribute>();
				if (wrenchAttribute.ForType == null) continue;
				TypesById.Add(wrenchAttribute.Name, wrenchAttribute.ForType);
				IdByType.Add(wrenchAttribute.ForType, wrenchAttribute.Name);
			}
		}

		public static bool ExpectId(Vm vm, Type type, out string id)
		{
			if (IdByType.TryGetValue(type, out id)) return true;

			vm.Slot0.SetString($"{type.Name} is not a component");
			vm.Abort(vm.Slot0);
			return false;
		}

		public static bool ExpectType(Vm vm, string id, out Type type)
		{
			if (TypesById.TryGetValue(id, out type)) return true;

			vm.Slot0.SetString($"{id} is not a component");
			vm.Abort(vm.Slot0);
			return false;
		}

		public static void SetNewForeign<T>(Vm vm, Slot slot, string type, T data = default)
		{
			slot.GetVariable(Instance.Path, type); // TODO: does this work for external types? 
			slot.SetNewForeign(slot, data);
		}
	}


	[WrenchClass(typeof(UnityModule), nameof(GameObject))]
	public class GameObjectBinding : Class
	{
		public const string WrenName = "GameObject";

		public GameObjectBinding()
		{
			Add(new Method(Signature.Create(MethodType.StaticMethod, "New"), new MethodBody
			{
				Token.DangerousInsert($"return {UnityGameObjectBinding.WrenName}.New()"),
			}));

			Add(new Method(Signature.Create(MethodType.StaticMethod, "New", 1), new MethodBody
			{
				Token.DangerousInsert($"return {UnityGameObjectBinding.WrenName}.New(arg0)"),
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

			if (self.TryGetComponent(type, out var component) == false)
			{
				vm.Slot0.SetNull();
				return;
			}

			UnityModule.SetNewForeign(vm, vm.Slot0, typeId, component);
		}

		public static void f_AddComponent(Vm vm, GameObject self, string typeId)
		{
			if (UnityModule.ExpectType(vm, typeId, out var type) == false) return;
			var component = self.AddComponent(type);

			UnityModule.SetNewForeign(vm, vm.Slot0, typeId, component);
		}
	}

	[WrenchClass(typeof(UnityModule), WrenName, typeof(GameObject), GameObjectBinding.WrenName)]
	public class UnityGameObjectBinding : Class
	{
		public const string WrenName = "UnityGameObject";

		public UnityGameObjectBinding()
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
		}

		[WrenchMethod(MethodType.Construct)]
		private void New(Vm vm, Slot self)
		{
			var foreign = self.GetForeign<GameObject>();
			foreign.Value = new GameObject();
		}

		[WrenchMethod(MethodType.Construct)]
		private void New(Vm vm, ForeignObject<GameObject> self, string name)
		{
			self.Value = new GameObject(name);
		}

		[WrenchMethod(MethodType.Method)]
		private new void name(Vm vm, ForeignObject<GameObject> self)
			=> GameObjectBinding.name(vm, self.Value);

		[WrenchMethod(MethodType.Method)]
		private new void name(Vm vm, ForeignObject<GameObject> self, string name)
			=> GameObjectBinding.name(vm, self.Value, name);

		[WrenchMethod(MethodType.Method)]
		private static void f_GetComponent_(Vm vm, ForeignObject<GameObject> self, string typeId, bool isWrenComponent)
		{
			if (isWrenComponent)
			{
				vm.Slot0.SetNull();
				return;
			}

			GameObjectBinding.f_GetComponent_(vm, self.Value, typeId);
		}

		[WrenchMethod(MethodType.Method)]
		private static void f_AddComponent(Vm vm, ForeignObject<GameObject> self, string typeId)
			=> GameObjectBinding.f_AddComponent(vm, self.Value, typeId);

		[WrenchMethod(MethodType.Method)]
		private static void f_RegisterAddComponent(Vm vm, ForeignObject<GameObject> self, string typeId, Handle instance)
		{
			var go = self.Value.AddComponent<WrenGameObject>();
			go.Init(vm);
			go.RegisterAddComponent(typeId, instance);
		}
	}

	[WrenchClass(typeof(UnityModule), "WrenGameObject", typeof(WrenGameObject), GameObjectBinding.WrenName)]
	public class WrenGameObjectBinding : Class
	{
		public WrenGameObjectBinding()
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
		}

		[WrenchMethod(MethodType.Method)]
		private new void name(Vm vm, ForeignObject<WrenGameObject> self)
			=> GameObjectBinding.name(vm, self.Value.gameObject);

		[WrenchMethod(MethodType.Method)]
		private new void name(Vm vm, ForeignObject<WrenGameObject> self, string name)
			=> GameObjectBinding.name(vm, self.Value.gameObject, name);

		[WrenchMethod(MethodType.Method)]
		private static void f_GetComponent_(Vm vm, ForeignObject<WrenGameObject> self, string typeId, bool isWrenComponent)
		{
			if (isWrenComponent) self.Value.f_GetComponent(vm, typeId);
			else GameObjectBinding.f_GetComponent_(vm, self.Value.gameObject, typeId);
		}

		[WrenchMethod(MethodType.Method)]
		private static void f_AddComponent_(Vm vm, ForeignObject<WrenGameObject> self, string typeId)
			=> GameObjectBinding.f_AddComponent(vm, self.Value.gameObject, typeId);

		[WrenchMethod(MethodType.Method)]
		private static void f_RegisterAddComponent_(Vm vm, ForeignObject<WrenGameObject> self, string typeId,
			Handle instance)
			=> self.Value.RegisterAddComponent(typeId, instance);
	}


	[WrenchClass(typeof(UnityModule), WrenName)]
	public class ComponentBinding : Class
	{
		public const string WrenName = "Component";
	}

	[WrenchClass(typeof(UnityModule), WrenName, inherit: ComponentBinding.WrenName)]
	public class UnityComponentBinding : Class
	{
		public const string WrenName = "UnityComponent";

		// public UnityComponentBinding()
		// {
		// 	Add(new Method(Signature.Create(MethodType.StaticMethod, "type"), new MethodBody
		// 	{
		// 		Token.DangerousInsert($"return {ComponentBinding.WrenName}.type"),
		// 	}));
		// }

		[WrenchMethod(MethodType.FieldGetter)]
		public static void gameObject(Vm vm, ForeignObject<Component> self)
		{
			if (UnityModule.ExpectId(vm, typeof(GameObject), out var typeId) == false) return;
			UnityModule.SetNewForeign(vm, vm.Slot0, typeId, self.Value.gameObject);
		}
	}

	[WrenchClass(typeof(UnityModule), WrenName, inherit: ComponentBinding.WrenName)]
	public class WrenComponentBinding : Class
	{
		public const string WrenName = "WrenComponent";

		public WrenComponentBinding()
		{
			// Add(new Method(Signature.Create(MethodType.StaticMethod, "type"), new MethodBody
			// {
			// 	Token.DangerousInsert($"return {ComponentBinding.WrenName}.type"),
			// }));
			//
// 			Add(new Method(Signature.Create(MethodType.Is), new MethodBody
// 			{
// 				Token.DangerousInsert(@$"
// System.print(arg0)
// return {ComponentBinding.WrenName}.type"),
// 			}));

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
