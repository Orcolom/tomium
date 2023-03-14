namespace Tomium.Builder
{
	public class ExpectValue
	{
		public static void AbortException(Vm vm, string msg)
		{
			vm.Slot0.SetString(msg);
			vm.Abort(vm.Slot0);
		}

		public static bool IsOfValueType(Slot slot, ValueType type, bool canBeNull = false)
		{
			var valueType = slot.GetValueType();
			bool isNull = valueType == ValueType.Null;
			if (slot.GetValueType() == type || (isNull && canBeNull)) return true;

			AbortException(new Vm(slot.VmPtr), $"slot {slot.Index}: invalid-type");
			return false;
		}

		public static bool ExpectForeign<T>(Slot slot, out ForeignObject<T> value, bool canBeNull = false)
		{
			value = default;
			if (IsOfValueType(slot, ValueType.Foreign, canBeNull) == false) return false;
			var ptr = slot.GetForeignPtr();
			if (Managed.ForeignObjects.TryGetValue(ptr, out var obj) == false) return false;
			//TODO: is it safe to assume null here??
			if (obj != null && obj is not T) return false;
			value = new ForeignObject<T>(ptr);
			return true;
		}

		public static bool ExpectInt(Vm vm, Slot slot, out int value)
		{
			value = 0;
			if (IsOfValueType(slot, ValueType.Number) == false) return false;

			value = slot.GetInt();
			return true;
		}

		public static bool ExpectFloat(Slot slot, out float value)
		{
			value = 0;
			if (IsOfValueType(slot, ValueType.Number) == false) return false;

			value = slot.GetFloat();
			return true;
		}

		public static bool ExpectDouble(Slot slot, out double value)
		{
			value = 0;
			if (IsOfValueType(slot, ValueType.Number) == false) return false;

			value = slot.GetDouble();
			return true;
		}

		public static bool ExpectString(Slot slot, out string value)
		{
			value = null;
			if (IsOfValueType(slot, ValueType.String, true) == false) return false;

			value = slot.GetString();
			return true;
		}

		public static bool ExpectByteArray(Slot slot, out byte[] value)
		{
			value = null;
			if (IsOfValueType(slot, ValueType.String, true) == false) return false;

			value = slot.GetBytes();
			return true;
		}

		public static bool ExpectBool(Slot slot, out bool value)
		{
			value = false;
			if (IsOfValueType(slot, ValueType.Bool) == false) return false;

			value = slot.GetBool();
			return true;
		}
		
		public static bool ExpectHandle(Slot slot, out Handle value)
		{
			value = slot.GetHandle();
			return true;
		}
	}
}
