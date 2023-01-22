using UnityEngine;
using Wrench;
using Wrench.Builder;
using Unity.Mathematics;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

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
		private void x(Vm vm, ForeignObject<Vector3> self) => vm.Slot0.SetFloat(self.Value.x);

		[WrenchMethod(MethodType.FieldSetter)]
		private void x(Vm vm, ForeignObject<Vector3> self, float x)
		{
			var selfValue = self.Value;
			selfValue.x = x;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void y(Vm vm, ForeignObject<Vector3> self) => vm.Slot0.SetFloat(self.Value.y);

		[WrenchMethod(MethodType.FieldSetter)]
		private void y(Vm vm, ForeignObject<Vector3> self, float y)
		{
			var selfValue = self.Value;
			selfValue.y = y;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void z(Vm vm, ForeignObject<Vector3> self) => vm.Slot0.SetFloat(self.Value.z);

		[WrenchMethod(MethodType.FieldSetter)]
		private void z(Vm vm, ForeignObject<Vector3> self, float z)
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

	[WrenchClass(typeof(UnityModule), nameof(Color), typeof(Color))]
	public class ColorBinding : Class
	{
		[WrenchExpect(typeof(ForeignObject<Color>), true)]
		public static bool Expect(Vm vm, Slot slot, out ForeignObject<Color> value)
		{
			return ExpectValue.ExpectForeign(vm, slot, out value);
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void r(Vm vm, ForeignObject<Color> self) => vm.Slot0.SetFloat(self.Value.r);

		[WrenchMethod(MethodType.FieldSetter)]
		private void r(Vm vm, ForeignObject<Color> self, float r)
		{
			var selfValue = self.Value;
			selfValue.r = r;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void g(Vm vm, ForeignObject<Color> self) => vm.Slot0.SetFloat(self.Value.g);

		[WrenchMethod(MethodType.FieldSetter)]
		private void g(Vm vm, ForeignObject<Color> self, float g)
		{
			var selfValue = self.Value;
			selfValue.g = g;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void b(Vm vm, ForeignObject<Color> self) => vm.Slot0.SetFloat(self.Value.b);

		[WrenchMethod(MethodType.FieldSetter)]
		private void b(Vm vm, ForeignObject<Color> self, float b)
		{
			var selfValue = self.Value;
			selfValue.b = b;
			self.Value = selfValue;
		}
		
		[WrenchMethod(MethodType.FieldGetter)]
		private void a(Vm vm, ForeignObject<Color> self) => vm.Slot0.SetFloat(self.Value.a);

		[WrenchMethod(MethodType.FieldSetter)]
		private void a(Vm vm, ForeignObject<Color> self, float a)
		{
			var selfValue = self.Value;
			selfValue.a = a;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.ToString)]
		private void ToString(Vm vm, ForeignObject<Color> self)
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
		private void x(Vm vm, ForeignObject<Quaternion> self) => vm.Slot0.SetFloat(self.Value.x);

		[WrenchMethod(MethodType.FieldSetter)]
		private void x(Vm vm, ForeignObject<Quaternion> self, float x)
		{
			var selfValue = self.Value;
			selfValue.x = x;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void y(Vm vm, ForeignObject<Quaternion> self) => vm.Slot0.SetFloat(self.Value.y);

		[WrenchMethod(MethodType.FieldSetter)]
		private void y(Vm vm, ForeignObject<Quaternion> self, float y)
		{
			var selfValue = self.Value;
			selfValue.y = y;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void z(Vm vm, ForeignObject<Quaternion> self) => vm.Slot0.SetFloat(self.Value.z);

		[WrenchMethod(MethodType.FieldSetter)]
		private void z(Vm vm, ForeignObject<Quaternion> self, float z)
		{
			var selfValue = self.Value;
			selfValue.z = z;
			self.Value = selfValue;
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private void w(Vm vm, ForeignObject<Quaternion> self) => vm.Slot0.SetFloat(self.Value.w);

		[WrenchMethod(MethodType.FieldSetter)]
		private void w(Vm vm, ForeignObject<Quaternion> self, float w)
		{
			var selfValue = self.Value;
			selfValue.w = w;
			self.Value = selfValue;
		}
	}

	[WrenchClass(typeof(UnityModule), nameof(Mesh), typeof(Mesh))]
	public class MeshBinding : Class
	{
		[WrenchExpect(typeof(ForeignObject<Mesh>))]
		public static bool Expect(Vm vm, Slot slot, out ForeignObject<Mesh> value)
		{
			return ExpectValue.ExpectForeign(vm, slot, out value);
		}

		[WrenchMethod(MethodType.Construct)]
		private static void New(Vm vm, ForeignObject<Mesh> self)
		{
			self.Value = new Mesh();
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private static void name(Vm vm, ForeignObject<Mesh> self)
		{
			vm.Slot0.SetString(self.Value.name);
		}

		[WrenchMethod(MethodType.FieldSetter)]
		private static void name(Vm vm, ForeignObject<Mesh> self, string name)
		{
			self.Value.name = name;
		}

		[WrenchMethod(MethodType.ToString)]
		private static void ToString(Vm vm, ForeignObject<Mesh> self)
		{
			vm.Slot0.SetString(self.Value.ToString());
		}
	}

	[WrenchClass(typeof(UnityModule), nameof(Material), typeof(Material))]
	public class MaterialBinding : Class
	{
		[WrenchExpect(typeof(ForeignObject<Material>))]
		public static bool Expect(Vm vm, Slot slot, out ForeignObject<Material> value)
		{
			return ExpectValue.ExpectForeign(vm, slot, out value);
		}

		[WrenchMethod(MethodType.FieldGetter)]
		private static void name(Vm vm, ForeignObject<Material> self)
		{
			vm.Slot0.SetString(self.Value.name);
		}

		[WrenchMethod(MethodType.FieldSetter)]
		private static void name(Vm vm, ForeignObject<Material> self, string name)
		{
			self.Value.name = name;
		}

		[WrenchMethod(MethodType.ToString)]
		private static void ToString(Vm vm, ForeignObject<Material> self)
		{
			vm.Slot0.SetString(self.Value.ToString());
		}
		
		[WrenchMethod(MethodType.Method)]
		private static void SetColor(Vm vm, ForeignObject<Material> self, int nameID, ForeignObject<Color> color)
		{
			self.Value.SetColor(nameID, color.Value);
		}
		
		[WrenchMethod(MethodType.Method)]
		private static void GetColor(Vm vm, ForeignObject<Material> self, int nameID)
		{
			if (UnityModule.ExpectId(vm, typeof(Color), out var type) == false) return;
			var color = self.Value.GetColor(nameID);
			UnityModule.SetNewForeign(vm, vm.Slot1, type, color);
		}
	}

	[WrenchClass(typeof(UnityModule), nameof(Shader), typeof(Shader))]
	public class ShaderBinding : Class
	{
		[WrenchMethod(MethodType.StaticMethod)]
		private static void PropertyToID(Vm vm, string name)
		{
			vm.Slot0.SetInt(Shader.PropertyToID(name));
		}
	}
}
