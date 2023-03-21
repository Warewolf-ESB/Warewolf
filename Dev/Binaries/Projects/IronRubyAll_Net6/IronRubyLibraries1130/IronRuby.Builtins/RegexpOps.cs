using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(Enumerable) })]
	[RubyClass("Regexp", Extends = typeof(RubyRegex), Inherits = typeof(object))]
	public static class RegexpOps
	{
		[RubyConstant]
		public const int IGNORECASE = 1;

		[RubyConstant]
		public const int EXTENDED = 2;

		[RubyConstant]
		public const int MULTILINE = 4;

		internal static bool NormalizeGroupIndex(ref int index, int groupCount)
		{
			if (index < 0)
			{
				index += groupCount;
				if (index == 0)
				{
					return false;
				}
			}
			if (index < 0 || index > groupCount)
			{
				return false;
			}
			return true;
		}

		[RubyConstructor]
		public static RubyRegex Create(RubyClass self, [NotNull] RubyRegex other)
		{
			return new RubyRegex(other);
		}

		[RubyConstructor]
		public static RubyRegex Create(RubyClass self, [NotNull] RubyRegex other, int options, [Optional] object encoding)
		{
			return Create(self, other, (object)options, encoding);
		}

		[RubyConstructor]
		public static RubyRegex Create(RubyClass self, [NotNull] RubyRegex other, object options, [Optional] object encoding)
		{
			ReportParametersIgnoredWarning(self.Context, encoding);
			return new RubyRegex(other);
		}

		[RubyConstructor]
		public static RubyRegex Create(RubyClass self, [NotNull][DefaultProtocol] MutableString pattern, int options, [Optional][DefaultProtocol] MutableString encoding)
		{
			return new RubyRegex(pattern, MakeOptions(options, encoding));
		}

		[RubyConstructor]
		public static RubyRegex Create(RubyClass self, [DefaultProtocol][NotNull] MutableString pattern, [Optional] bool ignoreCase, [Optional][DefaultProtocol] MutableString encoding)
		{
			return new RubyRegex(pattern, MakeOptions(ignoreCase, encoding));
		}

		[RubyMethod("compile", RubyMethodAttributes.PublicSingleton)]
		public static RuleGenerator Compile()
		{
			return RuleGenerators.InstanceConstructor;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyRegex Reinitialize(RubyRegex self, [NotNull] RubyRegex other)
		{
			self.Set(other.Pattern, other.Options);
			return self;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyRegex Reinitialize(RubyContext context, RubyRegex self, [NotNull] RubyRegex regex, int options, [Optional] object encoding)
		{
			ReportParametersIgnoredWarning(context, encoding);
			return Reinitialize(self, regex);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyRegex Reinitialize(RubyContext context, RubyRegex self, [NotNull] RubyRegex regex, object ignoreCase, [Optional] object encoding)
		{
			ReportParametersIgnoredWarning(context, encoding);
			return Reinitialize(self, regex);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyRegex Reinitialize(RubyRegex self, [NotNull][DefaultProtocol] MutableString pattern, int options, [Optional][DefaultProtocol] MutableString encoding)
		{
			self.Set(pattern, MakeOptions(options, encoding));
			return self;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyRegex Reinitialize(RubyRegex self, [NotNull][DefaultProtocol] MutableString pattern, [Optional] bool ignoreCase, [Optional][DefaultProtocol] MutableString encoding)
		{
			self.Set(pattern, MakeOptions(ignoreCase, encoding));
			return self;
		}

		private static void ReportParametersIgnoredWarning(RubyContext context, object encoding)
		{
			context.ReportWarning((encoding != Missing.Value) ? "flags and encoding ignored" : "flags ignored");
		}

		internal static RubyRegexOptions MakeOptions(bool ignoreCase, MutableString encoding)
		{
			return (ignoreCase ? RubyRegexOptions.IgnoreCase : RubyRegexOptions.NONE) | StringToRegexEncoding(encoding);
		}

		internal static RubyRegexOptions MakeOptions(int options, MutableString encoding)
		{
			return (RubyRegexOptions)(options | (int)StringToRegexEncoding(encoding));
		}

		internal static RubyRegexOptions StringToRegexEncoding(MutableString encoding)
		{
			if (MutableString.IsNullOrEmpty(encoding))
			{
				return RubyRegexOptions.NONE;
			}
			switch (encoding.GetChar(0))
			{
			case 'N':
			case 'n':
				return RubyRegexOptions.FIXED;
			case 'E':
			case 'e':
				return RubyRegexOptions.EUC;
			case 'S':
			case 's':
				return RubyRegexOptions.SJIS;
			case 'U':
			case 'u':
				return RubyRegexOptions.UTF8;
			default:
				return RubyRegexOptions.NONE;
			}
		}

		[RubyMethod("to_s")]
		public static MutableString ToS(RubyRegex self)
		{
			return self.ToMutableString();
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyRegex self)
		{
			return self.Inspect();
		}

		[RubyMethod("options")]
		public static int GetOptions(RubyRegex self)
		{
			return (int)self.Options;
		}

		[RubyMethod("encoding", Compatibility = RubyCompatibility.Default)]
		public static RubyEncoding GetEncoding(RubyRegex self)
		{
			return self.Encoding;
		}

		[RubyMethod("casefold?")]
		public static bool IsCaseInsensitive(RubyRegex self)
		{
			return (self.Options & RubyRegexOptions.IgnoreCase) != 0;
		}

		[RubyMethod("match")]
		public static MatchData Match(RubyScope scope, RubyRegex self, [DefaultProtocol] MutableString str)
		{
			return RubyRegex.SetCurrentMatchData(scope, self, str);
		}

		[RubyMethod("hash")]
		public static int GetHash(RubyRegex self)
		{
			return self.GetHashCode();
		}

		[RubyMethod("eql?")]
		[RubyMethod("==")]
		public static bool Equals(RubyRegex self, object other)
		{
			return false;
		}

		[RubyMethod("==")]
		[RubyMethod("eql?")]
		public static bool Equals(RubyContext context, RubyRegex self, [NotNull] RubyRegex other)
		{
			return self.Equals(other);
		}

		[RubyMethod("=~")]
		public static object MatchIndex(RubyScope scope, RubyRegex self, [DefaultProtocol] MutableString str)
		{
			MatchData matchData = RubyRegex.SetCurrentMatchData(scope, self, str);
			if (matchData == null)
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(matchData.Index);
		}

		[RubyMethod("===")]
		public static bool CaseCompare(ConversionStorage<MutableString> stringTryCast, RubyScope scope, RubyRegex self, object obj)
		{
			MutableString mutableString = Protocols.TryCastToString(stringTryCast, obj);
			if (mutableString != null)
			{
				return Match(scope, self, mutableString) != null;
			}
			return false;
		}

		[RubyMethod("~")]
		public static object ImplicitMatch(ConversionStorage<MutableString> stringCast, RubyScope scope, RubyRegex self)
		{
			return MatchIndex(scope, self, Protocols.CastToString(stringCast, scope.GetInnerMostClosureScope().LastInputLine));
		}

		[RubyMethod("source")]
		public static MutableString Source(RubyRegex self)
		{
			return self.Pattern.Clone();
		}

		[RubyMethod("escape", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("quote", RubyMethodAttributes.PublicSingleton)]
		public static MutableString Escape(RubyClass self, [DefaultProtocol][NotNull] MutableString str)
		{
			return RubyRegex.Escape(str).TaintBy(str);
		}

		[RubyMethod("last_match", RubyMethodAttributes.PublicSingleton)]
		public static MatchData LastMatch(RubyScope scope, RubyClass self)
		{
			return scope.GetInnerMostClosureScope().CurrentMatch;
		}

		[RubyMethod("last_match", RubyMethodAttributes.PublicSingleton)]
		public static MutableString LastMatch(RubyScope scope, RubyClass self, [DefaultProtocol] int groupIndex)
		{
			return scope.GetInnerMostClosureScope().CurrentMatch.GetGroupValue(groupIndex);
		}

		[RubyMethod("union", RubyMethodAttributes.PublicSingleton)]
		public static RubyRegex Union(ConversionStorage<MutableString> stringCast, ConversionStorage<IList> toAry, RubyClass self, [NotNull] object obj)
		{
			IList list = Protocols.TryCastToArray(toAry, obj);
			if (list != null)
			{
				return Union(stringCast, list);
			}
			RubyRegex rubyRegex = obj as RubyRegex;
			if (rubyRegex != null)
			{
				return rubyRegex;
			}
			return new RubyRegex(RubyRegex.Escape(Protocols.CastToString(stringCast, obj)), RubyRegexOptions.NONE);
		}

		[RubyMethod("union", RubyMethodAttributes.PublicSingleton)]
		public static RubyRegex Union(ConversionStorage<MutableString> stringCast, RubyClass self, [NotNull] IList objs)
		{
			return Union(stringCast, objs);
		}

		[RubyMethod("union", RubyMethodAttributes.PublicSingleton)]
		public static RubyRegex Union(ConversionStorage<MutableString> stringCast, RubyClass self, [NotNullItems] params object[] objs)
		{
			return Union(stringCast, objs);
		}

		private static RubyRegex Union(ConversionStorage<MutableString> stringCast, ICollection objs)
		{
			if (objs.Count == 0)
			{
				return new RubyRegex(MutableString.CreateAscii("(?!)"), RubyRegexOptions.NONE);
			}
			MutableString mutableString = MutableString.CreateMutable(RubyEncoding.Binary);
			int num = 0;
			foreach (object obj in objs)
			{
				if (num > 0)
				{
					mutableString.Append('|');
				}
				RubyRegex rubyRegex = obj as RubyRegex;
				if (rubyRegex != null)
				{
					if (objs.Count == 1)
					{
						return rubyRegex;
					}
					rubyRegex.AppendTo(mutableString);
				}
				else
				{
					mutableString.Append(RubyRegex.Escape(Protocols.CastToString(stringCast, obj)));
				}
				num++;
			}
			return new RubyRegex(mutableString, RubyRegexOptions.NONE);
		}
	}
}
