using System;

namespace Tomia.Native
{
	/// <summary>
	/// result from <see cref="NativeConfig.NativeLoadModule"/> call
	/// </summary>
	internal struct NativeLoadModuleResult
	{
		/// <summary>
		/// Source code of the module
		/// </summary>
		public IntPtr Source;

		/// <summary>
		/// an optional callback that will be called once Wren is done with the result.
		/// </summary>
		public IntPtr OnComplete;

		public IntPtr UserData;
	}
}
