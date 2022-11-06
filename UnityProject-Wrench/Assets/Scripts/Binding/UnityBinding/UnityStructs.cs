using UnityEngine;
using Wrench;
using Wrench.Builder;

namespace Binding.UnityBinding
{
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

		[WrenchMethod(MethodType.ToString)]
		private void ToString(Vm vm, ForeignObject<Vector3> self)
		{
			vm.Slot0.SetString(self.Value.ToString());
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

}
