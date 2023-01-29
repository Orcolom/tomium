using System;

namespace Tomia.Native
{
	internal struct NativeForeignClass
	{
		/// <summary>
		/// The callback invoked when the foreign object is created.
		///
		/// This must be provided. Inside the body of this,
		/// it must call <see cref="Interop.wrenSetSlotNewForeign"/> exactly once.
		/// </summary>
		public IntPtr AllocateFn;


		/// <summary>
		/// user data bound to allocatFn
		/// </summary>
		public IntPtr AllocateUserData;

		/// <summary>
		/// The callback invoked when the garbage collector is about to collect a foreign object's memory.
		/// This may be `null` if the foreign class does not need to finalize.
		/// </summary>
		public IntPtr FinalizeFn;
		
		/// <summary>
		/// user data bound to allocatFn
		/// </summary>
		public IntPtr FinalizeUserData;
	}
}
