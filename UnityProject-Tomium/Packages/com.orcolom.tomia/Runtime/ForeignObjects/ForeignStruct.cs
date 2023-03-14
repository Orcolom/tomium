using System;

namespace Tomia
{
	public readonly struct ForeignStruct<T>
		where T : unmanaged
	{
		internal static int TypeID { get; } = ForeignMetadata.GetTypeID<T>();

		private readonly IntPtr _ptr;

		public T Value
		{
			get
			{
				if (ExpectedValid(this)) return default;
				return ForeignValue<T>.TryGetValue(_ptr, out var value) ? value : default;
			}
			set
			{
				if (ExpectedValid(this)) return;
				ForeignValue<T>.Set(_ptr, value);
			}
		}

		public bool IsValid => ForeignValue<T>.ContainsKey(_ptr);

		internal ForeignStruct(IntPtr ptr)
		{
			_ptr = ptr;
		}

		internal static ForeignStruct<T> FromPtr(IntPtr ptr)
		{
			var foreignStruct = new ForeignStruct<T>(ptr);
			ExpectedValid(foreignStruct);
			return foreignStruct;
		}

		private static bool ExpectedValid(in ForeignStruct<T> foreign)
		{
			if (foreign.IsValid) return false;

			throw new ObjectDisposedException("ForeignStruct is already disposed");
			// return true;
		}

		internal static void Add(IntPtr ptr, T data)
		{
			ForeignMetadata.TryAdd(ptr, new ForeignMetadata(ForeignStyle.Struct, TypeID));
			ForeignValue<T>.TryAdd(ptr, data);
		}
	}
}
