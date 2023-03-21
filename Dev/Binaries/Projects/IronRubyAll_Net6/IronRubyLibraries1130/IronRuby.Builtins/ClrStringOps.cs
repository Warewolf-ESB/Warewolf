using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass(Extends = typeof(string), Restrictions = ModuleRestrictions.None)]
	[HideMethod("insert")]
	[HideMethod("split")]
	[HideMethod("clone")]
	[HideMethod("[]")]
	[HideMethod("==")]
	[Includes(new Type[]
	{
		typeof(ClrString),
		typeof(Enumerable),
		typeof(Comparable)
	})]
	public static class ClrStringOps
	{
		[RubyConstructor]
		public static string Create(RubyClass self, [DefaultProtocol] MutableString str)
		{
			return str.ToString();
		}

		[RubyConstructor]
		public static string Create(RubyClass self, char c, int repeatCount)
		{
			return new string(c, repeatCount);
		}

		[RubyConstructor]
		public static string Create(RubyClass self, [NotNull] char[] chars)
		{
			return new string(chars);
		}

		[RubyConstructor]
		public static string Create(RubyClass self, [NotNull] char[] chars, int startIndex, int length)
		{
			return new string(chars, startIndex, length);
		}
	}
}
