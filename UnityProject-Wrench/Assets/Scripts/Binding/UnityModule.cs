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
		public static bool ExpectObject<T>(in Vm vm, in Slot slot, out ForeignObject<T> value) where T : Object
		{
			value = new ForeignObject<T>();
			if (ExpectValue.IsOfValueType(vm, slot, ValueType.Foreign, true) == false) return false;
			value = slot.GetForeign<T>();
			return true;
		}
	}

	[WrenchClass(typeof(UnityModule), nameof(GameObject), typeof(GameObject))]
	public class GameObjectBinding : Class
	{
		[WrenchMethod(MethodType.Construct)]
		private void Create(Vm vm, Slot self)
		{
			var foreign = self.GetForeign<GameObject>();
			foreign.Value = new GameObject();
		}

		[WrenchMethod(MethodType.Construct)]
		private static void Create(in Vm vm, ForeignObject<GameObject> self, string name)
		{
			self.Value = new GameObject(name);
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
	}
}
