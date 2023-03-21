using IronRuby.Builtins;

namespace IronRuby.Runtime
{
	public static class UnionSpecializations
	{
		public static bool IsFixnum(this Union<int, MutableString> union)
		{
			return object.ReferenceEquals(union.Second, null);
		}

		public static int Fixnum(this Union<int, MutableString> union)
		{
			return union.First;
		}

		public static MutableString String(this Union<int, MutableString> union)
		{
			return union.Second;
		}

		public static bool IsFixnum(this Union<MutableString, int> union)
		{
			return object.ReferenceEquals(union.First, null);
		}

		public static int Fixnum(this Union<MutableString, int> union)
		{
			return union.Second;
		}

		public static MutableString String(this Union<MutableString, int> union)
		{
			return union.First;
		}
	}
}
