using System;

namespace IronRuby.Runtime
{
	[Flags]
	public enum RubyMemberFlags
	{
		Invalid = 0,
		Public = 1,
		Private = 2,
		Protected = 4,
		VisibilityMask = 7,
		Empty = 8
	}
}
