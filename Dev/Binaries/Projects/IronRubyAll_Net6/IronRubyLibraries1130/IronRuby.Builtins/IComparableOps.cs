using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule(Extends = typeof(IComparable), Restrictions = ModuleRestrictions.None)]
	[Includes(new Type[] { typeof(Comparable) })]
	public static class IComparableOps
	{
		[RubyMethod("<=>", RubyMethodAttributes.PublicInstance)]
		public static int Compare(IComparable self, object other)
		{
			return self.CompareTo(other);
		}
	}
}
