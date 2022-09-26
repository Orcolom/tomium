using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using Wrench.Native;

namespace Wrench
{
	internal interface IVmElement
	{
		internal IntPtr VmPtr { get; }
	}

	public delegate void AtomicAction(in Vm vm);

	public delegate InterpretResult AtomicInterpretAction(in Vm vm);

	public delegate void AtomicAction<TStruct>(ref TStruct data, in Vm vm) where TStruct : struct;
	
	public struct Vm : IDisposable, IEquatable<Vm>
	{
		internal static readonly SharedStatic<StaticMap<Vm>> Vms = SharedStatic<StaticMap<Vm>>.GetOrCreate<Vm>();

		private static readonly ProfilerMarker _prefInterpret = PrefHelper.Create($"{nameof(Vm)}.{nameof(Interpret)}");
		private static readonly ProfilerMarker _prefCall = PrefHelper.Create($"{nameof(Vm)}.{nameof(Call)}");
		
		static Vm()
		{
			Vms.Data.Init(16);
		}

		[NativeDisableUnsafePtrRestriction]
		private IntPtr _ptr;

		public readonly Slot Slot0;
		public readonly Slot Slot1;
		public readonly Slot Slot2;
		public readonly Slot Slot3;
		public readonly Slot Slot4;
		public readonly Slot Slot5;
		public readonly Slot Slot6;
		public readonly Slot Slot7;
		public readonly Slot Slot8;
		public readonly Slot Slot9;
		public readonly Slot Slot10;
		public readonly Slot Slot11;
		public readonly Slot Slot12;
		public readonly Slot Slot13;
		public readonly Slot Slot14;
		public readonly Slot Slot15;
		public readonly Slot Slot16;

		public bool IsValid => FromPtr(_ptr)._ptr != IntPtr.Zero;
		internal IntPtr Ptr => _ptr;

		#region Lifetime

		public static Vm New(Config? config = null)
		{
			// set config
			var configuration = Config.ToInterop(config ?? Config.Default);
			configuration.NativeWrite = global::Wrench.Wrench.WriteCallback;
			configuration.NativeError = global::Wrench.Wrench.ErrorCallback;
			configuration.NativeResolveModule = global::Wrench.Wrench.ResolveCallback;
			configuration.NativeLoadModule = global::Wrench.Wrench.LoadCallback;
			configuration.NativeBindForeignMethod = global::Wrench.Wrench.BindForeignMethodCallback;

			// get vm ptr
			IntPtr ptr = Interop.wrenNewVM(configuration);
			Vm vm = new Vm(ptr);

			// store data
			Vms.Data.Map.Add(ptr, vm);
			Managed.ManagedClasses.Add(ptr, new Managed()); // store *managed* events separately 

			return vm;
		}

		internal static Vm FromPtr(IntPtr ptr)
		{
			return Vms.Data.Map.TryGetValue(ptr, out var vm) ? vm : new Vm();
		}

		internal static void GetData(IntPtr ptr, out Vm vm, out Managed managed)
		{
			vm = Vm.FromPtr(ptr);
			managed = Managed.ManagedClasses[ptr];
		}

		private Vm(IntPtr ptr)
		{
			_ptr = ptr;
			Slot0 = new Slot(_ptr, 0);
			Slot1 = new Slot(_ptr, 1);
			Slot2 = new Slot(_ptr, 2);
			Slot3 = new Slot(_ptr, 3);
			Slot4 = new Slot(_ptr, 4);
			Slot5 = new Slot(_ptr, 5);
			Slot6 = new Slot(_ptr, 6);
			Slot7 = new Slot(_ptr, 7);
			Slot8 = new Slot(_ptr, 8);
			Slot9 = new Slot(_ptr, 9);
			Slot10 = new Slot(_ptr, 10);
			Slot11 = new Slot(_ptr, 11);
			Slot12 = new Slot(_ptr, 12);
			Slot13 = new Slot(_ptr, 13);
			Slot14 = new Slot(_ptr, 14);
			Slot15 = new Slot(_ptr, 15);
			Slot16 = new Slot(_ptr, 16);
		}

		public void Dispose()
		{
			if (IsValid == false) return;

			Interop.wrenFreeVM(_ptr);
			Managed.ManagedClasses.Remove(_ptr);
			Vms.Data.Map.Remove(_ptr);
			_ptr = IntPtr.Zero;
		}

		#endregion

		#region Atomic Access

		public void AtomicAccess<TStruct>(ref TStruct data, AtomicAction<TStruct> action)
			where TStruct : struct
		{
			if (IfInvalid(this)) return;

			lock (Managed.ManagedClasses[_ptr])
			{
				action.Invoke(ref data, this);
			}
		}

		public void AtomicAccess(AtomicAction action)
		{
			if (IfInvalid(this)) return;

			lock (Managed.ManagedClasses[_ptr])
			{
				action.Invoke(this);
			}
		}

		public InterpretResult AtomicAccess(AtomicInterpretAction action)
		{
			if (IfInvalid(this)) return InterpretResult.CompileError;
			
			lock (Managed.ManagedClasses[_ptr])
			{
				return action.Invoke(this);
			}
		}

		#endregion
		
		#region Managed

		public readonly void SetWriteListener(WriteDelegate @delegate)
		{
			if (IfInvalid(this)) return;
			Managed.ManagedClasses[_ptr].WriteEvent = @delegate;
		}

		public readonly void SetErrorListener(ErrorDelegate @delegate)
		{
			if (IfInvalid(this)) return;
			Managed.ManagedClasses[_ptr].ErrorEvent = @delegate;
		}

		public readonly void SetResolveModuleListener(ResolveModuleDelegate @delegate)
		{
			if (IfInvalid(this)) return;
			Managed.ManagedClasses[_ptr].ResolveModuleEvent = @delegate;
		}

		public readonly void SetLoadModuleListener(LoadModuleDelegate @delegate)
		{
			if (IfInvalid(this)) return;
			Managed.ManagedClasses[_ptr].LoadModuleEvent = @delegate;
		}

		public void SetBindForeignMethodListener(BindForeignMethodDelegate @delegate)
		{
			if (IfInvalid(this)) return;
			Managed.ManagedClasses[_ptr].BindForeignMethodEvent = @delegate;
		}
		
		public void SetBindForeignClassListener(BindForeignClassDelegate @delegate)
		{
			if (IfInvalid(this)) return;
			Managed.ManagedClasses[_ptr].BindForeignClassEvent = @delegate;
		}

		public void SetUserData<T>(T obj)
		{
			if (IfInvalid(this)) return;
			Managed.ManagedClasses[_ptr].UserData = obj;
		}

		public T GetUserData<T>()
		{
			if (IfInvalid(this)) return default;
			return (T) Managed.ManagedClasses[_ptr].UserData;
		}

		#endregion

		#region Methods

		public readonly InterpretResult Interpret([DisallowNull] string module, string source)
		{
			_prefInterpret.Begin();
			
			if (IfInvalid(this)) return InterpretResult.CompileError;
			if (string.IsNullOrEmpty(module)) throw new ArgumentNullException();
			var result = Interop.wrenInterpret(_ptr, module, source);
			
			_prefInterpret.End();
			
			return result;
		}

		public readonly InterpretResult Call(Handle handle)
		{
			_prefCall.Begin();
			
			if (IfInvalid(this)) return InterpretResult.CompileError;
			if (Handle.IfInvalid(handle)) return InterpretResult.CompileError;
			if (IfNotSameVm(this, handle)) return InterpretResult.CompileError;
			var result = Interop.wrenCall(_ptr, handle.Ptr);

			_prefCall.End();

			return result;
		}

		public readonly void Gc()
		{
			if (IfInvalid(this)) return;
			Interop.wrenCollectGarbage(_ptr);
		}

		public readonly void Abort(in Slot msg)
		{
			if (IfInvalid(this)) return;
			Interop.wrenAbortFiber(_ptr, msg.Index);
		}

		public readonly Handle MakeCallHandle(string signature)
		{
			return Handle.New(this, signature);
		}

		public bool HasModuleAndVariable(string module, string variable)
		{
			if (HasModule(module) == false) return false;
			if (HasVariable(module, variable) == false) return false;
			return true;
		}

		/// <summary>
		/// Looks up the top level variable with <paramref name="name"/> in resolved <paramref name="module"/>, 
		/// returns false if not found. The module must be imported at the time, 
		/// use <see cref="HasModule"/>  to ensure that before calling.
		/// </summary>
		public bool HasVariable(string module, string name)
		{
			return Interop.wrenHasVariable(_ptr, module, name);
		}

		/// <summary>
		/// Returns true if <paramref name="module"/> has been imported/resolved before, false if not.
		/// </summary>
		public bool HasModule(string module)
		{
			return Interop.wrenHasModule(_ptr, module);
		}

		#endregion

		#region Slots

		public int SlotCount
		{
			get
			{
				if (IfInvalid(this)) return 0;
				return Interop.wrenGetSlotCount(_ptr);
			}
		}


		public readonly void EnsureSlots(int size)
		{
			if (IfInvalid(this)) return;
			Interop.wrenEnsureSlots(_ptr, size);
		}

		#endregion

		#region Validate
		
		internal static bool IfInvalid(IntPtr vmPtr)
		{
			Vm vm = FromPtr(vmPtr);
			return IfInvalid(vm);
		}

		internal static bool IfInvalid(in Vm vm)
		{
			if (vm.IsValid) return false;
			
			Expect.ThrowException(new ObjectDisposedException(nameof(Vm), "Vm is already disposed"));
			return true;
		}

		internal static bool IfNotSameVm(in Vm vm, in IVmElement element, params IVmElement[] elements)
		{
			return IfNotSameVm(vm._ptr, element, elements);
		}

		internal static bool IfNotSameVm(in IVmElement element, params IVmElement[] elements)
		{
			return IfNotSameVm(element.VmPtr, element, elements);
		}

		private static bool IfNotSameVm(IntPtr ptr, IVmElement element, params IVmElement[] elements)
		{
			bool sameVm = ptr == element.VmPtr;

			if (sameVm)
			{
				for (int i = 0; i < elements.Length; i++)
				{
					if (ptr == elements[i].VmPtr) continue;

					sameVm = false;
					break;
				}
			}

			if (sameVm) return false;
			
			Expect.ThrowException(new ArgumentOutOfRangeException(nameof(sameVm), "Not all elements are from the same Vm"));
			return true;
		}

		#endregion

		#region Equality

		public bool Equals(Vm other)
		{
			return _ptr.Equals(other._ptr);
		}

		public override bool Equals(object obj)
		{
			return obj is Vm other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _ptr.GetHashCode();
		}

		public static bool operator ==(Vm left, Vm right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Vm left, Vm right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}
