using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Wrench;
using Wrench.Builder;
using MethodBody = Wrench.Builder.MethodBody;
using Module = Wrench.Builder.Module;
using Object = UnityEngine.Object;
using ValueType = Wrench.ValueType;

namespace Binding
{
	[WrenchModule("Unity")]
	public class UnityModule : Module
	{
		public const string ComponentName = "BaseComponent";

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
			slot.GetVariable(Instance.Path, type);
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

		public new static void Name(Vm vm, GameObject self)
		{
			vm.Slot0.SetString(self.name);
		}

		public new static void Name(Vm vm, GameObject self, string name)
		{
			self.name = name;
		}

		public static void f_GetComponent(Vm vm, GameObject self, string typeId)
		{
			if (UnityModule.ExpectType(vm, typeId, out var type) == false) return;

			if (self.TryGetComponent(type, out var  component) == false)
			{
				vm.Slot0.SetNull();
				return;
			}

			UnityModule.SetNewForeign(vm, vm.Slot0, typeId, component);
		}
	}

	[WrenchClass(typeof(UnityModule), WrenName, typeof(GameObject), GameObjectBinding.WrenName)]
	public class UnityGameObjectBinding : Class
	{
		public const string WrenName = "UnityGameObject";

		public UnityGameObjectBinding()
		{
			Add(new Method(Signature.Create(MethodType.StaticMethod, "type"), new MethodBody
			{
				Token.DangerousInsert($"return {GameObjectBinding.WrenName}.type"),
			}));

			Add(new Method(Signature.Create(MethodType.Method, "GetComponent", 1), new MethodBody
			{
				Token.DangerousInsert($"return {nameof(f_GetComponent)}(\"%(arg0)\", arg0 is {WrenComponentBinding.WrenName})"),
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

		[WrenchMethod(MethodType.FieldGetter)]
		private new void Name(Vm vm, ForeignObject<GameObject> self)
			=> GameObjectBinding.Name(vm, self.Value);

		[WrenchMethod(MethodType.FieldSetter)]
		private new void Name(Vm vm, ForeignObject<GameObject> self, string name)
			=> GameObjectBinding.Name(vm, self.Value, name);

		[WrenchMethod(MethodType.Method)]
		private static void f_GetComponent(Vm vm, ForeignObject<GameObject> self, string typeId, bool isWrenComponent)
		{
			if (isWrenComponent)
			{
				vm.Slot0.SetNull();
				return;
			}

			GameObjectBinding.f_GetComponent(vm, self.Value, typeId);
		}
	}

	[WrenchClass(typeof(UnityModule), "WrenGameObject", typeof(WrenGameObject), GameObjectBinding.WrenName)]
	public class WrenGameObjectBinding : Class
	{
		public WrenGameObjectBinding()
		{
			Add(new Method(Signature.Create(MethodType.StaticMethod, "type"), new MethodBody
			{
				Token.DangerousInsert($"return {GameObjectBinding.WrenName}.type"),
			}));

			Add(new Method(Signature.Create(MethodType.Method, "GetComponent", 1), new MethodBody
			{
				Token.DangerousInsert($"return {nameof(f_GetComponent)}(\"%(arg0)\", arg0 is {WrenComponentBinding.WrenName})"),
			}));
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private new void Name(Vm vm, ForeignObject<WrenGameObject> self)
			=> GameObjectBinding.Name(vm, self.Value.gameObject);

		[WrenchMethod(MethodType.FieldSetter)]
		private new void Name(Vm vm, ForeignObject<WrenGameObject> self, string name)
			=> GameObjectBinding.Name(vm, self.Value.gameObject, name);

		[WrenchMethod(MethodType.Method)]
		private static void f_GetComponent(Vm vm, ForeignObject<WrenGameObject> self, string typeId, bool isWrenComponent)
		{
			if (isWrenComponent) self.Value.f_GetComponent(vm, typeId);
			else GameObjectBinding.f_GetComponent(vm, self.Value.gameObject, typeId);
		}
	}


	[WrenchClass(typeof(UnityModule), WrenName)]
	public class ComponentBinding : Class
	{
		public const string WrenName = "Component";

		public static void GameObject(Vm vm, Component self)
		{
			if (UnityModule.ExpectId(vm, typeof(GameObject), out var typeId) == false) return;
			UnityModule.SetNewForeign(vm, vm.Slot0, typeId, self.gameObject);
		}
	}

	[WrenchClass(typeof(UnityModule), WrenName, inherit: ComponentBinding.WrenName)]
  public class UnityComponentBinding : Class
  {
  	public const string WrenName = "UnityComponent";

		[WrenchMethod(MethodType.FieldGetter)]
		public void GameObject(Vm vm, ForeignObject<Component> self)
			=> ComponentBinding.GameObject(vm, self.Value);
	}
		
	[WrenchClass(typeof(UnityModule), WrenName, typeof(WrenComponentData), ComponentBinding.WrenName)]
	public class WrenComponentBinding : Class
	{
		public const string WrenName = "WrenComponent";
		
		[WrenchExpect(typeof(ForeignObject<WrenComponentData>), true)]
		public static bool Expect(Vm vm, Slot slot, out ForeignObject<WrenComponentData> value)
			=> ExpectValue.ExpectForeign(vm, slot, out value, true);
		
		[WrenchMethod(MethodType.FieldGetter)]
		public void GameObject(Vm vm, ForeignObject<WrenComponentData> self)
			=> ComponentBinding.GameObject(vm, self.Value.GameObject);
	}
}
