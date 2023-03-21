using System;
using System.Text;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[]
	{
		typeof(ClrString),
		typeof(Enumerable),
		typeof(Comparable)
	})]
	[RubyClass(Extends = typeof(char), Restrictions = ModuleRestrictions.None)]
	public static class CharOps
	{
		private static Exception EmptyError(string argType)
		{
			return RubyExceptions.CreateArgumentError("cannot convert an empty {0} to System::Char", argType);
		}

		[RubyConstructor]
		public static char Create(RubyClass self, int utf16)
		{
			try
			{
				return (char)checked((ushort)utf16);
			}
			catch (OverflowException)
			{
				throw RubyExceptions.CreateRangeError("{0} is not a valid UTF-16 character code", utf16);
			}
		}

		[RubyConstructor]
		public static char Create(RubyClass self, char c)
		{
			return c;
		}

		[RubyConstructor]
		public static char Create(RubyClass self, [NotNull] char[] chars)
		{
			if (chars.Length == 0)
			{
				throw EmptyError("System::Char[]");
			}
			return chars[0];
		}

		[RubyConstructor]
		public static char Create(RubyClass self, [NotNull] string str)
		{
			if (str.Length == 0)
			{
				throw EmptyError("string");
			}
			return str[0];
		}

		[RubyConstructor]
		public static char Create(RubyClass self, [DefaultProtocol] MutableString str)
		{
			if (str.IsEmpty)
			{
				throw EmptyError("string");
			}
			return str.GetChar(0);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyContext context, char self)
		{
			return MutableString.Create(MutableString.AppendUnicodeRepresentation(new StringBuilder().Append('\''), self.ToString(), MutableString.Escape.Special, 39, -1).Append("' (Char)").ToString(), context.GetIdentifierEncoding());
		}

		[RubyMethod("dump", RubyMethodAttributes.PublicInstance)]
		public static MutableString Dump(char self)
		{
			return MutableString.CreateAscii(MutableString.AppendUnicodeRepresentation(new StringBuilder().Append('\''), self.ToString(), MutableString.Escape.NonAscii | MutableString.Escape.Special, 39, -1).Append("' (Char)").ToString());
		}
	}
}
