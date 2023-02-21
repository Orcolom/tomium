using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Profiling;
using UnityEngine;

namespace Tomia
{
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ForeignAction(Vm vm);


	internal static class ForeignMethodStatics
	{
		static ForeignMethodStatics()
		{
			Methods.Data.Init(16);
		}
		
		internal static readonly SharedStatic<StaticMap<ForeignMethod>> Methods = SharedStatic<StaticMap<ForeignMethod>>.GetOrCreate<ForeignMethod>();
	}

	public struct ForeignMethod
	{
		
		private ProfilerMarker _profilerMarker;

		private IntPtr _ptr;
		internal IntPtr Ptr => _ptr;
		public bool IsValid => _ptr != IntPtr.Zero;

		#region Lifetime

		public ForeignMethod(ForeignAction action, string marker = null)
		{
#if UNITY_EDITOR || DEBUG
			// TO//DO: only do this if il2cpp backend is selected
			// if (action.Method.IsStatic == false) Debug.LogError($"il2cpp only allows static methods. {action.Method.Name} is not");
#endif

			Debug.Log($"x {action.Method.Name}, {action.Method.MethodHandle.Value}");
			_ptr = action.Method.MethodHandle.Value;
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
			var action = Managed.Actions[_ptr];
			action.Invoke(vm);
		}
	}
}
