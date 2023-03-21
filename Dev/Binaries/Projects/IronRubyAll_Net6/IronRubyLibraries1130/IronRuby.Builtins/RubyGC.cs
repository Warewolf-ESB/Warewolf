using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("GC")]
	public static class RubyGC
	{
		[RubyMethod("enable", RubyMethodAttributes.PublicSingleton)]
		public static bool Enable(object self)
		{
			return false;
		}

		[RubyMethod("disable", RubyMethodAttributes.PublicSingleton)]
		public static bool Disable(object self)
		{
			return false;
		}

		[RubyMethod("start", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("garbage_collect", RubyMethodAttributes.PublicInstance)]
		public static void GarbageCollect(object self)
		{
			GC.Collect();
		}
	}
}
