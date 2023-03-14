using NUnit.Framework;

namespace Tomia.Tests.Helpers
{
	public class Marker
	{
		private bool _isReached;
		private string _name;

		public Marker(string name)
		{
			_name = name;
		}

		public bool IsReached => _isReached;

		public void Trigger() => _isReached = true;

		public void Verify()
		{
			if (_isReached == false) Assert.Fail($"Marker `{_name}` was not reached");
		}

		public void Reset()
		{
			_isReached = false;
		}
	}
}
