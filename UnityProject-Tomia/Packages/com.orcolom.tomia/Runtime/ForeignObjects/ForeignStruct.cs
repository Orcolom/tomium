using System;

namespace Tomia
{
	public readonly struct ForeignStruct<T>
		where T : unmanaged
	{
		internal static int TypeID { get; } = ForeignMetadata.GetID<T>();

		private readonly IntPtr _ptr;

		public T Value
		{
			get
			{
				if (ExpectedValid(this)) return default;
				return ForeignValue<T>.StaticMap.Data.Map.TryGetValue(_ptr, out var value) ? value : default;
			}
			set
			{
				if (ExpectedValid(this)) return;
				using (ProfilerUtils.AllocScope.Auto())
				{
					ForeignValue<T>.StaticMap.Data.Map[_ptr] = value;
				}
			}
		}

		public bool IsValid => ForeignValue<T>.StaticMap.Data.Map.ContainsKey(_ptr);

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
				ForeignMetadata.StaticMap.Data.Map.TryAdd(ptr, new ForeignMetadata(ForeignStyle.Struct, TypeID));
				ForeignValue<T>.StaticMap.Data.Map.TryAdd(ptr, data);
			}
		}
	}
}
