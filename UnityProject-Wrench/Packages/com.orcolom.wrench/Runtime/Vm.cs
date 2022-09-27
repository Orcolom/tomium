using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using Wrench.Native;

namespace Wrench
{
	public interface IVmElement
	{
		internal IntPtr VmPtr { get; }
	}

	public readonly struct UnmanagedVm : IVmUnmanaged, IEquatable<IVmUnmanaged>
	{
		[NativeDisableUnsafePtrRestriction]
		private readonly IntPtr _ptr;

		IntPtr IVmUnmanaged.Ptr => _ptr;

		public readonly UnmanagedSlot Slot0;
		public readonly UnmanagedSlot Slot1;
		public readonly UnmanagedSlot Slot2;
		public readonly UnmanagedSlot Slot3;
		public readonly UnmanagedSlot Slot4;
		public readonly UnmanagedSlot Slot5;
		public readonly UnmanagedSlot Slot6;
		public readonly UnmanagedSlot Slot7;
		public readonly UnmanagedSlot Slot8;
		public readonly UnmanagedSlot Slot9;
		public readonly UnmanagedSlot Slot10;
		public readonly UnmanagedSlot Slot11;
		public readonly UnmanagedSlot Slot12;
		public readonly UnmanagedSlot Slot13;
		public readonly UnmanagedSlot Slot14;
		public readonly UnmanagedSlot Slot15;
		public readonly UnmanagedSlot Slot16;

		public UnmanagedVm(Vm vm) : this(((IVmUnmanaged) vm).Ptr) { }

		internal UnmanagedVm(IntPtr vmPtr)
		{
			_ptr = vmPtr;

			Slot0 = new UnmanagedSlot(_ptr, 0);
			Slot1 = new UnmanagedSlot(_ptr, 1);
			Slot2 = new UnmanagedSlot(_ptr, 2);
			Slot3 = new UnmanagedSlot(_ptr, 3);
			Slot4 = new UnmanagedSlot(_ptr, 4);
			Slot5 = new UnmanagedSlot(_ptr, 5);
			Slot6 = new UnmanagedSlot(_ptr, 6);
			Slot7 = new UnmanagedSlot(_ptr, 7);
			Slot8 = new UnmanagedSlot(_ptr, 8);
			Slot9 = new UnmanagedSlot(_ptr, 9);
			Slot10 = new UnmanagedSlot(_ptr, 10);
			Slot11 = new UnmanagedSlot(_ptr, 11);
			Slot12 = new UnmanagedSlot(_ptr, 12);
			Slot13 = new UnmanagedSlot(_ptr, 13);
			Slot14 = new UnmanagedSlot(_ptr, 14);
			Slot15 = new UnmanagedSlot(_ptr, 15);
			Slot16 = new UnmanagedSlot(_ptr, 16);
		}

		public override int GetHashCode() => VmUtils.Vm_GetHashCode(this);
		public override bool Equals(object obj) => VmUtils.Vm_Equals(this, obj);

		public bool Equals(IVmUnmanaged other) => VmUtils.Vm_Equals(this, other);

		public static bool operator ==(UnmanagedVm left, UnmanagedVm right) => VmUtils.Vm_EqualsOp(left, right);
		public static bool operator !=(UnmanagedVm left, UnmanagedVm right) => VmUtils.Vm_NotEqualOp(left, right);

		public static bool operator ==(Vm left, UnmanagedVm right) => VmUtils.Vm_EqualsOp(left, right);
		public static bool operator !=(Vm left, UnmanagedVm right) => VmUtils.Vm_EqualsOp(left, right);

		public static bool operator ==(UnmanagedVm left, Vm right) => VmUtils.Vm_NotEqualOp(left, right);
		public static bool operator !=(UnmanagedVm left, Vm right) => VmUtils.Vm_NotEqualOp(left, right);
	}

	public struct Vm : IVmManaged, IEquatable<IVmUnmanaged>, IDisposable
	{
		private IntPtr _ptr;
		IntPtr IVmUnmanaged.Ptr => _ptr;

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

		internal Vm(IntPtr ptr)
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
			if (this.IsValid() == false) return;

			Interop.wrenFreeVM(_ptr);
			Managed.ManagedClasses.Remove(_ptr);
			VmUtils.Vms.Data.Map.Remove(_ptr);
			_ptr = IntPtr.Zero;
		}

		public static Vm New(Config? config = null)
		{
			// set config
			var configuration = Config.ToInterop(config ?? Config.Default);
			configuration.NativeWrite = Wrench.WriteCallback;
			configuration.NativeError = Wrench.ErrorCallback;
			configuration.NativeResolveModule = Wrench.ResolveCallback;
			configuration.NativeLoadModule = Wrench.LoadCallback;
			configuration.NativeBindForeignMethod = Wrench.BindForeignMethodCallback;

			// get vm ptr
			IntPtr ptr = Interop.wrenNewVM(configuration);
			Vm vm = new Vm(ptr);

			// store data
			VmUtils.Vms.Data.Map.Add(ptr, vm);
			Managed.ManagedClasses.Add(ptr, new Managed()); // store *managed* events separately 

			return vm;
		}

		public override int GetHashCode() => VmUtils.Vm_GetHashCode(this);
		public override bool Equals(object obj) => VmUtils.Vm_Equals(this, obj);

		public bool Equals(IVmUnmanaged other) => VmUtils.Vm_Equals(this, other);

		public static bool operator ==(Vm left, Vm right) => VmUtils.Vm_EqualsOp(left, right);
		public static bool operator !=(Vm left, Vm right) => VmUtils.Vm_NotEqualOp(left, right);

		public static bool operator ==(Vm left, UnmanagedVm right) => VmUtils.Vm_EqualsOp(left, right);
		public static bool operator !=(Vm left, UnmanagedVm right) => VmUtils.Vm_EqualsOp(left, right);

		public static bool operator ==(UnmanagedVm left, Vm right) => VmUtils.Vm_NotEqualOp(left, right);
		public static bool operator !=(UnmanagedVm left, Vm right) => VmUtils.Vm_NotEqualOp(left, right);
	}

	public interface IVmUnmanaged
	{
		internal IntPtr Ptr { get; }
	}

	public interface IVmManaged : IVmUnmanaged { }

	public static class VmUtils
	{
		private static readonly ProfilerMarker PrefInterpret = PrefHelper.Create($"{nameof(Vm)}.{nameof(Interpret)}");
		private static readonly ProfilerMarker PrefCall = PrefHelper.Create($"{nameof(Vm)}.{nameof(Call)}");

		internal static readonly SharedStatic<StaticMap<Vm>> Vms = SharedStatic<StaticMap<Vm>>.GetOrCreate<Vm>();

		static VmUtils()
		{
			Vms.Data.Init(16);
		}

		#region Expected

		public static bool IsValid(this IVmUnmanaged vm) => IsValid(vm.Ptr);

		public static bool IsValid(IntPtr vmPtr)
		{
			IVmUnmanaged vm = FromPtr(vmPtr);
			return vm.Ptr != IntPtr.Zero;
		}

		public static bool ExpectedValid(this IVmUnmanaged vm) => ExpectedValid(vm.Ptr);

		public static bool ExpectedValid(IntPtr vmPtr)
		{
			if (IsValid(vmPtr)) return false;

			Expected.ThrowException(new ObjectDisposedException(nameof(Vm), "Vm is already disposed"));
			return true;
		}

		internal static bool ExpectedSameVm(this IVmUnmanaged vm, in IVmElement element) => ExpectedSameVm(vm.Ptr, element);
		internal static bool ExpectedSameVm(this IVmElement self, in IVmElement other) => ExpectedSameVm(self.VmPtr, other);

		internal static bool ExpectedSameVm(IntPtr vmPtr, in IVmElement element)
		{
			if (vmPtr == element.VmPtr) return false;

			Expected.ThrowException(new ArgumentOutOfRangeException(nameof(element),
				"Not all elements are from the same Vm"));
			return true;
		}

		internal static Vm FromPtr(IntPtr ptr)
		{
			return Vms.Data.Map.TryGetValue(ptr, out var vm) ? vm : new Vm();
		}

		internal static void GetData(IntPtr ptr, out Vm vm, out Managed managed)
		{
			vm = FromPtr(ptr);
			managed = Managed.ManagedClasses[ptr];
		}

		#endregion

		#region Methods

		public static InterpretResult Interpret(this IVmUnmanaged vm, [DisallowNull] string module, string source)
		{
			PrefInterpret.Begin();

			if (ExpectedValid(vm)) return InterpretResult.CompileError;
			if (string.IsNullOrEmpty(module)) throw new ArgumentNullException();
			var result = Interop.wrenInterpret(vm.Ptr, module, source);

			PrefInterpret.End();

			return result;
		}

		public static InterpretResult Call(this IVmUnmanaged vm, Handle handle)
		{
			PrefCall.Begin();

			if (ExpectedValid(vm)) return InterpretResult.CompileError;
			if (Handle.IfInvalid(handle)) return InterpretResult.CompileError;
			if (ExpectedSameVm(vm, handle)) return InterpretResult.CompileError;
			var result = Interop.wrenCall(vm.Ptr, handle.Ptr);

			PrefCall.End();

			return result;
		}

		public static void Gc(this IVmUnmanaged vm)
		{
			if (ExpectedValid(vm)) return;
			Interop.wrenCollectGarbage(vm.Ptr);
		}

		public static void Abort(this IVmUnmanaged vm, in ISlotUnmanaged msg)
		{
			if (ExpectedValid(vm)) return;
			Interop.wrenAbortFiber(vm.Ptr, msg.Index);
		}

		public static Handle MakeCallHandle(this IVmManaged vm, string signature)
		{
			return Handle.New(vm, signature);
		}

		public static bool HasModuleAndVariable(this IVmUnmanaged vm, string module, string variable)
		{
			if (vm.HasModule(module) == false) return false;
			if (vm.HasVariable(module, variable) == false) return false;
			return true;
		}

		/// <summary>
		/// Looks up the top level variable with <paramref name="name"/> in resolved <paramref name="module"/>, 
		/// returns false if not found. The module must be imported at the time, 
		/// use <see cref="HasModule"/>  to ensure that before calling.
		/// </summary>
		public static bool HasVariable(this IVmUnmanaged vm, string module, string name)
		{
			return Interop.wrenHasVariable(vm.Ptr, module, name);
		}

		/// <summary>
		/// Returns true if <paramref name="module"/> has been imported/resolved before, false if not.
		/// </summary>
		public static bool HasModule(this IVmUnmanaged vm, string module)
		{
			return Interop.wrenHasModule(vm.Ptr, module);
		}

		#endregion

		#region Slots

		public static int GetSlotCount(this IVmUnmanaged vm)
		{
			if (ExpectedValid(vm)) return 0;
			return Interop.wrenGetSlotCount(vm.Ptr);
		}


		public static void EnsureSlots(this IVmUnmanaged vm, int size)
		{
			if (ExpectedValid(vm)) return;
			Interop.wrenEnsureSlots(vm.Ptr, size);
		}

		#endregion

		#region Managed

		public static void SetWriteListener(this IVmManaged vm, WriteDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].WriteEvent = @delegate;
		}

		public static void SetErrorListener(this IVmManaged vm, ErrorDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].ErrorEvent = @delegate;
		}

		public static void SetResolveModuleListener(this IVmManaged vm, ResolveModuleDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].ResolveModuleEvent = @delegate;
		}

		public static void SetLoadModuleListener(this IVmManaged vm, LoadModuleDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].LoadModuleEvent = @delegate;
		}

		public static void SetBindForeignMethodListener(this IVmManaged vm, BindForeignMethodDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].BindForeignMethodEvent = @delegate;
		}

		public static void SetBindForeignClassListener(this IVmManaged vm, BindForeignClassDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].BindForeignClassEvent = @delegate;
		}

		public static void SetUserData<T>(this IVmManaged vm, T obj)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].UserData = obj;
		}

		public static T GetUserData<T>(this IVmManaged vm)
		{
			if (ExpectedValid(vm)) return default;
			return (T) Managed.ManagedClasses[vm.Ptr].UserData;
		}

		#endregion

		#region Equality

		public static bool Vm_Equals(IVmUnmanaged self, object obj) => obj switch
		{
			ISlotUnmanaged slot => Vm_Equals(self, slot),
			_ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null),
		};

		public static bool Vm_Equals(in IVmUnmanaged self, in IVmUnmanaged other) => self.Ptr == other.Ptr;

		public static int Vm_GetHashCode(in IVmUnmanaged self) => self.Ptr.GetHashCode();
		public static bool Vm_EqualsOp(in IVmUnmanaged left, in IVmUnmanaged right) => Vm_Equals(left, right);
		public static bool Vm_NotEqualOp(in IVmUnmanaged left, in IVmUnmanaged right) => Vm_Equals(left, right) == false;

		#endregion
	}
}
