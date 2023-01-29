namespace Tomia
{
	/// <summary>
	/// types a WrenValue can have
	/// </summary>
	public enum ValueType
	{
		Bool = 0,
		Number = 1,
		Foreign = 2,
		List = 3,
		Map = 4,
		Null = 5,
		String = 6,

		// The object is of a type that isn't accessible by the C API.
		Unknown = 7,
	}
}
