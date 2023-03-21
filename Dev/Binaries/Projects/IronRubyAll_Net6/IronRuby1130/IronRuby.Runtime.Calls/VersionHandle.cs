using System.Diagnostics;

namespace IronRuby.Runtime.Calls
{
	public sealed class VersionHandle
	{
		public int Method;

		internal VersionHandle(int method)
		{
			Method = method;
		}

		[Conditional("DEBUG")]
		internal void SetName(string className)
		{
		}
	}
}
