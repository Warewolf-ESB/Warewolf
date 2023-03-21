using System;

namespace IronRuby.Runtime
{
	[Flags]
	public enum RubyMethodAttributes
	{
		None = 0,
		Public = 1,
		Private = 2,
		Protected = 4,
		DefaultVisibility = 1,
		VisibilityMask = 7,
		Empty = 8,
		MemberFlagsMask = 0xF,
		Instance = 0x10,
		Singleton = 0x20,
		NoEvent = 0x40,
		PublicInstance = 0x11,
		PrivateInstance = 0x12,
		ProtectedInstance = 0x14,
		PublicSingleton = 0x21,
		PrivateSingleton = 0x22,
		ProtectedSingleton = 0x24,
		ModuleFunction = 0x31,
		Default = 0x11
	}
}
