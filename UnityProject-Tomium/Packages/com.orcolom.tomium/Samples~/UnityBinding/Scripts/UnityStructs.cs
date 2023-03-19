using UnityEngine;
using Tomium;
using Tomium.Builder;
using Tomium.Samples.UnityBinding;

namespace Tomium.Samples.UnityBinding
{
	public class Vector3Class : UnityModule.Class<Vector3>
	{
		public Vector3Class() : base(nameof(Vector3))
		{
			const string x = "x";
			Add(new Method(Signature.Create(MethodType.FieldGetter, x), new ForeignMethod(GetX)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, x), new ForeignMethod(SetX)));
			
			const string y = "y";
			Add(new Method(Signature.Create(MethodType.FieldGetter, y), new ForeignMethod(GetY)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, y), new ForeignMethod(SetY)));
			
			const string z = "z";
			Add(new Method(Signature.Create(MethodType.FieldGetter, z), new ForeignMethod(GetZ)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, z), new ForeignMethod(SetZ)));

			Add(new Method(Signature.Create(MethodType.ToString), new ForeignMethod(ToString)));
		}

		public static bool Expect(Slot slot, out ForeignObject<Vector3> value)
		{
			return ExpectValue.ExpectForeign(slot, out value);
		}

		private static void GetX(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm.Slot0, out ForeignObject<Vector3> self) == false) return;
			
			vm.Slot0.SetFloat(self.Value.x);
		}

		private static void SetX(Vm vm)
		{
			vm.EnsureSlots(2);
			if (Expect(vm.Slot0, out ForeignObject<Vector3> self) == false) return;
			if (ExpectValue.ExpectFloat(vm.Slot1, out float x) == false) return;
			
			var selfValue = self.Value;
			selfValue.x = x;
			self.Value = selfValue;
		}

		private static void GetY(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm.Slot0, out ForeignObject<Vector3> self) == false) return;

			vm.Slot0.SetFloat(self.Value.y);
		}

		private static void SetY(Vm vm)
		{
			vm.EnsureSlots(2);
			if (Expect(vm.Slot0, out ForeignObject<Vector3> self) == false) return;
			if (ExpectValue.ExpectFloat(vm.Slot1, out float y) == false) return;

			var selfValue = self.Value;
			selfValue.y = y;
			self.Value = selfValue;
		}

		private static void GetZ(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm.Slot0, out ForeignObject<Vector3> self) == false) return;

			vm.Slot0.SetFloat(self.Value.z);
		}

		private static void SetZ(Vm vm)
		{
			vm.EnsureSlots(2);
			if (Expect(vm.Slot0, out ForeignObject<Vector3> self) == false) return;
			if (ExpectValue.ExpectFloat(vm.Slot1, out float z) == false) return;

			var selfValue = self.Value;
			selfValue.z = z;
			self.Value = selfValue;
		}

		private void ToString(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm.Slot0, out ForeignObject<Vector3> self) == false) return;

			vm.Slot0.SetString(self.Value.ToString());
		}
	}

	public class ColorClass : UnityModule.Class<Color>
	{
		public ColorClass() : base(nameof(Color))
		{
			const string r = "r";
			Add(new Method(Signature.Create(MethodType.FieldGetter, r), new ForeignMethod(GetR)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, r), new ForeignMethod(SetR)));
			
			const string g = "g";
			Add(new Method(Signature.Create(MethodType.FieldGetter, g), new ForeignMethod(GetG)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, g), new ForeignMethod(SetG)));
			
			const string b = "b";
			Add(new Method(Signature.Create(MethodType.FieldGetter, b), new ForeignMethod(GetB)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, b), new ForeignMethod(SetB)));
			
			const string a = "a";
			Add(new Method(Signature.Create(MethodType.FieldGetter, a), new ForeignMethod(GetA)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, a), new ForeignMethod(SetA)));

			Add(new Method(Signature.Create(MethodType.ToString), new ForeignMethod(ToString)));
		}
		
		public static bool Expect(Vm vm, Slot slot, out ForeignObject<Color> value)
		{
			return ExpectValue.ExpectForeign(slot, out value);
		}

		private static void GetR(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm, vm.Slot0, out var self) == false) return;
			
			vm.Slot0.SetFloat(self.Value.r);
		}

		private static void SetR(Vm vm)
		{
			vm.EnsureSlots(2);
			if (Expect(vm, vm.Slot0, out var self) == false) return;
			if (ExpectValue.ExpectFloat(vm.Slot1, out float r) == false) return;

			var selfValue = self.Value;
			selfValue.r = r;
			self.Value = selfValue;
		}

		private static void GetG(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm, vm.Slot0, out var self) == false) return;
			
			vm.Slot0.SetFloat(self.Value.g);
		}

		private static void SetG(Vm vm)
		{
			vm.EnsureSlots(2);
			if (Expect(vm, vm.Slot0, out var self) == false) return;
			if (ExpectValue.ExpectFloat(vm.Slot1, out float g) == false) return;

			var selfValue = self.Value;
			selfValue.g = g;
			self.Value = selfValue;
		}

		private static void GetB(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm, vm.Slot0, out var self) == false) return;
			
			vm.Slot0.SetFloat(self.Value.b);
		}

		private static void SetB(Vm vm)
		{
			vm.EnsureSlots(2);
			if (Expect(vm, vm.Slot0, out var self) == false) return;
			if (ExpectValue.ExpectFloat(vm.Slot1, out float b) == false) return;
			
			var selfValue = self.Value;
			selfValue.b = b;
			self.Value = selfValue;
		}
		
		private static void GetA(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm, vm.Slot0, out var self) == false) return;

			vm.Slot0.SetFloat(self.Value.a);
		}

		private static void SetA(Vm vm)
		{
			vm.EnsureSlots(2);
			if (Expect(vm, vm.Slot0, out var self) == false) return;
			if (ExpectValue.ExpectFloat(vm.Slot1, out float a) == false) return;
			
			var selfValue = self.Value;
			selfValue.a = a;
			self.Value = selfValue;
		}

		private static void ToString(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm, vm.Slot0, out var self) == false) return;
			
			vm.Slot0.SetString(self.Value.ToString());
		}
	}
	
	public class QuaternionClass : UnityModule.Class<Quaternion>
	{
		public QuaternionClass() : base(nameof(Quaternion))
		{
			const string x = "x";
			Add(new Method(Signature.Create(MethodType.FieldGetter, x), new ForeignMethod(GetX)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, x), new ForeignMethod(SetX)));
			
			const string y = "y";
			Add(new Method(Signature.Create(MethodType.FieldGetter, y), new ForeignMethod(GetY)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, y), new ForeignMethod(SetY)));
			
			const string z = "z";
			Add(new Method(Signature.Create(MethodType.FieldGetter, z), new ForeignMethod(GetZ)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, z), new ForeignMethod(SetZ)));
			
			const string w = "w";
			Add(new Method(Signature.Create(MethodType.FieldGetter, w), new ForeignMethod(GetW)));
			Add(new Method(Signature.Create(MethodType.FieldSetter, w), new ForeignMethod(SetW)));

			Add(new Method(Signature.Create(MethodType.ToString), new ForeignMethod(ToString)));
		}
		
		public static bool Expect(Slot slot, out ForeignObject<Quaternion> value)
		{
			return ExpectValue.ExpectForeign(slot, out value);
		}

		private static void GetX(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm.Slot0, out var self) == false) return;
			
			vm.Slot0.SetFloat(self.Value.x);
		}

		private static void SetX(Vm vm)
		{
			vm.EnsureSlots(2);
			if (Expect(vm.Slot0, out var self) == false) return;
			if (ExpectValue.ExpectFloat(vm.Slot1, out float x) == false) return;

			var selfValue = self.Value;
			selfValue.x = x;
			self.Value = selfValue;
		}

		private static void GetY(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm.Slot0, out var self) == false) return;
			
			vm.Slot0.SetFloat(self.Value.y);
		}

		private static void SetY(Vm vm)
		{
			vm.EnsureSlots(2);
			if (Expect(vm.Slot0, out var self) == false) return;
			if (ExpectValue.ExpectFloat(vm.Slot1, out float y) == false) return;

			var selfValue = self.Value;
			selfValue.y = y;
			self.Value = selfValue;
		}

		private static void GetZ(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm.Slot0, out var self) == false) return;
			
			vm.Slot0.SetFloat(self.Value.z);
		}

		private static void SetZ(Vm vm)
		{
			vm.EnsureSlots(2);
			if (Expect(vm.Slot0, out var self) == false) return;
			if (ExpectValue.ExpectFloat(vm.Slot1, out float z) == false) return;
			
			var selfValue = self.Value;
			selfValue.z = z;
			self.Value = selfValue;
		}

		private static void GetW(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm.Slot0, out var self) == false) return;
			
			vm.Slot0.SetFloat(self.Value.w);
		}

		private static void SetW(Vm vm)
		{		
			vm.EnsureSlots(2);
			if (Expect(vm.Slot0, out var self) == false) return;
			if (ExpectValue.ExpectFloat(vm.Slot1, out float w) == false) return;

			var selfValue = self.Value;
			selfValue.w = w;
			self.Value = selfValue;
		}
		
		private static void ToString(Vm vm)
		{
			vm.EnsureSlots(1);
			if (Expect(vm.Slot0, out var self) == false) return;
			
			vm.Slot0.SetString(self.Value.ToString());
		}
	}
}
