using System;
using System.Collections.Generic;
using Unity.Burst;

namespace Tomium
{
	/// <summary>
	/// Hold meta data about every foreign data, what type it is and if its an ForeignObject or a ForeignStruct 
	/// </summary>
	internal readonly struct ForeignMetadata
	{
		private static readonly SharedStatic<StaticMap<ForeignMetadata>> Metadata =
			SharedStatic<StaticMap<ForeignMetadata>>.GetOrCreate<ForeignMetadata>();
		
		/// <summary>
		/// ForeignStructs use this to remove values on finalize
		/// </summary>
		private static readonly Dictionary<int, (Action<IntPtr>, Type)> TypeIdToRemove = new Dictionary<int, (Action<IntPtr>, Type)>(32);

		public readonly int TypeID;
		public readonly ForeignStyle Style;
		
		static ForeignMetadata()
		{
			Metadata.Data.Init(512);
		}

		public ForeignMetadata(ForeignStyle style, int typeID)
		{
			Style = style;
			TypeID = typeID;
		}

		public override string ToString()
		{
			return $"{Style}, {TypeID}, {(TypeIdToRemove.TryGetValue(TypeID, out var value) ? value.Item2.Name : string.Empty)}";
		}

		/// <summary>
		/// Use <see cref="ForeignStruct{T}.TypeID"/> or <see cref="ForeignObject{T}.TypeID"/> as they are the cashed versions of this
		/// </summary>
		internal static int GetTypeID<T>()
		{
			// NOTE: is hashcode unique enough?
			var id = typeof(T).GetHashCode();
			return id;
		}

		internal static bool TryAdd(IntPtr ptr, ForeignMetadata metadata)
		{
			return Metadata.Data.TryAdd(ptr, metadata);
		}

		internal static bool TryGetValue(IntPtr ptr, out ForeignMetadata value) => Metadata.Data.TryGetValue(ptr, out value);

		internal static void Remove(IntPtr ptr)
		{
			Metadata.Data.Remove(ptr);
		}
		
		
		internal static bool TryAddType<T>(Action<IntPtr> remove)
			where T : unmanaged
		{
			using (ProfilerUtils.AllocScope.Auto())
			{
				return TypeIdToRemove.TryAdd(ForeignStruct<T>.TypeID, (remove, typeof(T)));
			}
		}

		internal static bool TryGetRemoveAction(int typeID, out Action<IntPtr> remove)
		{
			if (TypeIdToRemove.TryGetValue(typeID, out var typeData))
			{
				remove = typeData.Item1;
				return true;
			}
			
			remove = null;
			return false;
		}
	}
}
