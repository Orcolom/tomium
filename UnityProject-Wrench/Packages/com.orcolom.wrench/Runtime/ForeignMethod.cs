using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Profiling;
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
		
		private ProfilerMarker _profilerMarker;

		private IntPtr _ptr;
		internal IntPtr Ptr => _ptr;
		public bool IsValid => _ptr != IntPtr.Zero;

		#region Lifetime

		public ForeignMethod(ForeignAction action, string marker = null)
		{
			try
			{
				// While this also returns a pointer, this isn't the same pointer as Marshal.GetFunctionPointerForDelegate
				// and will return invalid methods when trying to use it.
				// Marshalling can just crash the editor when getting an invalid method, GetFunctionPointer throws an error instead. it also compiles the method, aka first call initialization
				var ptr = action.Method.MethodHandle.GetFunctionPointer();
			}
			catch
			{
				_profilerMarker = default;
				_ptr = default;
				return;
			}

			_ptr = Marshal.GetFunctionPointerForDelegate(action);
			
			_profilerMarker = ProfilerUtils.Create(marker ?? action.Method.Name);
			
			Managed.Actions.TryAdd(_ptr, action);
			ForeignMethodStatics.Methods.Data.Map.TryAdd(_ptr, this);
		}

		internal static ForeignMethod FromPtr(IntPtr ptr)
		{
			return ForeignMethodStatics.Methods.Data.Map.TryGetValue(ptr, out var method) ? method : new ForeignMethod();
		}

		#endregion

		public void Invoke(Vm vm)
		{
			using var scope = _profilerMarker.Auto();
			var action = Marshal.GetDelegateForFunctionPointer<ForeignAction>(_ptr);
			action.Invoke(vm);
		}
	}
}
