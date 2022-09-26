using System;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Wrench
{
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ForeignAction(in Vm vm);

	public struct ForeignMethod
	{
		internal static readonly SharedStatic<StaticMap<ForeignMethod>> Methods = SharedStatic<StaticMap<ForeignMethod>>.GetOrCreate<ForeignMethod>();

		static ForeignMethod()
		{
			Methods.Data.Init(16);
		}
		
		private IntPtr _ptr;
		internal IntPtr Ptr => _ptr;
		public bool IsValid => _ptr != IntPtr.Zero;

		#region Lifetime

		public ForeignMethod(ForeignAction action)
		{
			_ptr = Marshal.GetFunctionPointerForDelegate(action);
			Managed.Actions.TryAdd(_ptr, action);
			Methods.Data.Map.TryAdd(_ptr, this);
		}

		internal static ForeignMethod FromPtr(IntPtr ptr)
		{
			return Methods.Data.Map.TryGetValue(ptr, out var method) ? method : new ForeignMethod();
		}

		#endregion

		public void Invoke(in Vm vm)
		{
			try
			{
				var action = Marshal.GetDelegateForFunctionPointer<ForeignAction>(_ptr);
				action.Invoke(vm);
			}
			catch (TypeAccessException)
			{
				// #if DEBUG
				// vm.AbortFiber(e.Message);
				// #else
				// vm.AbortFiber("invalid type");
				// #endif
			}
			catch (Exception)
			{
				// vm.AbortFiber($"<native> {e.Message}");
			}
		}
	}
}
