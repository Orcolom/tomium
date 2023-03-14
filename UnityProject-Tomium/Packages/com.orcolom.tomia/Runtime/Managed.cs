using System;
using System.Collections.Generic;

namespace Tomia
{
	/// <summary>
	/// object with managed data from a vm
	/// </summary>
	internal class Managed
	{
		internal static readonly Dictionary<IntPtr, ForeignAction> Actions =
			new Dictionary<IntPtr, ForeignAction>(128);

		internal static readonly Dictionary<IntPtr, Managed> ManagedClasses = new Dictionary<IntPtr, Managed>(128);

		internal static readonly Dictionary<IntPtr, object> ForeignObjects =
			new Dictionary<IntPtr, object>(512);

		public WriteDelegate WriteEvent;
		public ErrorDelegate ErrorEvent;
		public LoadModuleDelegate LoadModuleEvent;
		public ResolveModuleDelegate ResolveModuleEvent;
		public BindForeignMethodDelegate BindForeignMethodEvent;
		public BindForeignClassDelegate BindForeignClassEvent;
		public object UserData;
		
		internal readonly List<IntPtr> CallHandles = new List<IntPtr>(32);
	}
}
