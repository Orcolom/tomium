﻿using System;
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
			_ptr = Marshal.GetFunctionPointerForDelegate(action);
			_profilerMarker = ProfilerUtils.Create(marker ?? action.Method.Name);
			
			Managed.Actions.TryAdd(_ptr, action);
			ForeignMethodStatics.Methods.Data.Map.TryAdd(_ptr, this);
			action.Method.MethodHandle.GetFunctionPointer(); // force compile method
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
