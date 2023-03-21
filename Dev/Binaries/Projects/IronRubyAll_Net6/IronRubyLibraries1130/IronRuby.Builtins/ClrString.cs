using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("String", DefineIn = typeof(IronRubyOps.Clr))]
	public static class ClrString
	{
		[RubyMethod("%")]
		public static string Format(StringFormatterSiteStorage storage, string self, [NotNull] IList args)
		{
			StringFormatter stringFormatter = new StringFormatter(storage, self, RubyEncoding.UTF8, args);
			return stringFormatter.Format().ToString();
		}

		[RubyMethod("%")]
		public static string Format(StringFormatterSiteStorage storage, ConversionStorage<IList> arrayTryCast, string self, object args)
		{
			return Format(storage, self, Protocols.TryCastToArray(arrayTryCast, args) ?? new object[1] { args });
		}

		[RubyMethod("*")]
		public static string Repeat(string self, [DefaultProtocol] int times)
		{
			if (times < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative argument");
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < times; i++)
			{
				stringBuilder.Append(self);
			}
			return stringBuilder.ToString();
		}

		[RubyMethod("+")]
		public static string Concatenate(string self, [DefaultProtocol][NotNull] MutableString other)
		{
			return self + other.ToString();
		}

		[RubyMethod("<=>")]
		public static int Compare(string self, [NotNull] string other)
		{
			return Math.Sign(string.CompareOrdinal(self, other));
		}

		[RubyMethod("<=>")]
		public static int Compare(string self, [NotNull] MutableString other)
		{
			return -Math.Sign(other.CompareTo(self));
		}

		[RubyMethod("<=>")]
		public static object Compare(BinaryOpStorage comparisonStorage, RespondToStorage respondToStorage, string self, object other)
		{
			return MutableStringOps.Compare(comparisonStorage, respondToStorage, self, other);
		}

		[RubyMethod("==")]
		[RubyMethod("===")]
		public static bool StringEquals(string lhs, [NotNull] string rhs)
		{
			return lhs == rhs;
		}

		[RubyMethod("==")]
		[RubyMethod("===")]
		public static bool StringEquals(string lhs, [NotNull] MutableString rhs)
		{
			return rhs.Equals(lhs);
		}

		[RubyMethod("===")]
		[RubyMethod("==")]
		public static bool Equals(RespondToStorage respondToStorage, BinaryOpStorage equalsStorage, string self, object other)
		{
			return MutableStringOps.Equals(respondToStorage, equalsStorage, self, other);
		}

		[RubyMethod("eql?")]
		public static bool Eql(string lhs, [NotNull] string rhs)
		{
			return lhs == rhs;
		}

		[RubyMethod("eql?")]
		public static bool Eql(string lhs, [NotNull] MutableString rhs)
		{
			return rhs.Equals(lhs);
		}

		[RubyMethod("eql?")]
		public static bool Eql(string lhs, object rhs)
		{
			return false;
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static object GetChar(string self, [DefaultProtocol] int index)
		{
			if (!MutableStringOps.InExclusiveRangeNormalized(self.Length, ref index))
			{
				return null;
			}
			return RubyUtils.CharToObject(self[index]);
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static string GetSubstring(string self, [DefaultProtocol] int start, [DefaultProtocol] int count)
		{
			if (!MutableStringOps.NormalizeSubstringRange(self.Length, ref start, ref count))
			{
				if (start != self.Length)
				{
					return null;
				}
				return self;
			}
			return self.Substring(start, count);
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static string GetSubstring(ConversionStorage<int> fixnumCast, string self, [NotNull] Range range)
		{
			int begin;
			int count;
			if (!MutableStringOps.NormalizeSubstringRange(fixnumCast, range, self.Length, out begin, out count))
			{
				return null;
			}
			if (count >= 0)
			{
				return GetSubstring(self, begin, count);
			}
			return self;
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static string GetSubstring(string self, [NotNull] string searchStr)
		{
			if (self.IndexOf(searchStr, StringComparison.Ordinal) == -1)
			{
				return null;
			}
			return searchStr;
		}

		[RubyMethod("slice")]
		[RubyMethod("[]")]
		public static string GetSubstring(RubyScope scope, string self, [NotNull] RubyRegex regex)
		{
			if (regex.IsEmpty)
			{
				return string.Empty;
			}
			MatchData matchData = RegexpOps.Match(scope, regex, MutableString.Create(self, RubyEncoding.UTF8));
			if (matchData == null)
			{
				return null;
			}
			MutableString value = matchData.GetValue();
			if (value == null)
			{
				return null;
			}
			return value.ToString();
		}

		[RubyMethod("slice")]
		[RubyMethod("[]")]
		public static string GetSubstring(RubyScope scope, string self, [NotNull] RubyRegex regex, [DefaultProtocol] int occurrance)
		{
			if (regex.IsEmpty)
			{
				return string.Empty;
			}
			MatchData matchData = RegexpOps.Match(scope, regex, MutableString.Create(self, RubyEncoding.UTF8));
			if (matchData == null || !RegexpOps.NormalizeGroupIndex(ref occurrance, matchData.GroupCount))
			{
				return null;
			}
			MutableString groupValue = matchData.GetGroupValue(occurrance);
			if (groupValue == null)
			{
				return null;
			}
			return groupValue.ToString();
		}

		[RubyMethod("inspect", RubyMethodAttributes.PublicInstance)]
		public static MutableString Inspect(string self)
		{
			return MutableString.Create(MutableString.AppendUnicodeRepresentation(new StringBuilder().Append('\''), self, MutableString.Escape.Special, 39, -1).Append('\'').ToString(), RubyEncoding.UTF8);
		}

		[RubyMethod("dump", RubyMethodAttributes.PublicInstance)]
		public static MutableString Dump(string self)
		{
			return MutableString.Create(MutableString.AppendUnicodeRepresentation(new StringBuilder().Append('\''), self, MutableString.Escape.NonAscii | MutableString.Escape.Special, 39, -1).Append('\'').ToString(), RubyEncoding.UTF8);
		}

		[RubyMethod("empty?")]
		public static bool IsEmpty(string self)
		{
			return self.Length == 0;
		}

		[RubyMethod("size")]
		public static int GetLength(string self)
		{
			return self.Length;
		}

		[RubyMethod("encoding")]
		public static RubyEncoding GetEncoding(string self)
		{
			return RubyEncoding.UTF8;
		}

		[RubyMethod("include?")]
		public static bool Include(string str, [NotNull][DefaultProtocol] string subString)
		{
			return str.IndexOf(subString, StringComparison.Ordinal) != -1;
		}

		[RubyMethod("insert")]
		public static string Insert(string self, [DefaultProtocol] int start, [NotNull][DefaultProtocol] string value)
		{
			return self.Insert(MutableStringOps.NormalizeInsertIndex(start, self.Length), value);
		}

		[RubyMethod("=~")]
		public static object Match(RubyScope scope, string self, [NotNull] RubyRegex regex)
		{
			return RegexpOps.MatchIndex(scope, regex, MutableString.Create(self, RubyEncoding.UTF8));
		}

		[RubyMethod("=~")]
		public static object Match(string self, [NotNull] string str)
		{
			throw RubyExceptions.CreateTypeError("type mismatch: String given");
		}

		[RubyMethod("=~")]
		public static object Match(CallSiteStorage<Func<CallSite, RubyScope, object, string, object>> storage, RubyScope scope, string self, object obj)
		{
			CallSite<Func<CallSite, RubyScope, object, string, object>> callSite = storage.GetCallSite("=~", new RubyCallSignature(1, (RubyCallFlags)17));
			return callSite.Target(callSite, scope, obj, self);
		}

		[RubyMethod("split")]
		public static RubyArray Split(ConversionStorage<MutableString> stringCast, string self)
		{
			return MutableStringOps.Split(stringCast, MutableString.Create(self, RubyEncoding.UTF8), (MutableString)null, 0);
		}

		[RubyMethod("split")]
		public static RubyArray Split(ConversionStorage<MutableString> stringCast, string self, [DefaultProtocol] string separator, [Optional][DefaultProtocol] int limit)
		{
			return MutableStringOps.Split(stringCast, MutableString.Create(self, RubyEncoding.UTF8), MutableString.Create(separator, RubyEncoding.UTF8), limit);
		}

		[RubyMethod("split")]
		public static RubyArray Split(ConversionStorage<MutableString> stringCast, string self, [NotNull] RubyRegex regexp, [Optional][DefaultProtocol] int limit)
		{
			return MutableStringOps.Split(stringCast, MutableString.Create(self, RubyEncoding.UTF8), regexp, limit);
		}

		[RubyMethod("to_i")]
		public static object ToInteger(string self, [DefaultProtocol] int @base)
		{
			if (@base == 1 || @base < 0 || @base > 36)
			{
				throw RubyExceptions.CreateArgumentError("illegal radix {0}", @base);
			}
			return Tokenizer.ParseInteger(self, @base).ToObject();
		}

		[RubyMethod("hex")]
		public static object ToIntegerHex(string self)
		{
			return Tokenizer.ParseInteger(self, 16).ToObject();
		}

		[RubyMethod("oct")]
		public static object ToIntegerOctal(string self)
		{
			return Tokenizer.ParseInteger(self, 8).ToObject();
		}

		[RubyMethod("to_f")]
		public static double ToDouble(string self)
		{
			double result;
			bool complete;
			if (!Tokenizer.TryParseDouble(self, out result, out complete))
			{
				return 0.0;
			}
			return result;
		}

		[RubyMethod("to_str", RubyMethodAttributes.PublicInstance)]
		[RubyMethod("to_s", RubyMethodAttributes.PublicInstance)]
		public static MutableString ToStr(string self)
		{
			return MutableString.Create(self, RubyEncoding.UTF8);
		}

		[RubyMethod("to_clr_string", RubyMethodAttributes.PublicInstance)]
		public static string ToClrString(string self)
		{
			return self;
		}

		[RubyMethod("intern")]
		[RubyMethod("to_sym")]
		public static RubySymbol ToSymbol(RubyContext context, string self)
		{
			if (self.IndexOf('\0') >= 0)
			{
				throw RubyExceptions.CreateArgumentError("symbol string may not contain '\0'");
			}
			return context.CreateSymbol(self, RubyEncoding.UTF8);
		}

		[RubyMethod("reverse")]
		public static string GetReversed(string self)
		{
			StringBuilder stringBuilder = new StringBuilder(self.Length);
			stringBuilder.Length = self.Length;
			for (int i = 0; i < self.Length; i++)
			{
				stringBuilder[i] = self[self.Length - 1 - i];
			}
			return stringBuilder.ToString();
		}

		[RubyMethod("method_missing", RubyMethodAttributes.PrivateInstance)]
		[RubyStackTraceHidden]
		public static object MethodMissing(RubyScope scope, BlockParam block, string self, [NotNull] RubySymbol name, params object[] args)
		{
			if (name.EndsWith('=') || name.EndsWith('!'))
			{
				throw RubyExceptions.CreateTypeError("Mutating method `{0}' called for an immutable string (System::String)", name);
			}
			return KernelOps.SendMessageOpt(scope, block, ToStr(self), name.ToString(), args);
		}
	}
}
