using System;

namespace IronRuby.Builtins
{
	[Flags]
	public enum IOMode
	{
		ReadOnly = 0,
		WriteOnly = 1,
		ReadWrite = 2,
		Closed = 3,
		ReadWriteMask = 3,
		WriteAppends = 8,
		CreateIfNotExists = 0x100,
		Truncate = 0x200,
		ErrorIfExists = 0x400,
		PreserveEndOfLines = 0x8000,
		Default = 0
	}
}
