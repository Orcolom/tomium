using UnityEngine;
using Tomia;
using Tomia.Builder;

namespace Binding.UnityBinding
{
	public class Vector3Binding : UnityModule.Class
	{
		public Vector3Binding() : base(nameof(Vector3), null, typeof(Vector3))
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
			if (ExpectValue.ExpectFloat(vm.Slot0, out float x) == false) return;
			
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
			if (ExpectValue.ExpectFloat(vm.Slot0, out float y) == false) return;

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
			if (ExpectValue.ExpectFloat(vm.Slot0, out float z) == false) return;

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

	public class ColorBinding : UnityModule.Class
	{
		public ColorBinding() : base(nameof(Color), null, typeof(Color))
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

	public class QuaternionBinding : UnityModule.Class
	{
		public QuaternionBinding() : base(nameof(Quaternion), null, typeof(Quaternion))
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

	// public class MeshBinding : UnityModule.Class
	// {
	// 	public MeshBinding() : base(nameof(Mesh), null, typeof(Mesh))
	// 	{
	// 		Add(new Method(Signature.Create(MethodType.Construct, nameof(New)), new ForeignMethod(New)));
	//
	// 		const string name = "name"; 
	// 		Add(new Method(Signature.Create(MethodType.FieldGetter, name), new ForeignMethod(GetName)));
	// 		Add(new Method(Signature.Create(MethodType.FieldSetter, name), new ForeignMethod(SetName)));
	// 		Add(new Method(Signature.Create(MethodType.ToString), new ForeignMethod(ToString)));
	// 	}
	//
	// 	public static bool Expect(Vm vm, Slot slot, out ForeignObject<Mesh> value)
	// 	{
	// 		return ExpectValue.ExpectForeign(vm, slot, out value);
	// 	}
	//
	// 	private static void New(Vm vm)
	// 	{
	// 		vm.EnsureSlots(1);
	// 		if (Expect(vm, vm.Slot0, out var self) == false) return;
	// 		
	// 		self.Value = new Mesh();
	// 	}
	//
	// 	private static void GetName(Vm vm)
	// 	{
	// 		vm.EnsureSlots(1);
	// 		if (Expect(vm, vm.Slot0, out var self) == false) return;
	//
	// 		vm.Slot0.SetString(self.Value.name);
	// 	}
	//
	// 	private static void SetName(Vm vm)
	// 	{
	// 		vm.EnsureSlots(1);
	// 		if (Expect(vm, vm.Slot0, out var self) == false) return;
	// 		if (ExpectValue.ExpectString(vm, vm.Slot0, out var name) == false) return;
	// 		
	// 		self.Value.name = name;
	// 	}
	//
	// 	private static void ToString(Vm vm)
	// 	{
	// 		vm.EnsureSlots(1);
	// 		if (Expect(vm, vm.Slot0, out var self) == false) return;
	// 		
	// 		vm.Slot0.SetString(self.Value.ToString());
	// 	}
	// }
	//
	// [TomiaClass(typeof(UnityModule), nameof(Material), typeof(Material))]
	// public class MaterialBinding : Class
	// {
	// 	[TomiaExpect(typeof(ForeignObject<Material>))]
	// 	public static bool Expect(Vm vm, Slot slot, out ForeignObject<Material> value)
	// 	{
	// 		return ExpectValue.ExpectForeign(vm, slot, out value);
	// 	}
	//
	// 	[TomiaMethod(MethodType.FieldGetter)]
	// 	private static void name(Vm vm, ForeignObject<Material> self)
	// 	{
	// 		vm.Slot0.SetString(self.Value.name);
	// 	}
	//
	// 	[TomiaMethod(MethodType.FieldSetter)]
	// 	private static void name(Vm vm, ForeignObject<Material> self, string name)
	// 	{
	// 		self.Value.name = name;
	// 	}
	//
	// 	[TomiaMethod(MethodType.ToString)]
	// 	private static void ToString(Vm vm, ForeignObject<Material> self)
	// 	{
	// 		vm.Slot0.SetString(self.Value.ToString());
	// 	}
	// 	
	// 	[TomiaMethod(MethodType.Method)]
	// 	private static void SetColor(Vm vm, ForeignObject<Material> self, int nameID, ForeignObject<Color> color)
	// 	{
	// 		self.Value.SetColor(nameID, color.Value);
	// 	}
	// 	
	// 	[TomiaMethod(MethodType.Method)]
	// 	private static void GetColor(Vm vm, ForeignObject<Material> self, int nameID)
	// 	{
	// 		if (UnityModule.ExpectId(vm, typeof(Color), out var type) == false) return;
	// 		var color = self.Value.GetColor(nameID);
	// 		UnityModule.SetNewForeign(vm, vm.Slot1, type, color);
	// 	}
	// }
	//
	// [TomiaClass(typeof(UnityModule), nameof(Shader), typeof(Shader))]
	// public class ShaderBinding : Class
	// {
	// 	[TomiaMethod(MethodType.StaticMethod)]
	// 	private static void PropertyToID(Vm vm, string name)
	// 	{
	// 		vm.Slot0.SetInt(Shader.PropertyToID(name));
	// 	}
	// }
}
