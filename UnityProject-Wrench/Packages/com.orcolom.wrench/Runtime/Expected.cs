using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

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

		public static bool Type(in Vm vm, in Slot slot, ValueType type)
		{
			if (slot.Type == type) return false;
			
			AbortException(vm, "invalid-type");
			return true;
		}
		
		public static bool Type(in Vm vm, in Slot slot, ValueType typeA, ValueType typeB)
		{
			if (slot.Type == typeA || slot.Type == typeB) return false;
			
			AbortException(vm, "invalid-type");
			return true;
		}
		
		public static bool Int(in Vm vm, in Slot slot, out int value)
		{
			value = 0;
			if (Expected.Type(vm, slot, ValueType.Number)) return false;

			value = slot.GetInt();
			return true;
		}

		public static bool Double(in Vm vm, in Slot slot, out double value)
		{
			value = 0;
			if (Expected.Type(vm, slot, ValueType.Number)) return false;

			value = slot.GetDouble();
			return true;
		}
		
		public static bool Float(in Vm vm, in Slot slot, out float value)
		{
			value = 0;
			if (Expected.Type(vm, slot, ValueType.Number)) return false;

			value = slot.GetFloat();
			return true;
		}
				
		public static bool String(in Vm vm, in Slot slot, out string value)
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

		public static bool UnManagedForeignType<TType>(in Vm vm, in Slot foreignSlot, out UnManagedForeignObject<TType> unManagedForeign) 
			where TType : unmanaged
		{
			unManagedForeign = default;
			
			var iForeign = foreignSlot.GetForeign();
			if (iForeign.IsValueOfUnManagedType<TType>())
			{
				unManagedForeign = iForeign.AsUnManaged<TType>();
				return false;
			}
			
			AbortException(vm, "invalid-foreign-type");
			return true;
		}
		
		
		public static bool ManagedForeignType<TType>(in Vm vm, in Slot foreignSlot, out ManagedForeignObject<TType> managedForeign) 
		{
			managedForeign = default;
			
			var iForeign = foreignSlot.GetForeign();
			if (iForeign.IsValueOfManagedType<TType>())
			{
				managedForeign = iForeign.AsManaged<TType>();
				return false;
			}
			
			AbortException(vm, "invalid-foreign-type");
			return true;
		}

		#endregion

	}
}
