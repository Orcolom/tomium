using System;
using System.Diagnostics;
using Wrench.Native;

namespace Wrench
{
	public class Expected
	{
		// static BooleanSwitch dataSwitch = new BooleanSwitch("Data", "DataAccess module");

		#region Internal

		// ReSharper disable Unity.PerformanceAnalysis
		[DebuggerHidden, DebuggerStepThrough]
		internal static void ThrowException<T>(T exception)
			where T:Exception
		{
			// // throw the exception -> this will fill its callstack
			// // then debug log -> this will log it and send it to unity dashboard
			// // don't actually throw so that the code doesn't deadlock
			//
			// try
			// {
				throw exception;
			// }
			// catch (Exception e)
			// {
			// 	Debug.LogException(e);
			// }
		}

		public static void AbortException(in Vm vm, string msg)
		{
			vm.Slot0.SetString(msg);
			vm.Abort(vm.Slot0);
		}

		#endregion

		#region Getters

		public static bool Type(in Vm vm, in ISlotUnmanaged slot, ValueType type)
		{
			if (slot.GetValueType() == type) return false;
			
			AbortException(vm, "invalid-type");
			return true;
		}
		
		public static bool Type(in Vm vm, in ISlotUnmanaged slot, ValueType typeA, ValueType typeB)
		{
			var type = slot.GetValueType();
			if (type == typeA || type == typeB) return false;
			
			AbortException(vm, "invalid-type");
			return true;
		}
		
		public static bool Int(in Vm vm, in ISlotUnmanaged slot, out int value)
		{
			value = 0;
			if (Expected.Type(vm, slot, ValueType.Number)) return false;

			value = slot.GetInt();
			return true;
		}

		public static bool Double(in Vm vm, in ISlotUnmanaged slot, out double value)
		{
			value = 0;
			if (Expected.Type(vm, slot, ValueType.Number)) return false;

			value = slot.GetDouble();
			return true;
		}
		
		public static bool Float(in Vm vm, in ISlotUnmanaged slot, out float value)
		{
			value = 0;
			if (Expected.Type(vm, slot, ValueType.Number)) return false;

			value = slot.GetFloat();
			return true;
		}
				
		public static bool String(in Vm vm, in ISlotUnmanaged slot, out string value)
		{
			value = null;
			if (Expected.Type(vm, slot, ValueType.String)) return false;

			value = slot.GetString();
			return true;
		}
		
		public static bool InArrayRange(in Vm vm, int value, int count)
		{
			if (value >= 0 || value < count) return false;

			AbortException(vm, "out-of-range");
			return true;
		}

		public static bool UnManagedForeignType<TType>(in Vm vm, in ISlotUnmanaged foreignSlot, out UnmanagedForeignObject<TType> unmanagedForeign) 
			where TType : unmanaged
		{
			unmanagedForeign = default;
			if (foreignSlot.ExpectedValid(ValueType.Foreign)) return false;
			
			var ptr = Interop.wrenGetSlotForeign(foreignSlot.VmPtr, foreignSlot.Index);
			
			if (ForeignObjectUtils.IsValueOfUnmanagedType<TType>(ptr))
			{
				unmanagedForeign = new UnmanagedForeignObject<TType>(ptr);
				return false;
			}
			
			AbortException(vm, "invalid-foreign-type");
			return true;
		}
		
		public static bool ForeignType<TType>(in Vm vm, in ISlotManaged foreignSlot, out ForeignObject<TType> foreign) 
		{
			foreign = default;
			if (foreignSlot.ExpectedValid(ValueType.Foreign)) return false;

			var ptr = Interop.wrenGetSlotForeign(foreignSlot.VmPtr, foreignSlot.Index);
			
			if (ForeignObjectUtils.IsValueOfManaged<TType>(ptr))
			{
				foreign = new ForeignObject<TType>(ptr);
				return false;
			}
			
			AbortException(vm, "invalid-foreign-type");
			return true;
		}

		#endregion

	}
}
