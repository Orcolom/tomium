namespace Wrench.Builder
{
	public class ExpectValue
	{
		public static void AbortException(in Vm vm, string msg)
		{
			vm.Slot0.SetString(msg);
			vm.Abort(vm.Slot0);
		}

		public static bool IsOfValueType(in Vm vm, in Slot slot, ValueType type, bool canBeNull = false)
		{
			var valueType = slot.GetValueType();
			bool isNull = valueType == ValueType.Null;
			if (slot.GetValueType() == type || (isNull && canBeNull)) return true;

			AbortException(vm, "invalid-type");
			return false;
		}

		[WrenchExpect(typeof(int))]
		public static bool ExpectInt(in Vm vm, in Slot slot, out int value)
		{
			value = 0;
			if (IsOfValueType(vm, slot, ValueType.Number) == false) return false;

			value = slot.GetInt();
			return true;
		}

		[WrenchExpect(typeof(float))]
		public static bool ExpectFloat(in Vm vm, in Slot slot, out float value)
		{
			value = 0;
			if (IsOfValueType(vm, slot, ValueType.Number) == false) return false;

			value = slot.GetFloat();
			return true;
		}

		[WrenchExpect(typeof(double))]
		public static bool ExpectDouble(in Vm vm, in Slot slot, out double value)
		{
			value = 0;
			if (IsOfValueType(vm, slot, ValueType.Number) == false) return false;

			value = slot.GetDouble();
			return true;
		}

		[WrenchExpect(typeof(string))]
		public static bool ExpectString(in Vm vm, in Slot slot, out string value)
		{
			value = null;
			if (IsOfValueType(vm, slot, ValueType.String, true) == false) return false;

			value = slot.GetString();
			return true;
		}

		[WrenchExpect(typeof(byte[]))]
		public static bool ExpectByteArray(in Vm vm, in Slot slot, out byte[] value)
		{
			value = null;
			if (IsOfValueType(vm, slot, ValueType.String, true) == false) return false;

			value = slot.GetBytes();
			return true;
		}

		[WrenchExpect(typeof(bool))]
		public static bool ExpectBool(in Vm vm, in Slot slot, out bool value)
		{
			value = false;
			if (IsOfValueType(vm, slot, ValueType.Bool) == false) return false;

			value = slot.GetBool();
			return true;
		}
	}
}
