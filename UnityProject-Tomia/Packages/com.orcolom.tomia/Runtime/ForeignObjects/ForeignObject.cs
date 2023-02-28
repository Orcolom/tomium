﻿using System;

namespace Tomia
{
	public readonly struct ForeignObject<T>
	{
		private readonly IntPtr _ptr;

		public bool IsValid => Managed.ForeignObjects.ContainsKey(_ptr);
		private static int TypeID { get; } = ForeignMetadata.GetTypeID<T>();

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

		internal static ForeignObject<T> FromPtr(IntPtr ptr)
		{
			var foreignObject = new ForeignObject<T>(ptr);
			ExpectedValid(foreignObject);
			return foreignObject;
		}

		public ForeignObject<TType> As<TType>()
		{
			return new ForeignObject<TType>(_ptr);
		}

		private static bool ExpectedValid(in ForeignObject<T> foreign)
		{
			if (foreign.IsValid) return false;

			throw new ObjectDisposedException("ForeignObject is already disposed");
			// return true;
		}
		
		public static void Add(IntPtr ptr, T data)
		{
			ForeignMetadata.TryAdd(ptr, new ForeignMetadata(ForeignStyle.Object, TypeID));
			Managed.ForeignObjects.TryAdd(ptr, data);
		}
	}
}
