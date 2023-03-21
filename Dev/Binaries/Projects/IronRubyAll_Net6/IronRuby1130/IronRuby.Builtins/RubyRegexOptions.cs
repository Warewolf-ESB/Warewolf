using System;

namespace IronRuby.Builtins
{
	[Flags]
	public enum RubyRegexOptions
	{
		NONE = 0,
		IgnoreCase = 1,
		Extended = 2,
		Multiline = 4,
		Once = 8,
		FIXED = 0x10,
		EUC = 0x20,
		SJIS = 0x40,
		UTF8 = 0x80,
		EncodingMask = 0xF0
	}
}
