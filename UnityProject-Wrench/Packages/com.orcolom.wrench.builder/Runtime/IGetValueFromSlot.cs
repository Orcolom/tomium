using System.Diagnostics.CodeAnalysis;

namespace Wrench.Builder
{
	// public interface IGetValueFromSlot<T>
	// {
	// 	public bool GetValue(in Vm vm, in ISlotManaged slot, out ForeignObject<T> value);
	// }
	//
	// public interface IGetUnmanagedValueFromSlot<T> where T : unmanaged
	// {
	// 	public bool GetValue(in Vm vm, in ISlotUnmanaged slot, out UnmanagedForeignObject<T> value);
	// }

	public interface IInternalGetUnmanagedValueFromSlot<T>
	{
		public bool GetValue(in Vm vm, in ISlotUnmanaged slot, out T value);
	}

	public class GetValueUtils :
		IInternalGetUnmanagedValueFromSlot<int>,
		IInternalGetUnmanagedValueFromSlot<float>,
		IInternalGetUnmanagedValueFromSlot<double>,
		IInternalGetUnmanagedValueFromSlot<string>,
		IInternalGetUnmanagedValueFromSlot<byte[]>,
		IInternalGetUnmanagedValueFromSlot<bool>
	{
		public static void AbortException(in Vm vm, string msg)
		{
			vm.Slot0.SetString(msg);
			vm.Abort(vm.Slot0);
		}

		public static bool IsOfValueType(in Vm vm, in ISlotUnmanaged slot, ValueType type, bool canBeNull = false)
		{
			var valueType = slot.GetValueType();
			bool isNull = valueType == ValueType.Null;
			if (slot.GetValueType() == type || (isNull && canBeNull)) return true;
			
			AbortException(vm, "invalid-type");
			return false;
		}

		public bool GetValue(in Vm vm, in ISlotUnmanaged slot, out int value)
		{
			value = 0;
			if (IsOfValueType(vm, slot, ValueType.Number) == false) return false;

			value = slot.GetInt();
			return true;
		}

		public bool GetValue(in Vm vm, in ISlotUnmanaged slot, out float value)
		{
			value = 0;
			if (IsOfValueType(vm, slot, ValueType.Number) == false) return false;

			value = slot.GetFloat();
			return true;
		}

		public bool GetValue(in Vm vm, in ISlotUnmanaged slot, out double value)
		{
			value = 0;
			if (IsOfValueType(vm, slot, ValueType.Number) == false) return false;

			value = slot.GetDouble();
			return true;
		}

		public bool GetValue(in Vm vm, in ISlotUnmanaged slot, out string value)
		{
			value = null;
			if (IsOfValueType(vm, slot, ValueType.String, true) == false) return false;

			value = slot.GetString();
			return true;
		}

		public bool GetValue(in Vm vm, in ISlotUnmanaged slot, out byte[] value)
		{
			value = null;
			if (IsOfValueType(vm, slot, ValueType.String, true) == false) return false;

			value = slot.GetBytes();
			return true;
		}

		public bool GetValue(in Vm vm, in ISlotUnmanaged slot, out bool value)
		{
			value = false;
			if (IsOfValueType(vm, slot, ValueType.Bool) == false) return false;

			value = slot.GetBool();
			return true;
		}
	}
}
