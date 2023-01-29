using System;
using System.Reflection;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using UnityEngine;

namespace Tomia
{
	internal static class ForeignClassStatics
	{
		static ForeignClassStatics()
		{
			// needs to happen here, il2pcc will strip the static constructor from ForeignClass. (guess: cause it doesn't have any statics itself?)
			// these statics are separate because mono.cecil weaver doesn't like to weave classes that have unity collections objects as statics 
			Classes.Data.Init(16);
			FinToAlloc.Data.Init(16);
		}
		
		internal static readonly SharedStatic<StaticMap<ForeignClass>> Classes =
			SharedStatic<StaticMap<ForeignClass>>.GetOrCreate<ForeignClass>();

		internal static readonly SharedStatic<StaticMap<IntPtr>> FinToAlloc =
			SharedStatic<StaticMap<IntPtr>>.GetOrCreate<ForeignClass, IntPtr>();
	}

	public struct ForeignClass
	{
		private IntPtr _allocPtr;
		private IntPtr _finPtr;

		internal IntPtr AllocPtr => _allocPtr;

		public bool IsValid => _allocPtr != IntPtr.Zero || _finPtr != IntPtr.Zero;

		#region Lifetime

		public ForeignClass(ForeignAction alloc)
		{
#if UNITY_EDITOR
			// TODO: only do this if il2cpp backend is selected
			if (alloc.Method.IsStatic == false) Debug.LogWarning("Alloc methods have to be static for il2cpp");
#endif
			
			_allocPtr = new IntPtr(alloc.Method.MetadataToken);
			_finPtr = IntPtr.Zero;
			using (ProfilerUtils.AllocScope.Auto())
			{
				Managed.Actions.TryAdd(_allocPtr, alloc);
				ForeignClassStatics.Classes.Data.Map.TryAdd(_allocPtr, this);
			}
		}

		public ForeignClass(ForeignAction alloc, ForeignAction fin)
		{
			_allocPtr = new IntPtr(alloc.Method.MetadataToken);
			_finPtr = new IntPtr(fin.Method.MetadataToken);

			using (ProfilerUtils.AllocScope.Auto())
			{
				Managed.Actions.TryAdd(_allocPtr, alloc);
				ForeignClassStatics.Classes.Data.Map.TryAdd(_allocPtr, this);
				ForeignClassStatics.FinToAlloc.Data.Map.TryAdd(_allocPtr, _finPtr);
			}
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
				var action = Managed.Actions[_allocPtr];
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
