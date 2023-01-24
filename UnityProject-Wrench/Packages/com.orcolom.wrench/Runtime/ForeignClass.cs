using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using UnityEngine;

namespace Wrench
{
	internal static class ForeignClassStatics
	{
		internal static readonly SharedStatic<StaticMap<ForeignClass>> Classes =
			SharedStatic<StaticMap<ForeignClass>>.GetOrCreate<ForeignClass>();

		internal static readonly SharedStatic<StaticMap<IntPtr>> FinToAlloc =
			SharedStatic<StaticMap<IntPtr>>.GetOrCreate<ForeignClass, IntPtr>();
	}

	public struct ForeignClass
	{
		static ForeignClass()
		{
			ForeignClassStatics.Classes.Data.Init(16);
			ForeignClassStatics.FinToAlloc.Data.Init(16);
		}
		
		private IntPtr _allocPtr;
		private IntPtr _finPtr;

		internal IntPtr AllocPtr => _allocPtr;
		internal IntPtr FinPtr => _allocPtr;

		public bool IsValid => _allocPtr != IntPtr.Zero || _finPtr != IntPtr.Zero;

		#region Lifetime

		public ForeignClass(ForeignAction alloc)
		{
			_allocPtr = Marshal.GetFunctionPointerForDelegate(alloc);
			_finPtr = IntPtr.Zero;
			Managed.Actions.TryAdd(_allocPtr, alloc);
			ForeignClassStatics.Classes.Data.Map.TryAdd(_allocPtr, this);
		}
		
		public ForeignClass(ForeignAction alloc, ForeignAction fin)
		{
			_allocPtr = Marshal.GetFunctionPointerForDelegate(alloc);
			_finPtr = Marshal.GetFunctionPointerForDelegate(fin);
			Managed.Actions.TryAdd(_allocPtr, alloc);
			ForeignClassStatics.Classes.Data.Map.TryAdd(_allocPtr, this);
			ForeignClassStatics.FinToAlloc.Data.Map.TryAdd(_allocPtr, _finPtr);
		}

		// public static ForeignClass DefaultUnmanagedAlloc<T>()
		// 	where T: unmanaged
		// {
		// 	return new ForeignClass(vm => vm.Slot0.SetNewForeign<T>(vm.Slot0));
		// }

		public static ForeignClass DefaultAlloc()
		{
			return new ForeignClass(DefaultAllocAction);
		}

		[MonoPInvokeCallback(typeof(ForeignAction))]
		private static void DefaultAllocAction(Vm vm)
		{
			vm.Slot0.SetNewForeign(vm.Slot0);
		}
		
		internal static ForeignClass FromAllocPtr(IntPtr ptr)
		{
			return ForeignClassStatics.Classes.Data.Map.TryGetValue(ptr, out var @class) ? @class : new ForeignClass();
		}

		internal static ForeignClass FromFinPtr(IntPtr ptr)
		{
			return ForeignClassStatics.FinToAlloc.Data.Map.TryGetValue(ptr, out var alloc) ? FromAllocPtr(alloc) : new ForeignClass();
		}

		#endregion

		public void InvokeAllocator(in Vm vm)
		{
			try
			{
				var action = Marshal.GetDelegateForFunctionPointer<ForeignAction>(_allocPtr);
				action.Invoke(vm);
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}
		}

		public void InvokeFinalizer(IntPtr intPtr)
		{
			Managed.ForeignObjects.Remove(intPtr);
		}
	}
}
