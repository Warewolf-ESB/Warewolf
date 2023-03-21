using System;

namespace IronRuby.Runtime.Calls
{
	[Flags]
	public enum BlockSignatureAttributes
	{
		None = 0,
		HasProcParameter = 1,
		HasUnsplatParameter = 2
	}
}
