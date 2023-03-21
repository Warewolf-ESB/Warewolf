using System;

namespace IronRuby.Runtime
{
	[Flags]
	public enum LoadFlags
	{
		None = 0,
		LoadOnce = 1,
		LoadIsolated = 2,
		AppendExtensions = 4,
		ResolveLoaded = 8,
		AnyLanguage = 0x10,
		Require = 5
	}
}
