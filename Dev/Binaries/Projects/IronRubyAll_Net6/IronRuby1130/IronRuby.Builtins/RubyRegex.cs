using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using IronRuby.Compiler;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	public class RubyRegex : IEquatable<RubyRegex>, IDuplicable
	{
		[DebuggerTypeProxy(typeof(RubyObjectDebugView))]
		[DebuggerDisplay("{_immediateClass.GetDebuggerDisplayValue(this),nq}", Type = "{_immediateClass.GetDebuggerDisplayType(),nq}")]
		public sealed class Subclass : RubyRegex, IRubyObject, IRubyObjectState
		{
			private RubyInstanceData _instanceData;

			private RubyClass _immediateClass;

			public RubyClass ImmediateClass
			{
				get
				{
					return _immediateClass;
				}
				set
				{
					_immediateClass = value;
				}
			}

			public bool IsFrozen
			{
				get
				{
					if (_instanceData != null)
					{
						return _instanceData.IsFrozen;
					}
					return false;
				}
			}

			public bool IsTainted
			{
				get
				{
					if (_instanceData != null)
					{
						return _instanceData.IsTainted;
					}
					return false;
				}
				set
				{
					GetInstanceData().IsTainted = value;
				}
			}

			public bool IsUntrusted
			{
				get
				{
					if (_instanceData != null)
					{
						return _instanceData.IsUntrusted;
					}
					return false;
				}
				set
				{
					GetInstanceData().IsUntrusted = value;
				}
			}

			public Subclass(RubyClass rubyClass)
			{
				ImmediateClass = rubyClass;
			}

			private Subclass(Subclass regex)
				: base(regex)
			{
				ImmediateClass = regex.ImmediateClass.NominalClass;
			}

			protected override RubyRegex Copy()
			{
				return new Subclass(this);
			}

			public RubyInstanceData GetInstanceData()
			{
				return RubyOps.GetInstanceData(ref _instanceData);
			}

			public RubyInstanceData TryGetInstanceData()
			{
				return _instanceData;
			}

			public void Freeze()
			{
				GetInstanceData().Freeze();
			}

			public int BaseGetHashCode()
			{
				return GetHashCode();
			}

			public bool BaseEquals(object other)
			{
				return Equals(other);
			}

			public string BaseToString()
			{
				return ToString();
			}
		}

		private const int EndOfPattern = -1;

		private MutableString _pattern;

		private RubyRegexOptions _options;

		private bool _hasGAnchor;

		private Regex _cachedRegex;

		private RubyRegexOptions _cachedKCode;

		public bool IsEmpty
		{
			get
			{
				return _pattern.IsEmpty;
			}
		}

		public RubyRegexOptions Options
		{
			get
			{
				return _options;
			}
		}

		public RubyEncoding Encoding
		{
			get
			{
				return _pattern.Encoding;
			}
		}

		public MutableString Pattern
		{
			get
			{
				return _pattern;
			}
		}

		public RubyRegex()
		{
			_pattern = MutableString.CreateEmpty();
			_options = RubyRegexOptions.NONE;
		}

		public RubyRegex(MutableString pattern)
			: this(pattern, RubyRegexOptions.NONE)
		{
		}

		public RubyRegex(MutableString pattern, RubyRegexOptions options)
		{
			Set(pattern, options);
		}

		public RubyRegex(RubyRegex regex)
		{
			ContractUtils.RequiresNotNull(regex, "regex");
			Set(regex.Pattern, regex.Options);
		}

		public void Set(MutableString pattern, RubyRegexOptions options)
		{
			ContractUtils.RequiresNotNull(pattern, "pattern");
			_options = options & ~RubyRegexOptions.Once;
			RubyEncoding regexEncoding = RubyEncoding.GetRegexEncoding(options);
			if (regexEncoding != null)
			{
				_pattern = MutableString.CreateBinary(pattern.ToByteArray(), regexEncoding ?? RubyEncoding.Binary).Freeze();
			}
			else
			{
				_pattern = pattern.PrepareForCharacterRead().Clone().Freeze();
			}
			TransformPattern(regexEncoding, options & RubyRegexOptions.EncodingMask);
		}

		protected virtual RubyRegex Copy()
		{
			return new RubyRegex(this);
		}

		object IDuplicable.Duplicate(RubyContext context, bool copySingletonMembers)
		{
			RubyRegex rubyRegex = Copy();
			context.CopyInstanceData(this, rubyRegex, copySingletonMembers);
			return rubyRegex;
		}

		private Regex Transform(ref RubyEncoding encoding, MutableString input, int start, out string strInput)
		{
			ContractUtils.RequiresNotNull(input, "input");
			RubyRegexOptions rubyRegexOptions = _options & RubyRegexOptions.EncodingMask;
			if (rubyRegexOptions != 0)
			{
				encoding = _pattern.Encoding;
			}
			else
			{
				rubyRegexOptions = RubyRegexOptions.NONE;
			}
			if (rubyRegexOptions != 0)
			{
				if (HasEscapedNonAsciiBytes(_pattern))
				{
					encoding = RubyEncoding.Binary;
					rubyRegexOptions = RubyRegexOptions.NONE;
				}
				strInput = ForceEncoding(input, encoding.Encoding, start);
			}
			else
			{
				_pattern.RequireCompatibleEncoding(input);
				input.PrepareForCharacterRead();
				strInput = input.ConvertToString();
			}
			return TransformPattern(encoding, rubyRegexOptions);
		}

		private Regex TransformPattern(RubyEncoding encoding, RubyRegexOptions kc)
		{
			if (_cachedRegex != null && kc == _cachedKCode)
			{
				return _cachedRegex;
			}
			string rubyPattern = ((kc == RubyRegexOptions.NONE && encoding != RubyEncoding.Binary) ? _pattern.ConvertToString() : _pattern.ToString(encoding.Encoding));
			Regex regex;
			try
			{
				regex = new Regex(RegexpTransformer.Transform(rubyPattern, _options, out _hasGAnchor), ToClrOptions(_options));
			}
			catch (Exception ex)
			{
				throw new RegexpError(ex.Message);
			}
			_cachedKCode = kc;
			_cachedRegex = regex;
			return regex;
		}

		private static bool HasEscapedNonAsciiBytes(MutableString pattern)
		{
			int num = 0;
			int byteCount = pattern.GetByteCount();
			while (num < byteCount - 2)
			{
				int @byte = pattern.GetByte(num++);
				if (@byte != 92)
				{
					continue;
				}
				@byte = pattern.GetByte(num++);
				switch (@byte)
				{
				case 120:
				{
					int num4 = Tokenizer.ToDigit(PeekByte(pattern, byteCount, num++));
					if (num4 < 16)
					{
						int num5 = Tokenizer.ToDigit(PeekByte(pattern, byteCount, num++));
						if (num5 < 16)
						{
							return num4 * 16 + num5 >= 128;
						}
					}
					break;
				}
				case 50:
				case 51:
				case 52:
				case 53:
				case 54:
				case 55:
				{
					int num2 = Tokenizer.ToDigit(PeekByte(pattern, byteCount, num++));
					if (num2 < 8)
					{
						int num3 = Tokenizer.ToDigit(@byte) * 8 + num2;
						num2 = Tokenizer.ToDigit(PeekByte(pattern, byteCount, num++));
						if (num2 < 8)
						{
							num3 = num3 * 8 + num2;
						}
						return num3 >= 128;
					}
					break;
				}
				}
			}
			return false;
		}

		private static int PeekByte(MutableString str, int length, int i)
		{
			if (i >= length)
			{
				return -1;
			}
			return str.GetByte(i);
		}

		private static string ForceEncoding(MutableString input, Encoding encoding, int start)
		{
			int byteCount = input.GetByteCount();
			if (start < 0)
			{
				start += byteCount;
			}
			if (start < 0)
			{
				return null;
			}
			if (start > byteCount)
			{
				return null;
			}
			return input.ToString(encoding, start, byteCount - start);
		}

		public bool Equals(RubyRegex other)
		{
			if (!object.ReferenceEquals(this, other))
			{
				if (other != null && _options.Equals(other._options))
				{
					return _pattern.Equals(other._pattern);
				}
				return false;
			}
			return true;
		}

		public override bool Equals(object other)
		{
			return Equals(other as RubyRegex);
		}

		public override int GetHashCode()
		{
			return _pattern.GetHashCode() ^ _options.GetHashCode();
		}

		public static RegexOptions ToClrOptions(RubyRegexOptions options)
		{
			RegexOptions regexOptions = RegexOptions.Multiline | RegexOptions.CultureInvariant;
			if ((options & RubyRegexOptions.IgnoreCase) != 0)
			{
				regexOptions |= RegexOptions.IgnoreCase;
			}
			if ((options & RubyRegexOptions.Extended) != 0)
			{
				regexOptions |= RegexOptions.IgnorePatternWhitespace;
			}
			if ((options & RubyRegexOptions.Multiline) != 0)
			{
				regexOptions |= RegexOptions.Singleline;
			}
			return regexOptions;
		}

		public MatchData Match(MutableString input)
		{
			RubyEncoding encoding = null;
			string strInput;
			return MatchData.Create(Transform(ref encoding, input, 0, out strInput).Match(strInput), input, true, strInput);
		}

		public MatchData Match(MutableString input, int start, bool freezeInput)
		{
			RubyEncoding encoding = null;
			string strInput;
			Regex regex = Transform(ref encoding, input, start, out strInput);
			Match match;
			if (encoding != null)
			{
				if (strInput == null)
				{
					return null;
				}
				match = regex.Match(strInput, 0);
			}
			else
			{
				if (start < 0)
				{
					start += strInput.Length;
				}
				if (start < 0 || start > strInput.Length)
				{
					return null;
				}
				match = regex.Match(strInput, start);
			}
			return MatchData.Create(match, input, freezeInput, strInput);
		}

		public MatchData LastMatch(MutableString input)
		{
			return LastMatch(input, int.MaxValue);
		}

		public MatchData LastMatch(MutableString input, int start)
		{
			RubyEncoding encoding = null;
			string strInput;
			Regex regex = Transform(ref encoding, input, 0, out strInput);
			if (encoding != null)
			{
				int count;
				byte[] byteArray = input.GetByteArray(out count);
				if (start < 0)
				{
					start += count;
				}
				start = ((start >= count) ? strInput.Length : (encoding.Encoding.GetCharCount(byteArray, 0, start + 1) - 1));
			}
			else
			{
				if (start < 0)
				{
					start += strInput.Length;
				}
				if (start > strInput.Length)
				{
					start = strInput.Length;
				}
			}
			Match match;
			if (_hasGAnchor)
			{
				match = regex.Match(strInput, start);
			}
			else
			{
				match = LastMatch(regex, strInput, start);
				if (match == null)
				{
					return null;
				}
			}
			return MatchData.Create(match, input, true, strInput);
		}

		private static Match LastMatch(Regex regex, string input, int start)
		{
			Match result = null;
			int num = 0;
			int num2 = start;
			while (num <= num2)
			{
				int num3 = (num + num2) / 2;
				Match match = regex.Match(input, num3);
				if (match.Success && match.Index <= num2)
				{
					result = match;
					num = match.Index + 1;
				}
				else
				{
					num2 = num3 - 1;
				}
			}
			return result;
		}

		public IList<MatchData> Matches(MutableString input, bool inputMayMutate)
		{
			RubyEncoding encoding = null;
			string strInput;
			MatchCollection matchCollection = Transform(ref encoding, input, 0, out strInput).Matches(strInput);
			MatchData[] array = new MatchData[matchCollection.Count];
			if (array.Length > 0 && inputMayMutate)
			{
				input = input.Clone().Freeze();
			}
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = MatchData.Create(matchCollection[i], input, false, strInput);
			}
			return array;
		}

		public IList<MatchData> Matches(MutableString input)
		{
			return Matches(input, true);
		}

		public MutableString[] Split(MutableString input)
		{
			RubyEncoding encoding = null;
			string strInput;
			return MutableString.MakeArray(Transform(ref encoding, input, 0, out strInput).Split(strInput), encoding ?? input.Encoding);
		}

		public MutableString[] Split(MutableString input, int count)
		{
			RubyEncoding encoding = null;
			string strInput;
			return MutableString.MakeArray(Transform(ref encoding, input, 0, out strInput).Split(strInput, count), encoding ?? input.Encoding);
		}

		public static MatchData SetCurrentMatchData(RubyScope scope, RubyRegex regex, MutableString str)
		{
			return scope.GetInnerMostClosureScope().CurrentMatch = ((str != null) ? regex.Match(str) : null);
		}

		public override string ToString()
		{
			return ToMutableString().ToString();
		}

		public MutableString ToMutableString()
		{
			return AppendTo(MutableString.CreateMutable(RubyEncoding.Binary));
		}

		public MutableString Inspect()
		{
			MutableString mutableString = MutableString.CreateMutable(RubyEncoding.Binary);
			mutableString.Append('/');
			AppendEscapeForwardSlash(mutableString, _pattern);
			mutableString.Append('/');
			AppendOptionString(mutableString, true);
			return mutableString;
		}

		public MutableString AppendTo(MutableString result)
		{
			result.Append("(?");
			if (AppendOptionString(result, true) < 3)
			{
				result.Append('-');
			}
			AppendOptionString(result, false);
			result.Append(':');
			AppendEscapeForwardSlash(result, _pattern);
			result.Append(')');
			return result;
		}

		private int AppendOptionString(MutableString result, bool enabled)
		{
			int num = 0;
			RubyRegexOptions options = Options;
			if ((options & RubyRegexOptions.Multiline) != 0 == enabled)
			{
				result.Append('m');
				num++;
			}
			if ((options & RubyRegexOptions.IgnoreCase) != 0 == enabled)
			{
				result.Append('i');
				num++;
			}
			if ((options & RubyRegexOptions.Extended) != 0 == enabled)
			{
				result.Append('x');
				num++;
			}
			return num;
		}

		private static int SkipToUnescapedForwardSlash(MutableString pattern, int patternLength, int i)
		{
			while (i < patternLength)
			{
				i = pattern.IndexOf('/', i);
				if (i <= 0)
				{
					return i;
				}
				if (pattern.GetChar(i - 1) != '\\')
				{
					return i;
				}
				i++;
			}
			return -1;
		}

		private static MutableString AppendEscapeForwardSlash(MutableString result, MutableString pattern)
		{
			int num = 0;
			int charCount = pattern.GetCharCount();
			for (int num2 = SkipToUnescapedForwardSlash(pattern, charCount, 0); num2 >= 0; num2 = SkipToUnescapedForwardSlash(pattern, charCount, num2 + 1))
			{
				result.Append(pattern, num, num2 - num);
				result.Append('\\');
				num = num2;
			}
			result.Append(pattern, num, charCount - num);
			return result;
		}

		public static MutableString Escape(MutableString str)
		{
			return str.EscapeRegularExpression();
		}

		private static int SkipNonSpecial(string pattern, int i, out char escaped)
		{
			while (i < pattern.Length)
			{
				char c = pattern[i];
				switch (c)
				{
				case ' ':
				case '#':
				case '$':
				case '(':
				case ')':
				case '*':
				case '+':
				case '-':
				case '.':
				case '?':
				case '[':
				case '\\':
				case ']':
				case '^':
				case '{':
				case '|':
				case '}':
					escaped = c;
					return i;
				case '\t':
					escaped = 't';
					return i;
				case '\n':
					escaped = 'n';
					return i;
				case '\r':
					escaped = 'r';
					return i;
				case '\f':
					escaped = 'f';
					return i;
				}
				i++;
			}
			escaped = '\0';
			return -1;
		}

		internal static string Escape(string pattern)
		{
			StringBuilder stringBuilder = EscapeToStringBuilder(pattern);
			if (stringBuilder == null)
			{
				return pattern;
			}
			return stringBuilder.ToString();
		}

		internal static StringBuilder EscapeToStringBuilder(string pattern)
		{
			int num = 0;
			char escaped;
			int num2 = SkipNonSpecial(pattern, 0, out escaped);
			if (num2 == -1)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder(pattern.Length + 1);
			do
			{
				stringBuilder.Append(pattern, num, num2 - num);
				stringBuilder.Append('\\');
				stringBuilder.Append(escaped);
				num2++;
				num = num2;
				num2 = SkipNonSpecial(pattern, num2, out escaped);
			}
			while (num2 >= 0);
			stringBuilder.Append(pattern, num, pattern.Length - num);
			return stringBuilder;
		}
	}
}
