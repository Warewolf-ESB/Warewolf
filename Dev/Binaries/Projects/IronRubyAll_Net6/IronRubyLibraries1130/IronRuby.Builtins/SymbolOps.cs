using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using IronRuby.Compiler;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("Symbol", Extends = typeof(RubySymbol), Inherits = typeof(object))]
	[HideMethod("==")]
	public static class SymbolOps
	{
		[RubyMethod("id2name")]
		[RubyMethod("to_s")]
		public static MutableString ToString(RubySymbol self)
		{
			return self.String.Clone();
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyContext context, RubySymbol self)
		{
			string text = self.ToString();
			MutableString mutableString = self.String.Clone();
			if (Tokenizer.IsMethodName(text) || Tokenizer.IsConstantName(text) || Tokenizer.IsInstanceVariableName(text) || Tokenizer.IsClassVariableName(text) || Tokenizer.IsGlobalVariableName(text))
			{
				mutableString.Insert(0, ':');
			}
			else
			{
				switch (text)
				{
				case null:
					return MutableString.CreateAscii(":\"\"");
				case "!":
				case "|":
				case "^":
				case "&":
				case "<=>":
				case "==":
				case "===":
				case "=~":
				case "!=":
				case "!~":
				case ">":
				case ">=":
				case "<":
				case "<=":
				case "<<":
				case ">>":
				case "+":
				case "-":
				case "*":
				case "/":
				case "%":
				case "**":
				case "~":
				case "+@":
				case "-@":
				case "[]":
				case "[]=":
				case "`":
				case "$!":
				case "$@":
				case "$,":
				case "$;":
				case "$/":
				case "$\\":
				case "$*":
				case "$$":
				case "$?":
				case "$=":
				case "$:":
				case "$\"":
				case "$<":
				case "$>":
				case "$.":
				case "$~":
				case "$&":
				case "$`":
				case "$'":
				case "$+":
					mutableString.Insert(0, ':');
					break;
				default:
					mutableString.Insert(0, ":\"").Append('"');
					break;
				}
			}
			if (context.RuntimeId != self.RuntimeId)
			{
				mutableString.Append(" @").Append(self.RuntimeId.ToString(CultureInfo.InvariantCulture));
			}
			return mutableString;
		}

		[RubyMethod("to_sym")]
		[RubyMethod("intern", Compatibility = RubyCompatibility.Default)]
		public static RubySymbol ToSymbol(RubySymbol self)
		{
			return self;
		}

		[RubyMethod("to_clr_string")]
		public static string ToClrString(RubySymbol self)
		{
			return self.ToString();
		}

		[RubyMethod("to_proc")]
		public static Proc ToProc(RubyScope scope, RubySymbol self)
		{
			return Proc.CreateMethodInvoker(scope, self.ToString());
		}

		[RubyMethod("<=>")]
		public static int Compare(RubySymbol self, [NotNull] RubySymbol other)
		{
			return Math.Sign(self.CompareTo(other));
		}

		[RubyMethod("<=>")]
		public static int Compare(RubyContext context, RubySymbol self, [NotNull] ClrName other)
		{
			return -ClrNameOps.Compare(context, other, self);
		}

		[RubyMethod("<=>")]
		public static object Compare(RubySymbol self, object other)
		{
			return null;
		}

		[RubyMethod("===")]
		[RubyMethod("==")]
		public static bool Equals(RubySymbol lhs, [NotNull] RubySymbol rhs)
		{
			return lhs.Equals(rhs);
		}

		[RubyMethod("==")]
		[RubyMethod("===")]
		public static bool Equals(RubyContext context, RubySymbol lhs, [NotNull] ClrName rhs)
		{
			return ClrNameOps.IsEqual(context, rhs, lhs);
		}

		[RubyMethod("==")]
		[RubyMethod("===")]
		public static bool Equals(RubySymbol self, object other)
		{
			return false;
		}

		[RubyMethod("casecmp")]
		public static int Casecmp(RubySymbol self, [NotNull] RubySymbol other)
		{
			return MutableStringOps.Casecmp(self.String, other.String);
		}

		[RubyMethod("casecmp")]
		public static int Casecmp(RubySymbol self, [NotNull][DefaultProtocol] MutableString other)
		{
			return MutableStringOps.Casecmp(self.String, other);
		}

		[RubyMethod("=~", Compatibility = RubyCompatibility.Default)]
		public static object Match(RubyScope scope, RubySymbol self, [NotNull] RubyRegex regex)
		{
			return MutableStringOps.Match(scope, self.String.Clone(), regex);
		}

		[RubyMethod("=~", Compatibility = RubyCompatibility.Default)]
		public static object Match(ClrName self, [NotNull] RubySymbol str)
		{
			throw RubyExceptions.CreateTypeError("type mismatch: Symbol given");
		}

		[RubyMethod("=~", Compatibility = RubyCompatibility.Default)]
		public static object Match(CallSiteStorage<Func<CallSite, RubyScope, object, object, object>> storage, RubyScope scope, RubySymbol self, object obj)
		{
			return MutableStringOps.Match(storage, scope, self.String.Clone(), obj);
		}

		[RubyMethod("match", Compatibility = RubyCompatibility.Default)]
		public static object Match(CallSiteStorage<Func<CallSite, RubyScope, object, object, object>> storage, RubyScope scope, RubySymbol self, [NotNull] RubyRegex regex)
		{
			return MutableStringOps.Match(storage, scope, self.String.Clone(), regex);
		}

		[RubyMethod("match", Compatibility = RubyCompatibility.Default)]
		public static object Match(CallSiteStorage<Func<CallSite, RubyScope, object, object, object>> storage, RubyScope scope, RubySymbol self, [DefaultProtocol][NotNull] MutableString pattern)
		{
			return MutableStringOps.Match(storage, scope, self.String.Clone(), pattern);
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static MutableString GetChar(RubySymbol self, [DefaultProtocol] int index)
		{
			return MutableStringOps.GetChar(self.String, index);
		}

		[RubyMethod("slice")]
		[RubyMethod("[]")]
		public static MutableString GetSubstring(RubySymbol self, [DefaultProtocol] int start, [DefaultProtocol] int count)
		{
			return MutableStringOps.GetSubstring(self.String, start, count);
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static MutableString GetSubstring(ConversionStorage<int> fixnumCast, RubySymbol self, [NotNull] Range range)
		{
			return MutableStringOps.GetSubstring(fixnumCast, self.String, range);
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static MutableString GetSubstring(RubySymbol self, [NotNull] MutableString searchStr)
		{
			return MutableStringOps.GetSubstring(self.String, searchStr);
		}

		[RubyMethod("slice")]
		[RubyMethod("[]")]
		public static MutableString GetSubstring(RubyScope scope, RubySymbol self, [NotNull] RubyRegex regex)
		{
			return MutableStringOps.GetSubstring(scope, self.String, regex);
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static MutableString GetSubstring(RubyScope scope, RubySymbol self, [NotNull] RubyRegex regex, [DefaultProtocol] int occurrance)
		{
			return MutableStringOps.GetSubstring(scope, self.String, regex, occurrance);
		}

		[RubyMethod("empty?")]
		public static bool IsEmpty(RubySymbol self)
		{
			return self.IsEmpty;
		}

		[RubyMethod("encoding")]
		public static RubyEncoding GetEncoding(RubySymbol self)
		{
			return self.Encoding;
		}

		[RubyMethod("length")]
		[RubyMethod("size")]
		public static int GetLength(RubySymbol self)
		{
			return self.GetCharCount();
		}

		[RubyMethod("downcase")]
		public static RubySymbol DownCase(RubyContext context, RubySymbol self)
		{
			return context.CreateSymbol(MutableStringOps.DownCase(self.String));
		}

		[RubyMethod("upcase")]
		public static RubySymbol UpCase(RubyContext context, RubySymbol self)
		{
			return context.CreateSymbol(MutableStringOps.UpCase(self.String));
		}

		[RubyMethod("swapcase")]
		public static RubySymbol SwapCase(RubyContext context, RubySymbol self)
		{
			return context.CreateSymbol(MutableStringOps.SwapCase(self.String));
		}

		[RubyMethod("capitalize")]
		public static RubySymbol Capitalize(RubyContext context, RubySymbol self)
		{
			return context.CreateSymbol(MutableStringOps.Capitalize(self.String));
		}

		[RubyMethod("next")]
		[RubyMethod("succ")]
		public static RubySymbol Succ(RubyContext context, RubySymbol self)
		{
			return context.CreateSymbol(MutableStringOps.Succ(self.String));
		}

		[RubyMethod("all_symbols", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetAllSymbols(RubyClass self)
		{
			return self.ImmediateClass.Context.GetAllSymbols();
		}
	}
}
