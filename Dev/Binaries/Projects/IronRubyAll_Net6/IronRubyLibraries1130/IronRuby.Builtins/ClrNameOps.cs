using System;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("Name", Extends = typeof(ClrName), DefineIn = typeof(IronRubyOps.Clr))]
	public static class ClrNameOps
	{
		[RubyMethod("inspect")]
		public static MutableString Inspect(ClrName self)
		{
			return ClrString.Inspect(self.MangledName);
		}

		[RubyMethod("dump")]
		public static MutableString Dump(ClrName self)
		{
			return ClrString.Dump(self.MangledName);
		}

		[RubyMethod("clr_name")]
		public static MutableString GetClrName(RubyContext context, ClrName self)
		{
			return MutableString.Create(self.ActualName, context.GetIdentifierEncoding());
		}

		[RubyMethod("ruby_name")]
		[RubyMethod("to_s")]
		[RubyMethod("to_str")]
		public static MutableString GetRubyName(RubyContext context, ClrName self)
		{
			return MutableString.Create(self.MangledName, context.GetIdentifierEncoding());
		}

		[RubyMethod("intern", Compatibility = RubyCompatibility.Default)]
		[RubyMethod("to_sym")]
		public static RubySymbol ToSymbol(RubyContext context, ClrName self)
		{
			return context.EncodeIdentifier(self.MangledName);
		}

		[RubyMethod("==")]
		public static bool IsEqual(ClrName self, [NotNull][DefaultProtocol] string other)
		{
			return self.MangledName == other;
		}

		[RubyMethod("==")]
		public static bool IsEqual(ClrName self, [NotNull] MutableString other)
		{
			return self.MangledName == other.ConvertToString();
		}

		[RubyMethod("==")]
		public static bool IsEqual(RubyContext context, ClrName self, [NotNull] RubySymbol other)
		{
			return other.Equals(GetRubyName(context, self));
		}

		[RubyMethod("==")]
		public static bool IsEqual(ClrName self, [NotNull] ClrName other)
		{
			return self.Equals(other);
		}

		[RubyMethod("<=>")]
		public static int Compare(ClrName self, [DefaultProtocol][NotNull] string other)
		{
			return Math.Sign(string.CompareOrdinal(self.MangledName, other));
		}

		[RubyMethod("<=>")]
		public static int Compare(ClrName self, [NotNull] ClrName other)
		{
			return string.CompareOrdinal(self.MangledName, other.MangledName);
		}

		[RubyMethod("<=>")]
		public static int Compare(RubyContext context, ClrName self, [NotNull] MutableString other)
		{
			return -Math.Sign(other.CompareTo(GetRubyName(context, self)));
		}

		[RubyMethod("<=>")]
		public static int Compare(RubyContext context, ClrName self, [NotNull] RubySymbol other)
		{
			return -Math.Sign(other.CompareTo(GetRubyName(context, self)));
		}

		[RubyMethod("<=>")]
		public static object Compare(BinaryOpStorage comparisonStorage, RespondToStorage respondToStorage, ClrName self, object other)
		{
			return MutableStringOps.Compare(comparisonStorage, respondToStorage, self.MangledName, other);
		}

		[RubyMethod("=~")]
		public static object Match(RubyScope scope, ClrName self, [NotNull] RubyRegex regex)
		{
			return MutableStringOps.Match(scope, GetRubyName(scope.RubyContext, self), regex);
		}

		[RubyMethod("=~")]
		public static object Match(ClrName self, [NotNull] ClrName str)
		{
			throw RubyExceptions.CreateTypeError("type mismatch: ClrName given");
		}

		[RubyMethod("=~")]
		public static object Match(CallSiteStorage<Func<CallSite, RubyScope, object, object, object>> storage, RubyScope scope, ClrName self, object obj)
		{
			return MutableStringOps.Match(storage, scope, GetRubyName(scope.RubyContext, self), obj);
		}

		[RubyMethod("match")]
		public static object Match(CallSiteStorage<Func<CallSite, RubyScope, object, object, object>> storage, RubyScope scope, ClrName self, [NotNull] RubyRegex regex)
		{
			return MutableStringOps.Match(storage, scope, GetRubyName(scope.RubyContext, self), regex);
		}

		[RubyMethod("match")]
		public static object Match(CallSiteStorage<Func<CallSite, RubyScope, object, object, object>> storage, RubyScope scope, ClrName self, [NotNull][DefaultProtocol] MutableString pattern)
		{
			return MutableStringOps.Match(storage, scope, GetRubyName(scope.RubyContext, self), pattern);
		}

		[RubyMethod("empty?", Compatibility = RubyCompatibility.Default)]
		public static bool IsEmpty(ClrName self)
		{
			return self.MangledName.Length == 0;
		}

		[RubyMethod("encoding", Compatibility = RubyCompatibility.Default)]
		public static RubyEncoding GetEncoding(ClrName self)
		{
			return RubyEncoding.UTF8;
		}

		[RubyMethod("size", Compatibility = RubyCompatibility.Default)]
		[RubyMethod("length", Compatibility = RubyCompatibility.Default)]
		public static int GetLength(ClrName self)
		{
			return self.MangledName.Length;
		}

		[RubyMethod("unmangle", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("ruby_to_clr", RubyMethodAttributes.PublicSingleton)]
		public static MutableString Unmangle(RubyClass self, [DefaultProtocol] string rubyName)
		{
			string text = RubyUtils.TryUnmangleName(rubyName);
			if (text == null)
			{
				return null;
			}
			return MutableString.Create(text, self.Context.GetIdentifierEncoding());
		}

		[RubyMethod("clr_to_ruby", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("mangle", RubyMethodAttributes.PublicSingleton)]
		public static MutableString Mangle(RubyClass self, [DefaultProtocol] string clrName)
		{
			string text = RubyUtils.TryMangleName(clrName);
			if (text == null)
			{
				return null;
			}
			return MutableString.Create(text, self.Context.GetIdentifierEncoding());
		}
	}
}
