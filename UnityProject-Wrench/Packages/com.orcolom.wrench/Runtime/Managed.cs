using System;
using System.Collections.Generic;

namespace Wrench
{
	/// <summary>
	/// object with managed data from a vm
	/// </summary>
	internal class Managed
	{
		internal static Dictionary<IntPtr, ForeignAction> Actions =
			new Dictionary<IntPtr, ForeignAction>(128);

		internal static Dictionary<IntPtr, Managed> ManagedClasses = new Dictionary<IntPtr, Managed>(128);

		internal static Dictionary<IntPtr, object> ForeignObjects =
			new Dictionary<IntPtr, object>(512);

		public WriteDelegate WriteEvent;
		public ErrorDelegate ErrorEvent;
		public LoadModuleDelegate LoadModuleEvent;
		public ResolveModuleDelegate ResolveModuleEvent;
		public BindForeignMethodDelegate BindForeignMethodEvent;
		public BindForeignClassDelegate BindForeignClassEvent;
		public object UserData;
	}
}
