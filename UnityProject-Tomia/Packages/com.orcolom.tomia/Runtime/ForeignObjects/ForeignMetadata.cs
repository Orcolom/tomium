using System;
using System.Collections.Generic;
using Unity.Burst;

namespace Tomia
{
	internal readonly struct ForeignMetadata
	{
		public static readonly SharedStatic<StaticMap<ForeignMetadata>> StaticMap =
			SharedStatic<StaticMap<ForeignMetadata>>.GetOrCreate<ForeignMetadata>();
		
		public static readonly Dictionary<int, Action<IntPtr>> TypeIdToRemove = new Dictionary<int, Action<IntPtr>>(32);

		public readonly int ID;
		public readonly ForeignStyle Style;
		
		static ForeignMetadata()
		{
			StaticMap.Data.Init(512);
		}

		public ForeignMetadata(ForeignStyle style, int id)
		{
			Style = style;
			ID = id;
		}

		public static int GetID<T>()
		{
			// NOTE: is hashcode unique enough?
			var id = typeof(T).GetHashCode();
			return id;
		}
	}
}
