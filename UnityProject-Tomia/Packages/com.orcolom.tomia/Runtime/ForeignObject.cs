using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Tomia.Native;
using Object = UnityEngine.Object;

namespace Tomia
{
	internal enum ForeignStyle : byte
	{
		Invalid,
		Object,
		Struct,
	}

	internal readonly struct ForeignTypeMetadata
	{
		public static readonly SharedStatic<StaticMap<ForeignTypeMetadata>> StaticMap =
			SharedStatic<StaticMap<ForeignTypeMetadata>>.GetOrCreate<ForeignTypeMetadata>();
		
		public static readonly Dictionary<int, Type> IdToType = new Dictionary<int, Type>(32);

		public readonly int ID;
		public readonly ForeignStyle Style;
		
		static ForeignTypeMetadata()
		{
			StaticMap.Data.Init(512);
		}

		public ForeignTypeMetadata(ForeignStyle style, int id)
		{
			Style = style;
			ID = id;
		}

		public static int GetID<T>()
		{
			// NOTE: is hashcode unique enough?
			var id = typeof(T).GetHashCode();

			using (ProfilerUtils.AllocScope.Auto())
			{
				IdToType.TryAdd(id, typeof(T));
			}

			return id;
		}
		
		public Type GetType(int id)
		{
			if (IdToType.TryGetValue(id, out var type)) return type;
			return null;
		}
	}

	internal static class ForeignData<T>
		where T : unmanaged
	{
		public static readonly SharedStatic<StaticMap<T>> StaticMap =
			SharedStatic<StaticMap<T>>.GetOrCreate<T>();

		static ForeignData()
		{
			StaticMap.Data.Init(64);
		}
	}

	public readonly struct ForeignStruct<T>
		where T : unmanaged
	{
		private static int TypeID { get; } = ForeignTypeMetadata.GetID<T>();

		private readonly IntPtr _ptr;

		public T Value
		{
			get
			{
				if (ExpectedValid(this)) return default;
				return ForeignData<T>.StaticMap.Data.Map.TryGetValue(_ptr, out var value) ? value : default;
			}
			set
			{
				if (ExpectedValid(this)) return;
				using (ProfilerUtils.AllocScope.Auto())
				{
					ForeignData<T>.StaticMap.Data.Map[_ptr] = value;
				}
			}
		}

		public bool IsValid => ForeignData<T>.StaticMap.Data.Map.ContainsKey(_ptr);

		internal ForeignStruct(IntPtr ptr)
		{
			_ptr = ptr;
		}

		public static ForeignStruct<T> FromPtr(IntPtr ptr)
		{
			var foreignStruct = new ForeignStruct<T>(ptr);
			ExpectedValid(foreignStruct);
			return foreignStruct;
		}

		internal static bool ExpectedValid(in ForeignStruct<T> foreign)
		{
			if (foreign.IsValid) return false;

			throw new ObjectDisposedException("ForeignStruct is already disposed");
			// return true;
		}

		public static void Add(IntPtr ptr, T data)
		{
			using (ProfilerUtils.AllocScope.Auto())
			{
				ForeignTypeMetadata.StaticMap.Data.Map.TryAdd(ptr, new ForeignTypeMetadata(ForeignStyle.Struct, TypeID));
				ForeignData<T>.StaticMap.Data.Map.TryAdd(ptr, data);
			}
		}
	}

	public readonly struct ForeignObject<T>
	{
		private readonly IntPtr _ptr;

		public bool IsValid => Managed.ForeignObjects.ContainsKey(_ptr);
		private static int TypeID { get; } = ForeignTypeMetadata.GetID<T>();

		public T Value
		{
			get
			{
				if (ExpectedValid(this)) return default;
				return Managed.ForeignObjects.TryGetValue(_ptr, out var value) ? (T) value : default;
			}
			set
			{
				if (ExpectedValid(this)) return;
				using (ProfilerUtils.AllocScope.Auto())
				{
					Managed.ForeignObjects[_ptr] = value;
				}
			}
		}

		internal ForeignObject(IntPtr ptr)
		{
			_ptr = ptr;
		}

		public static ForeignObject<T> FromPtr(IntPtr ptr)
		{
			var foreignObject = new ForeignObject<T>(ptr);
			ExpectedValid(foreignObject);
			return foreignObject;
		}

		public ForeignObject<TType> As<TType>()
		{
			return new ForeignObject<TType>(_ptr);
		}

		internal static bool ExpectedValid(in ForeignObject<T> foreign)
		{
			if (foreign.IsValid) return false;

			throw new ObjectDisposedException("ForeignObject is already disposed");
			// return true;
		}
		
		public static void Add(IntPtr ptr, T data)
		{
			using (ProfilerUtils.AllocScope.Auto())
			{
				ForeignTypeMetadata.StaticMap.Data.Map.TryAdd(ptr, new ForeignTypeMetadata(ForeignStyle.Object, TypeID));
				Managed.ForeignObjects.TryAdd(ptr, data);
			}
		}
	}
}
