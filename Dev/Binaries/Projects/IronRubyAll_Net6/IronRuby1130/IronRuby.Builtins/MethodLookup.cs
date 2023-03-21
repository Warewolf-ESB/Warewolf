using System;

namespace IronRuby.Builtins
{
	[Flags]
	public enum MethodLookup
	{
		Default = 0,
		Virtual = 1,
		ReturnForwarder = 2,
		FallbackToObject = 4
	}
}
