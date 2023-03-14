using System;
using System.Runtime.InteropServices;

namespace Tomium.Native
{
	/// <summary>
	/// interop struct for WrenConfiguration
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct NativeBindForeignMethodResult
	{
		public IntPtr ExecuteFn;
		public IntPtr UserData;
	}
}
