using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("TrueClass")]
	public static class TrueClass
	{
		[RubyMethod("to_s")]
		public static MutableString ToString(bool self)
		{
			return MutableString.CreateAscii("true");
		}

		[RubyMethod("&")]
		public static bool And(bool self, object obj)
		{
			return obj != null;
		}

		[RubyMethod("&")]
		public static bool And(bool self, bool obj)
		{
			return obj;
		}

		[RubyMethod("^")]
		public static bool Xor(bool self, object obj)
		{
			return obj == null;
		}

		[RubyMethod("^")]
		public static bool Xor(bool self, bool obj)
		{
			return !obj;
		}

		[RubyMethod("|")]
		public static bool Or(bool self, object obj)
		{
			return true;
		}
	}
}
