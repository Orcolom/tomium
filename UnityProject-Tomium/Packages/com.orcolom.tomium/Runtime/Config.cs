using System;
using Tomium.Native;

namespace Tomium
{
	public struct Config
	{
		/// <inheritdoc cref="NativeConfig.InitialHeapSize"/>
		public ulong InitialHeapSize;

		/// <inheritdoc cref="NativeConfig.MinHeapSize"/>
		public ulong MinHeapSize;

		/// <inheritdoc cref="NativeConfig.HeapGrowthPercent"/>
		public int HeapGrowthPercent;

		/// <summary>
		/// the default settings
		/// </summary>
		public static readonly Config Default;

		/// <summary>
		/// reallocate function needed internally
		/// </summary>
		internal static readonly NativeReallocateDelegate NativeReallocate;

		static Config()
		{
			NativeConfig nativeConfig = new NativeConfig();
			Interop.wrenInitConfiguration(nativeConfig);
			Default = FromInterop(nativeConfig);
			NativeReallocate = nativeConfig.NativeReallocate;
		}

		#region Cast

		internal static NativeConfig ToInterop(Config config)
		{
			return new NativeConfig
			{
				InitialHeapSize = new UIntPtr(config.InitialHeapSize),
				MinHeapSize = new UIntPtr(config.MinHeapSize),
				HeapGrowthPercent = config.HeapGrowthPercent,
			};
		}

		private static Config FromInterop(NativeConfig config)
		{
			return new Config
			{
				HeapGrowthPercent = config.HeapGrowthPercent,
				InitialHeapSize = config.InitialHeapSize.ToUInt64(),
				MinHeapSize = config.MinHeapSize.ToUInt64(),
			};
		}

		#endregion
	}
}
