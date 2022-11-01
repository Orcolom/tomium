using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using UnityEngine;

namespace Wrench
{
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ForeignAction(Vm vm);


	internal static class ForeignMethodStatics
	{
		internal static readonly SharedStatic<StaticMap<ForeignMethod>> Methods = SharedStatic<StaticMap<ForeignMethod>>.GetOrCreate<ForeignMethod>();
	}
	

	public struct ForeignMethod
	{

		static ForeignMethod()
		{
			ForeignMethodStatics.Methods.Data.Init(16);
		}
		
		private IntPtr _ptr;
		internal IntPtr Ptr => _ptr;
		public bool IsValid => _ptr != IntPtr.Zero;

		#region Lifetime

		public ForeignMethod(ForeignAction action)
		{
			_ptr = Marshal.GetFunctionPointerForDelegate(action);
			Managed.Actions.TryAdd(_ptr, action);
			ForeignMethodStatics.Methods.Data.Map.TryAdd(_ptr, this);
		}

		internal static ForeignMethod FromPtr(IntPtr ptr)
		{
			return ForeignMethodStatics.Methods.Data.Map.TryGetValue(ptr, out var method) ? method : new ForeignMethod();
		}

		#endregion

		public void Invoke(in Vm vm)
		{
			var action = Marshal.GetDelegateForFunctionPointer<ForeignAction>(_ptr);
			action.Invoke(vm);
		}
	}
}
