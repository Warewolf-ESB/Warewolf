using System;

namespace IronRuby.Builtins
{
	[Flags]
	public enum ModuleRestrictions
	{
		None = 0,
		NoOverrides = 1,
		NoNameMapping = 2,
		NotPublished = 4,
		NoUnderlyingType = 8,
		AllowReopening = 0x18,
		Builtin = 7,
		All = 7
	}
}
