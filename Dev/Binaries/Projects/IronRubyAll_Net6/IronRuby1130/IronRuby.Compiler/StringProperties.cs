using System;

namespace IronRuby.Compiler
{
	[Flags]
	public enum StringProperties : byte
	{
		Default = 0,
		ExpandsEmbedded = 1,
		RegularExpression = 2,
		Words = 4,
		Symbol = 8,
		IndentedHeredoc = 0x10
	}
}
