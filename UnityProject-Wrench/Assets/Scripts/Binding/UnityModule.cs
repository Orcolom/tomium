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
		[WrenchExpect(typeof(ForeignObject<Object>), true)]
		public static bool ExpectObject<T>(Vm vm, Slot slot, out ForeignObject<T> value) where T : Object
		{
			if (ExpectValue.ExpectForeign(vm, slot, out value, true) == false) return false;
			// TODO: unity null check
			return true;
		}

		public static UnityModule Instance;
		public static Dictionary<string, Type> Types = new Dictionary<string, Type>();

		static UnityModule()
		{
			Instance = new UnityModule();

			foreach (var pair in Instance.Classes)
			{
				var type = pair.Value.GetType();
				var wrenchAttribute = type.GetCustomAttribute<WrenchClassAttribute>();
				Types.Add(wrenchAttribute.Name, wrenchAttribute.ForType);
			}
		}

		private UnityModule() { }
	}

	[WrenchClass(typeof(UnityModule), nameof(Vector3), typeof(Vector3))]
	public class Vector3Binding : Class
	{
		[WrenchExpect(typeof(ForeignObject<Vector3>), true)]
		public static bool Expect(Vm vm, Slot slot, out ForeignObject<Vector3> value)
		{
			return ExpectValue.ExpectForeign(vm, slot, out value);
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void X(Vm vm, ForeignObject<Vector3> self) => vm.Slot0.SetFloat(self.Value.x);

		[WrenchMethod(MethodType.FieldSetter)]
		private void X(Vm vm, ForeignObject<Vector3> self, float x)
		{
			var selfValue = self.Value;
			selfValue.x = x;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void Y(Vm vm, ForeignObject<Vector3> self) => vm.Slot0.SetFloat(self.Value.y);

		[WrenchMethod(MethodType.FieldSetter)]
		private void Y(Vm vm, ForeignObject<Vector3> self, float y)
		{
			var selfValue = self.Value;
			selfValue.y = y;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void Z(Vm vm, ForeignObject<Vector3> self) => vm.Slot0.SetFloat(self.Value.z);

		[WrenchMethod(MethodType.FieldSetter)]
		private void Z(Vm vm, ForeignObject<Vector3> self, float z)
		{
			var selfValue = self.Value;
			selfValue.z = z;
			self.Value = selfValue;
		}
	}

	[WrenchClass(typeof(UnityModule), nameof(Quaternion), typeof(Quaternion))]
	public class QuaternionBinding : Class
	{
		[WrenchExpect(typeof(ForeignObject<Quaternion>), true)]
		public static bool Expect(Vm vm, Slot slot, out ForeignObject<Quaternion> value)
		{
			return ExpectValue.ExpectForeign(vm, slot, out value);
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void X(Vm vm, ForeignObject<Quaternion> self) => vm.Slot0.SetFloat(self.Value.x);

		[WrenchMethod(MethodType.FieldSetter)]
		private void X(Vm vm, ForeignObject<Quaternion> self, float x)
		{
			var selfValue = self.Value;
			selfValue.x = x;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void Y(Vm vm, ForeignObject<Quaternion> self) => vm.Slot0.SetFloat(self.Value.y);

		[WrenchMethod(MethodType.FieldSetter)]
		private void Y(Vm vm, ForeignObject<Quaternion> self, float y)
		{
			var selfValue = self.Value;
			selfValue.y = y;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void Z(Vm vm, ForeignObject<Quaternion> self) => vm.Slot0.SetFloat(self.Value.z);

		[WrenchMethod(MethodType.FieldSetter)]
		private void Z(Vm vm, ForeignObject<Quaternion> self, float z)
		{
			var selfValue = self.Value;
			selfValue.z = z;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void W(Vm vm, ForeignObject<Quaternion> self) => vm.Slot0.SetFloat(self.Value.w);

		[WrenchMethod(MethodType.FieldSetter)]
		private void W(Vm vm, ForeignObject<Quaternion> self, float w)
		{
			var selfValue = self.Value;
			selfValue.w = w;
			self.Value = selfValue;
		}
	}

	[WrenchClass(typeof(UnityModule), nameof(Transform), typeof(Transform))]
	public class TransformBinding : Class { }

	[WrenchClass(typeof(UnityModule), nameof(GameObject), typeof(GameObject))]
	public class GameObjectBinding : Class
	{
		public GameObjectBinding()
		{
			Add(new Method(Signature.Create(MethodType.Method, "GetComponent", 1), new MethodBody
			{
				Token.DangerousInsert($"return this.{nameof(f_GetComponent)}(\"%(arg0)\")"),
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
		private new string Name(Vm vm, ForeignObject<GameObject> self)
		{
			return self.Value.name;
		}

		[WrenchMethod(MethodType.FieldSetter)]
		private new void Name(Vm vm, ForeignObject<GameObject> self, string name)
		{
			self.Value.name = name;
		}

		[WrenchMethod(MethodType.Method)]
		private void f_GetComponent(Vm vm, ForeignObject<GameObject> self, string typeId)
		{
			if (UnityModule.Types.TryGetValue(typeId, out var type) == false)
			{
				vm.Slot0.SetNull();
				return;
			}

			var component = self.Value.GetComponent(type);
			if (component == null)
			{
				vm.Slot0.SetNull();
				return;
			}

			vm.Slot0.GetVariable(UnityModule.Instance.Path, typeId);
			vm.Slot0.SetNewForeign(vm.Slot0, component);
		}
	}
}
