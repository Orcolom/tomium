using System;
using UnityEngine;
using Wrench;
using Wrench.Builder;
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
			value = new ForeignObject<T>();
			if (ExpectValue.IsOfValueType(vm, slot, ValueType.Foreign, true) == false) return false;
			value = slot.GetForeign<T>();
			return true;
		}
	}

	[WrenchClass(typeof(UnityModule), nameof(Transform), typeof(Transform))]
	public class TransformBinding : Class { }

	[WrenchClass(typeof(UnityModule), nameof(GameObject), typeof(GameObject))]
	public class GameObjectBinding : Class
	{
		[WrenchMethod(MethodType.Construct)]
		private void New(Vm vm, Slot self)
		{
			var foreign = self.GetForeign<GameObject>();
			foreign.Value = new GameObject();
		}

		[WrenchMethod(MethodType.Construct)]
		private void New(Vm vm, Slot self, Slot name)
		{
			var foreign = self.GetForeign<GameObject>();
			foreign.Value = new GameObject(name.GetString());
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private string Name(Vm vm, ForeignObject<GameObject> self)
		{
			return self.Value.name;
		}

		[WrenchMethod(MethodType.FieldSetter)]
		private void Name(Vm vm, ForeignObject<GameObject> self, string name)
		{
			self.Value.name = name;
		}
		
		[WrenchMethod(MethodType.Method)]
		private void GetComponent(Vm vm, ForeignObject<GameObject> self, Slot type)
		{
			Debug.Log(type.GetValueType());
		}
	}
}
