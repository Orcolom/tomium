using System;
using System.Text;
using NUnit.Framework;
using Tomia;
using ValueType = Tomia.ValueType;

namespace Tests
{
	public class TestsSlot
	{
		private Vm _vm;

		[SetUp]
		public void Vm_Setup()
		{
			_vm = Vm.New();
			_vm.Interpret("m", @"
var Value = null
");
			_vm.EnsureSlots(2);
			_vm.Slot0.GetVariable("m", "Value");
		}

		private void GetSetTest<T>(in Slot slot, ValueType type, T value, Func<Slot, T> get, Action<Slot, T> set)
		{
			Assert.AreEqual(_vm.Slot0.GetValueType(), ValueType.Null);
			set.Invoke(slot, value);
			Assert.AreEqual(_vm.Slot0.GetValueType(), type);
			var result = get.Invoke(slot);
			Assert.AreEqual(value, result);
		}

		[Test]
		public void Slot_Bool()
		{
			GetSetTest(_vm.Slot0,
				ValueType.Bool, true,
				slot => slot.GetBool(),
				(slot, b) => slot.SetBool(b));
		}

		[Test]
		public void Slot_Double()
		{
			GetSetTest(_vm.Slot0,
				ValueType.Number, 12.3,
				slot => slot.GetDouble(),
				(slot, b) => slot.SetDouble(b));
		}

		[Test]
		public void Slot_Float()
		{
			GetSetTest(_vm.Slot0,
				ValueType.Number, 12.3f,
				slot => slot.GetFloat(),
				(slot, b) => slot.SetFloat(b));
		}

		[Test]
		public void Slot_Int()
		{
			GetSetTest(_vm.Slot0,
				ValueType.Number, 12,
				slot => slot.GetInt(),
				(slot, b) => slot.SetInt(b));
		}

		[Test]
		public void Slot_String()
		{
			GetSetTest(_vm.Slot0,
				ValueType.String, "hello world",
				slot => slot.GetString(),
				(slot, b) => slot.SetString(b));
		}

		[Test]
		public void Slot_Bytes()
		{
			GetSetTest(_vm.Slot0,
				ValueType.String, Encoding.ASCII.GetBytes("hello world"),
				slot => slot.GetBytes(),
				(slot, b) => slot.SetBytes(b));
		}

		[Test]
		public void Slot_Null()
		{
			Assert.AreEqual(_vm.Slot0.GetValueType(), ValueType.Null);
		}
	}
}
