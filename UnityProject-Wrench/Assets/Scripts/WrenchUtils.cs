using Wrench;
using Wrench.Builder;

namespace Wrench
{
	public static class WrenchUtils
	{
		public delegate void UnManagedForeignActionS1<TType>(in Vm vm, UnmanagedForeignObject<TType> fo)
			where TType : unmanaged;

		public delegate void UnManagedForeignActionS2<TType>(in Vm vm, in Slot s1, UnmanagedForeignObject<TType> fo)
			where TType : unmanaged;

		public delegate void UnManagedForeignActionS3<TType>(in Vm vm, in Slot s1, in Slot s2, UnmanagedForeignObject<TType> fo)
			where TType : unmanaged;

		public delegate void ManagedForeignActionS1<TType>(in Vm vm, ForeignObject<TType> fo);

		public delegate void ManagedForeignActionS2<TType>(in Vm vm, in Slot s1, ForeignObject<TType> fo);

		public delegate void ManagedForeignActionS3<TType>(in Vm vm, in Slot s1, in Slot s2, ForeignObject<TType> fo);

		public static void Method(this Class @class, Signature signature, ForeignAction action)
		{
			@class.Add(new Method(signature, new ForeignMethod(action)));
		}
		
		public static void Field<TType>(this Class @class, string name, bool isStatic = false,
			ManagedForeignActionS1<TType> get = null,
			ManagedForeignActionS2<TType> set = null)
		{
			if (get != null)
			{
				@class.Add(new Method(
					new Signature(isStatic ? MethodType.StaticFieldGetter : MethodType.FieldGetter, name),
					new ForeignMethod((in Vm vm) =>
					{
						vm.EnsureSlots(1);
						if (Expected.ForeignType<TType>(vm, vm.Slot0, out var foreign)) return;
						get.Invoke(vm, foreign);
					})
				));
			}

			if (set != null)
			{
				@class.Add(new Method(
					new Signature(isStatic ? MethodType.StaticFieldSetter : MethodType.FieldSetter, name),
					new ForeignMethod((in Vm vm) =>
					{
						vm.EnsureSlots(2);
						if (Expected.ForeignType<TType>(vm, vm.Slot0, out var foreign)) return;
						set.Invoke(vm, vm.Slot1, foreign);
					})
				));
			}
		}
		
		public static void Field<TType>(this Class @class, string name, bool isStatic = false,
			UnManagedForeignActionS1<TType> get = null,
			UnManagedForeignActionS2<TType> set = null)
			where TType : unmanaged
		{
			if (get != null)
			{
				@class.Add(new Method(
					new Signature(isStatic ? MethodType.StaticFieldGetter : MethodType.FieldGetter, name),
					new ForeignMethod((in Vm vm) =>
					{
						vm.EnsureSlots(1);
						if (Expected.UnManagedForeignType<TType>(vm, vm.Slot0, out var foreign)) return;
						get.Invoke(vm, foreign);
					})
				));
			}

			if (set != null)
			{
				@class.Add(new Method(
					new Signature(isStatic ? MethodType.StaticFieldSetter : MethodType.FieldSetter, name),
					new ForeignMethod((in Vm vm) =>
					{
						vm.EnsureSlots(2);
						if (Expected.UnManagedForeignType<TType>(vm, vm.Slot0, out var foreign)) return;
						set.Invoke(vm, vm.Slot1, foreign);
					})
				));
			}
		}

		public static void SubScript<TType>(this Class @class,
			UnManagedForeignActionS2<TType> get = null,
			UnManagedForeignActionS3<TType> set = null)
			where TType : unmanaged
		{
			if (get != null)
			{
				@class.Add(new Method(
					new Signature(MethodType.SubScriptGetter, null),
					new ForeignMethod((in Vm vm) =>
					{
						vm.EnsureSlots(2);
						if (Expected.UnManagedForeignType<TType>(vm, vm.Slot0, out var foreign)) return;
						if (Expected.Type(vm, vm.Slot1, ValueType.Number, ValueType.String)) return;
						get.Invoke(vm, vm.Slot1, foreign);
					})
				));
			}

			if (set != null)
			{
				@class.Add(new Method(
					new Signature(MethodType.SubScriptSetter, null),
					new ForeignMethod((in Vm vm) =>
					{
						vm.EnsureSlots(3);
						if (Expected.UnManagedForeignType<TType>(vm, vm.Slot0, out var foreign)) return;
						set.Invoke(vm, vm.Slot1, vm.Slot2, foreign);
					})
				));
			}
		}

		// public static void NewForeignFromClass<TClass, TData>(in Slot slot, TData snowflake)
		// 	where TClass : Class 
		// 	where TData : unmanaged
		// {
		// 	var snowflakeClass = WrenSystem.Modules.Get<TClass>();
		// 	slot.GetVariable(snowflakeClass.Module.Path, snowflakeClass.Name.Text);
		// 	slot.SetNewForeign<TData>(slot, snowflake);
		// }
	}
}
