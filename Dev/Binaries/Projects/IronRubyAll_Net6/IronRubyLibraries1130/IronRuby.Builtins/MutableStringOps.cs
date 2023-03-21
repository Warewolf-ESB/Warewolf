using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(Comparable) })]
	[RubyClass("String", Extends = typeof(MutableString), Inherits = typeof(object))]
	public class MutableStringOps
	{
		public class IntervalParser
		{
			private readonly MutableString _range;

			private int _pos;

			private bool _rangeStarted;

			private int _startRange;

			public IntervalParser(MutableString range)
			{
				_range = range;
				_pos = 0;
				_rangeStarted = false;
			}

			public int PeekChar()
			{
				if (_pos < _range.Length)
				{
					return _range.GetChar(_pos);
				}
				return -1;
			}

			public int GetChar()
			{
				if (_pos < _range.Length)
				{
					return _range.GetChar(_pos++);
				}
				return -1;
			}

			public int NextToken()
			{
				int num = GetChar();
				if (num == 92)
				{
					switch (PeekChar())
					{
					case 120:
					{
						_pos++;
						int num2 = Tokenizer.ToDigit(GetChar());
						int num3 = Tokenizer.ToDigit(GetChar());
						if (num2 >= 16)
						{
							throw RubyExceptions.CreateArgumentError("Invalid escape character syntax");
						}
						num = ((num3 < 16) ? ((num2 << 4) + num3) : num2);
						break;
					}
					case 116:
						num = 9;
						break;
					case 110:
						num = 10;
						break;
					case 114:
						num = 13;
						break;
					case 118:
						num = 11;
						break;
					case 92:
						num = 92;
						break;
					}
				}
				return num;
			}

			public MutableString ParseSequence()
			{
				_pos = 0;
				MutableString mutableString = MutableString.CreateBinary();
				if (_range.Length == 0)
				{
					return mutableString;
				}
				bool flag = false;
				if (_range.StartsWith('^'))
				{
					if (_range.GetLength() == 1)
					{
						mutableString.Append('^');
						return mutableString;
					}
					flag = true;
					_pos = 1;
				}
				BitArray bitArray = new BitArray(256);
				bitArray.Not();
				int num;
				while ((num = NextToken()) != -1)
				{
					if (_rangeStarted)
					{
						if (_startRange <= num)
						{
							for (int i = _startRange; i <= num; i++)
							{
								if (flag)
								{
									bitArray.Set(i, false);
								}
								else
								{
									mutableString.Append((byte)i);
								}
							}
						}
						_rangeStarted = false;
						continue;
					}
					int num2 = PeekChar();
					if (num2 == 45)
					{
						if (_pos == _range.Length - 1)
						{
							if (flag)
							{
								bitArray.Set(num, false);
								bitArray.Set(45, false);
							}
							else
							{
								mutableString.Append((byte)num);
								mutableString.Append('-');
							}
							break;
						}
						_startRange = num;
						if (_rangeStarted)
						{
							if (flag)
							{
								bitArray.Set(45, false);
							}
							else
							{
								mutableString.Append('-');
							}
							_rangeStarted = false;
						}
						else
						{
							_rangeStarted = true;
						}
						_pos++;
					}
					else if (flag)
					{
						bitArray.Set(num, false);
					}
					else
					{
						mutableString.Append((byte)num);
					}
				}
				if (flag)
				{
					for (int j = 0; j < 256; j++)
					{
						if (bitArray.Get(j))
						{
							mutableString.Append((byte)j);
						}
					}
				}
				return mutableString;
			}

			public BitArray Parse()
			{
				_pos = 0;
				BitArray bitArray = new BitArray(256);
				if (_range.Length == 0)
				{
					return bitArray;
				}
				bool flag = false;
				if (_range.StartsWith('^'))
				{
					if (_range.GetLength() == 1)
					{
						bitArray.Set(94, true);
						return bitArray;
					}
					flag = true;
					_pos = 1;
					bitArray.Not();
				}
				int num;
				while ((num = NextToken()) != -1)
				{
					if (_rangeStarted)
					{
						if (_startRange <= num)
						{
							for (int i = _startRange; i <= num; i++)
							{
								bitArray.Set(i, !flag);
							}
						}
						_rangeStarted = false;
						continue;
					}
					int num2 = PeekChar();
					if (num2 == 45)
					{
						if (_pos == _range.Length - 1)
						{
							bitArray.Set(num, !flag);
							bitArray.Set(45, !flag);
							break;
						}
						_startRange = num;
						if (_rangeStarted)
						{
							bitArray.Set(45, !flag);
							_rangeStarted = false;
						}
						else
						{
							_rangeStarted = true;
						}
						_pos++;
					}
					else
					{
						bitArray.Set(num, !flag);
					}
				}
				return bitArray;
			}
		}

		public class RangeParser
		{
			private readonly MutableString[] _ranges;

			public RangeParser(params MutableString[] ranges)
			{
				ContractUtils.RequiresNotNull(ranges, "ranges");
				_ranges = ranges;
			}

			public BitArray Parse()
			{
				BitArray bitArray = new IntervalParser(_ranges[0]).Parse();
				for (int i = 1; i < _ranges.Length; i++)
				{
					bitArray.And(new IntervalParser(_ranges[i]).Parse());
				}
				return bitArray;
			}
		}

		private static readonly MutableString _DefaultPadding = MutableString.CreateAscii(" ").Freeze();

		internal static readonly MutableString DefaultLineSeparator = MutableString.CreateAscii("\n").Freeze();

		internal static readonly MutableString DefaultParagraphSeparator = MutableString.CreateAscii("\n\n").Freeze();

		private static char[] _WhiteSpaceSeparators = new char[5] { ' ', '\n', '\r', '\t', '\v' };

		[RubyConstructor]
		public static MutableString Create(RubyClass self)
		{
			return MutableString.CreateEmpty();
		}

		[RubyConstructor]
		public static MutableString Create(RubyClass self, [NotNull][DefaultProtocol] MutableString value)
		{
			return MutableString.Create(value);
		}

		[RubyConstructor]
		public static MutableString Create(RubyClass self, [NotNull] byte[] value)
		{
			return MutableString.CreateBinary(value);
		}

		internal static bool InExclusiveRangeNormalized(int length, ref int index)
		{
			if (index < 0)
			{
				index += length;
			}
			if (index >= 0)
			{
				return index < length;
			}
			return false;
		}

		private static bool InInclusiveRangeNormalized(MutableString str, ref int index)
		{
			if (index < 0)
			{
				index += str.Length;
			}
			if (index >= 0)
			{
				return index <= str.Length;
			}
			return false;
		}

		internal static bool NormalizeSubstringRange(ConversionStorage<int> fixnumCast, Range range, int length, out int begin, out int count)
		{
			begin = Protocols.CastToFixnum(fixnumCast, range.Begin);
			int index = Protocols.CastToFixnum(fixnumCast, range.End);
			begin = IListOps.NormalizeIndex(length, begin);
			if (begin < 0 || begin > length)
			{
				count = 0;
				return false;
			}
			index = IListOps.NormalizeIndex(length, index);
			count = (range.ExcludeEnd ? (index - begin) : (index - begin + 1));
			return true;
		}

		internal static bool NormalizeSubstringRange(int length, ref int start, ref int count)
		{
			if (start < 0)
			{
				start += length;
			}
			if (start < 0 || start >= length || count < 0)
			{
				return false;
			}
			if (start + count > length)
			{
				count = length - start;
			}
			return true;
		}

		internal static int NormalizeInsertIndex(int index, int length)
		{
			int num = ((index < 0) ? (index + length + 1) : index);
			if (num > length || num < 0)
			{
				throw RubyExceptions.CreateIndexError("index {0} out of string", index);
			}
			return num;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static MutableString Reinitialize(MutableString self)
		{
			self.RequireNotFrozen();
			return self;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		public static MutableString Reinitialize(MutableString self, [NotNull][DefaultProtocol] MutableString other)
		{
			if (object.ReferenceEquals(self, other))
			{
				return self;
			}
			self.Clear();
			self.Append(other);
			return self.TaintBy(other);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static MutableString Reinitialize(MutableString self, [NotNull] byte[] other)
		{
			self.Clear();
			self.Append(other);
			return self;
		}

		[RubyMethod("%")]
		public static MutableString Format(StringFormatterSiteStorage storage, MutableString self, [NotNull] IList args)
		{
			StringFormatter stringFormatter = new StringFormatter(storage, self.ConvertToString(), self.Encoding, args);
			return stringFormatter.Format().TaintBy(self);
		}

		[RubyMethod("%")]
		public static MutableString Format(StringFormatterSiteStorage storage, ConversionStorage<IList> arrayTryCast, MutableString self, object args)
		{
			return Format(storage, self, Protocols.TryCastToArray(arrayTryCast, args) ?? new object[1] { args });
		}

		[RubyMethod("*")]
		public static MutableString Repeat(MutableString self, [DefaultProtocol] int times)
		{
			if (times < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative argument");
			}
			return self.CreateInstance().TaintBy(self).AppendMultiple(self, times);
		}

		[RubyMethod("+")]
		public static MutableString Concatenate(MutableString self, [NotNull][DefaultProtocol] MutableString other)
		{
			return self.Concat(other).TaintBy(self).TaintBy(other);
		}

		[RubyMethod("+")]
		public static MutableString Concatenate(MutableString self, [NotNull] RubySymbol other)
		{
			return self.Concat(other.String).TaintBy(self).TaintBy(other);
		}

		[RubyMethod("<<")]
		[RubyMethod("concat")]
		public static MutableString Append(MutableString self, [DefaultProtocol][NotNull] MutableString other)
		{
			return self.Append(other).TaintBy(other);
		}

		[RubyMethod("<<")]
		[RubyMethod("concat")]
		public static MutableString Append(MutableString self, int c)
		{
			return self.Append(Integer.ToChr(self.Encoding, self.Encoding, c));
		}

		[RubyMethod("<=>")]
		public static int Compare(MutableString self, [NotNull] MutableString other)
		{
			return Math.Sign(self.CompareTo(other));
		}

		[RubyMethod("<=>")]
		public static int Compare(MutableString self, [NotNull] string other)
		{
			return Math.Sign(self.CompareTo(other));
		}

		[RubyMethod("<=>")]
		public static object Compare(BinaryOpStorage comparisonStorage, RespondToStorage respondToStorage, object self, object other)
		{
			if (Protocols.RespondTo(respondToStorage, other, "to_str") && Protocols.RespondTo(respondToStorage, other, "<=>"))
			{
				CallSite<Func<CallSite, object, object, object>> callSite = comparisonStorage.GetCallSite("<=>");
				object obj = Integer.TryUnaryMinus(callSite.Target(callSite, other, self));
				if (obj == null)
				{
					throw RubyExceptions.CreateTypeError("{0} can't be coerced into Fixnum", comparisonStorage.Context.GetClassDisplayName(obj));
				}
				return obj;
			}
			return null;
		}

		[RubyMethod("eql?")]
		public static bool Eql(MutableString lhs, [NotNull] MutableString rhs)
		{
			return lhs.Equals(rhs);
		}

		[RubyMethod("eql?")]
		public static bool Eql(MutableString lhs, [NotNull] string rhs)
		{
			return lhs.Equals(rhs);
		}

		[RubyMethod("eql?")]
		public static bool Eql(MutableString lhs, object rhs)
		{
			return false;
		}

		[RubyMethod("===")]
		[RubyMethod("==")]
		public static bool StringEquals(MutableString lhs, [NotNull] MutableString rhs)
		{
			return lhs.Equals(rhs);
		}

		[RubyMethod("==")]
		[RubyMethod("===")]
		public static bool StringEquals(MutableString lhs, [NotNull] string rhs)
		{
			return lhs.Equals(rhs);
		}

		[RubyMethod("==")]
		[RubyMethod("===")]
		public static bool Equals(RespondToStorage respondToStorage, BinaryOpStorage equalsStorage, object self, object other)
		{
			if (!Protocols.RespondTo(respondToStorage, other, "to_str"))
			{
				return false;
			}
			CallSite<Func<CallSite, object, object, object>> callSite = equalsStorage.GetCallSite("==");
			return Protocols.IsTrue(callSite.Target(callSite, other, self));
		}

		[RubyMethod("slice!")]
		public static object RemoveCharInPlace(RubyContext context, MutableString self, [DefaultProtocol] int index)
		{
			if (!InExclusiveRangeNormalized(self.GetByteCount(), ref index))
			{
				return null;
			}
			int @byte = self.GetByte(index);
			self.Remove(index, 1);
			return @byte;
		}

		[RubyMethod("slice!")]
		public static MutableString RemoveSubstringInPlace(MutableString self, [DefaultProtocol] int start, [DefaultProtocol] int length)
		{
			if (length < 0)
			{
				return null;
			}
			if (!InInclusiveRangeNormalized(self, ref start))
			{
				return null;
			}
			if (start + length > self.Length)
			{
				length = self.Length - start;
			}
			MutableString result = self.CreateInstance().Append(self, start, length).TaintBy(self);
			self.Remove(start, length);
			return result;
		}

		[RubyMethod("slice!")]
		public static MutableString RemoveSubstringInPlace(ConversionStorage<int> fixnumCast, MutableString self, [NotNull] Range range)
		{
			int index = Protocols.CastToFixnum(fixnumCast, range.Begin);
			int index2 = Protocols.CastToFixnum(fixnumCast, range.End);
			if (!InInclusiveRangeNormalized(self, ref index))
			{
				return null;
			}
			index2 = IListOps.NormalizeIndex(self.Length, index2);
			int num = (range.ExcludeEnd ? (index2 - index) : (index2 - index + 1));
			if (num >= 0)
			{
				return RemoveSubstringInPlace(self, index, num);
			}
			return self.CreateInstance();
		}

		[RubyMethod("slice!")]
		public static MutableString RemoveSubstringInPlace(RubyScope scope, MutableString self, [NotNull] RubyRegex regex)
		{
			if (regex.IsEmpty)
			{
				return self.Clone().TaintBy(regex, scope);
			}
			MatchData matchData = RegexpOps.Match(scope, regex, self);
			if (matchData == null)
			{
				return null;
			}
			return RemoveSubstringInPlace(self, matchData.Index, matchData.Length).TaintBy(regex, scope);
		}

		[RubyMethod("slice!")]
		public static MutableString RemoveSubstringInPlace(RubyScope scope, MutableString self, [NotNull] RubyRegex regex, [DefaultProtocol] int occurrance)
		{
			if (regex.IsEmpty)
			{
				return self.Clone().TaintBy(regex, scope);
			}
			MatchData matchData = RegexpOps.Match(scope, regex, self);
			if (matchData == null || !RegexpOps.NormalizeGroupIndex(ref occurrance, matchData.GroupCount))
			{
				return null;
			}
			if (!matchData.GroupSuccess(occurrance))
			{
				return null;
			}
			return RemoveSubstringInPlace(self, matchData.GetGroupStart(occurrance), matchData.GetGroupLength(occurrance)).TaintBy(regex, scope);
		}

		[RubyMethod("slice!")]
		public static MutableString RemoveSubstringInPlace(MutableString self, [NotNull] MutableString searchStr)
		{
			if (searchStr.IsEmpty)
			{
				return searchStr.Clone();
			}
			int num = self.IndexOf(searchStr);
			if (num < 0)
			{
				return null;
			}
			RemoveSubstringInPlace(self, num, searchStr.Length);
			return searchStr.Clone();
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static MutableString GetChar(MutableString self, [DefaultProtocol] int index)
		{
			if (!InExclusiveRangeNormalized(self.GetCharCount(), ref index))
			{
				return null;
			}
			return self.GetSlice(index, 1);
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static MutableString GetSubstring(MutableString self, [DefaultProtocol] int start, [DefaultProtocol] int count)
		{
			int charCount = self.GetCharCount();
			if (!NormalizeSubstringRange(charCount, ref start, ref count))
			{
				if (start != charCount)
				{
					return null;
				}
				return self.CreateInstance().TaintBy(self);
			}
			return self.CreateInstance().Append(self, start, count).TaintBy(self);
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static MutableString GetSubstring(ConversionStorage<int> fixnumCast, MutableString self, [NotNull] Range range)
		{
			int begin;
			int count;
			if (!NormalizeSubstringRange(fixnumCast, range, self.GetCharCount(), out begin, out count))
			{
				return null;
			}
			if (count >= 0)
			{
				return GetSubstring(self, begin, count);
			}
			return self.CreateInstance().TaintBy(self);
		}

		[RubyMethod("slice")]
		[RubyMethod("[]")]
		public static MutableString GetSubstring(MutableString self, [NotNull] MutableString searchStr)
		{
			if (self.IndexOf(searchStr) == -1)
			{
				return null;
			}
			return searchStr.Clone();
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static MutableString GetSubstring(RubyScope scope, MutableString self, [NotNull] RubyRegex regex)
		{
			if (regex.IsEmpty)
			{
				return self.CreateInstance().TaintBy(self).TaintBy(regex, scope);
			}
			MatchData matchData = RegexpOps.Match(scope, regex, self);
			if (matchData == null)
			{
				return null;
			}
			return self.CreateInstance().TaintBy(self).Append(self, matchData.Index, matchData.Length)
				.TaintBy(regex, scope);
		}

		[RubyMethod("slice")]
		[RubyMethod("[]")]
		public static MutableString GetSubstring(RubyScope scope, MutableString self, [NotNull] RubyRegex regex, [DefaultProtocol] int occurrance)
		{
			if (regex.IsEmpty)
			{
				return self.CreateInstance().TaintBy(self).TaintBy(regex, scope);
			}
			MatchData matchData = RegexpOps.Match(scope, regex, self);
			if (matchData == null || !RegexpOps.NormalizeGroupIndex(ref occurrance, matchData.GroupCount))
			{
				return null;
			}
			MutableString mutableString = matchData.AppendGroupValue(occurrance, self.CreateInstance());
			if (mutableString == null)
			{
				return null;
			}
			return mutableString.TaintBy(regex, scope);
		}

		[RubyMethod("getbyte")]
		public static object GetByte(MutableString self, [DefaultProtocol] int index)
		{
			if (!InExclusiveRangeNormalized(self.GetByteCount(), ref index))
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(self.GetByte(index));
		}

		[RubyMethod("ord")]
		public static int Ord(MutableString str)
		{
			if (str.IsEmpty)
			{
				throw RubyExceptions.CreateArgumentError("empty string");
			}
			char @char = str.GetChar(0);
			if (!char.IsSurrogate(@char))
			{
				return @char;
			}
			char char2;
			if (Tokenizer.IsHighSurrogate(@char) && str.GetCharCount() > 1 && Tokenizer.IsLowSurrogate(char2 = str.GetChar(1)))
			{
				return Tokenizer.ToCodePoint(@char, char2);
			}
			throw RubyExceptions.CreateArgumentError("invalid byte sequence in {0}", str.Encoding);
		}

		[RubyMethod("setbyte")]
		public static MutableString SetByte(MutableString self, [DefaultProtocol] int index, [DefaultProtocol] int value)
		{
			self.SetByte(index, (byte)value);
			return self;
		}

		[RubyMethod("[]=")]
		public static MutableString ReplaceCharacter(MutableString self, [DefaultProtocol] int index, [DefaultProtocol][NotNull] MutableString value)
		{
			index = ((index < 0) ? (index + self.Length) : index);
			if (index < 0 || index >= self.Length)
			{
				throw RubyExceptions.CreateIndexError("index {0} out of string", index);
			}
			if (value.IsEmpty)
			{
				self.Remove(index, 1).TaintBy(value);
				return MutableString.CreateEmpty();
			}
			self.Replace(index, 1, value).TaintBy(value);
			return value;
		}

		[RubyMethod("[]=")]
		public static int SetCharacter(MutableString self, [DefaultProtocol] int index, int value)
		{
			index = ((index < 0) ? (index + self.Length) : index);
			if (index < 0 || index >= self.Length)
			{
				throw RubyExceptions.CreateIndexError("index {0} out of string", index);
			}
			self.SetByte(index, (byte)value);
			return value;
		}

		[RubyMethod("[]=")]
		public static MutableString ReplaceSubstring(MutableString self, [DefaultProtocol] int start, [DefaultProtocol] int charsToOverwrite, [NotNull][DefaultProtocol] MutableString value)
		{
			if (charsToOverwrite < 0)
			{
				throw RubyExceptions.CreateIndexError("negative length {0}", charsToOverwrite);
			}
			if (Math.Abs(start) > self.Length)
			{
				throw RubyExceptions.CreateIndexError("index {0} out of string", start);
			}
			start = ((start < 0) ? (start + self.Length) : start);
			if (charsToOverwrite <= value.Length)
			{
				int num = start + charsToOverwrite;
				int num2 = charsToOverwrite;
				if (num > self.Length)
				{
					num2 -= num - self.Length;
					num = self.Length;
				}
				self.Replace(start, num2, value);
			}
			else
			{
				self.Replace(start, value.Length, value);
				int num3 = start + value.Length;
				int val = charsToOverwrite - value.Length;
				int val2 = self.Length - num3;
				self.Remove(num3, Math.Min(val, val2));
			}
			self.TaintBy(value);
			return value;
		}

		[RubyMethod("[]=")]
		public static MutableString ReplaceSubstring(ConversionStorage<int> fixnumCast, MutableString self, [NotNull] Range range, [DefaultProtocol][NotNull] MutableString value)
		{
			int num = Protocols.CastToFixnum(fixnumCast, range.Begin);
			int num2 = Protocols.CastToFixnum(fixnumCast, range.End);
			num = ((num < 0) ? (num + self.Length) : num);
			if (num < 0 || num > self.Length)
			{
				throw RubyExceptions.CreateRangeError("{0}..{1} out of range", num, num2);
			}
			num2 = ((num2 < 0) ? (num2 + self.Length) : num2);
			int charsToOverwrite = (range.ExcludeEnd ? (num2 - num) : (num2 - num + 1));
			return ReplaceSubstring(self, num, charsToOverwrite, value);
		}

		[RubyMethod("[]=")]
		public static MutableString ReplaceSubstring(MutableString self, [NotNull] MutableString substring, [NotNull][DefaultProtocol] MutableString value)
		{
			int num = self.IndexOf(substring);
			if (num == -1)
			{
				throw RubyExceptions.CreateIndexError("string not matched");
			}
			return ReplaceSubstring(self, num, substring.Length, value);
		}

		[RubyMethod("[]=")]
		public static MutableString ReplaceSubstring(RubyContext context, MutableString self, [NotNull] RubyRegex regex, [Optional][DefaultProtocol] int groupIndex, [NotNull][DefaultProtocol] MutableString value)
		{
			MatchData matchData = regex.Match(self);
			if (matchData == null)
			{
				throw RubyExceptions.CreateIndexError("regexp not matched");
			}
			if (groupIndex <= -matchData.GroupCount || groupIndex >= matchData.GroupCount)
			{
				throw RubyExceptions.CreateIndexError("index {0} out of regexp", groupIndex);
			}
			if (groupIndex < 0)
			{
				groupIndex += matchData.GroupCount;
			}
			return ReplaceSubstring(self, matchData.GetGroupStart(groupIndex), matchData.GetGroupLength(groupIndex), value);
		}

		public static bool UpCaseChar(MutableString self, int index)
		{
			char @char = self.GetChar(index);
			if (@char >= 'a' && @char <= 'z')
			{
				self.SetChar(index, @char.ToUpperInvariant());
				return true;
			}
			return false;
		}

		public static bool DownCaseChar(MutableString self, int index)
		{
			char @char = self.GetChar(index);
			if (@char >= 'A' && @char <= 'Z')
			{
				self.SetChar(index, @char.ToLowerInvariant());
				return true;
			}
			return false;
		}

		public static bool SwapCaseChar(MutableString self, int index)
		{
			char @char = self.GetChar(index);
			if (@char >= 'A' && @char <= 'Z')
			{
				self.SetChar(index, @char.ToLowerInvariant());
				return true;
			}
			if (@char >= 'a' && @char <= 'z')
			{
				self.SetChar(index, @char.ToUpperInvariant());
				return true;
			}
			return false;
		}

		public static bool CapitalizeMutableString(MutableString str)
		{
			bool result = false;
			if (!str.IsEmpty)
			{
				int charCount = str.GetCharCount();
				if (UpCaseChar(str, 0))
				{
					result = true;
				}
				for (int i = 1; i < charCount; i++)
				{
					if (DownCaseChar(str, i))
					{
						result = true;
					}
				}
			}
			return result;
		}

		public static bool UpCaseMutableString(MutableString str)
		{
			bool result = false;
			for (int i = 0; i < str.Length; i++)
			{
				if (UpCaseChar(str, i))
				{
					result = true;
				}
			}
			return result;
		}

		public static bool DownCaseMutableString(MutableString str)
		{
			bool result = false;
			for (int i = 0; i < str.Length; i++)
			{
				if (DownCaseChar(str, i))
				{
					result = true;
				}
			}
			return result;
		}

		public static bool SwapCaseMutableString(MutableString str)
		{
			bool result = false;
			for (int i = 0; i < str.Length; i++)
			{
				if (SwapCaseChar(str, i))
				{
					result = true;
				}
			}
			return result;
		}

		[RubyMethod("casecmp")]
		public static int Casecmp(MutableString self, [NotNull][DefaultProtocol] MutableString other)
		{
			return Compare(DownCase(self), DownCase(other));
		}

		[RubyMethod("capitalize")]
		public static MutableString Capitalize(MutableString self)
		{
			MutableString mutableString = self.Clone();
			CapitalizeMutableString(mutableString);
			return mutableString;
		}

		[RubyMethod("capitalize!")]
		public static MutableString CapitalizeInPlace(MutableString self)
		{
			self.RequireNotFrozen();
			if (!CapitalizeMutableString(self))
			{
				return null;
			}
			return self;
		}

		[RubyMethod("downcase")]
		public static MutableString DownCase(MutableString self)
		{
			MutableString mutableString = self.Clone();
			DownCaseMutableString(mutableString);
			return mutableString;
		}

		[RubyMethod("downcase!")]
		public static MutableString DownCaseInPlace(MutableString self)
		{
			self.RequireNotFrozen();
			if (!DownCaseMutableString(self))
			{
				return null;
			}
			return self;
		}

		[RubyMethod("swapcase")]
		public static MutableString SwapCase(MutableString self)
		{
			MutableString mutableString = self.Clone();
			SwapCaseMutableString(mutableString);
			return mutableString;
		}

		[RubyMethod("swapcase!")]
		public static MutableString SwapCaseInPlace(MutableString self)
		{
			self.RequireNotFrozen();
			if (!SwapCaseMutableString(self))
			{
				return null;
			}
			return self;
		}

		[RubyMethod("upcase")]
		public static MutableString UpCase(MutableString self)
		{
			MutableString mutableString = self.Clone();
			UpCaseMutableString(mutableString);
			return mutableString;
		}

		[RubyMethod("upcase!")]
		public static MutableString UpCaseInPlace(MutableString self)
		{
			self.RequireNotFrozen();
			if (!UpCaseMutableString(self))
			{
				return null;
			}
			return self;
		}

		[RubyMethod("center")]
		public static MutableString Center(MutableString self, [DefaultProtocol] int length, [Optional][DefaultProtocol] MutableString padding)
		{
			if (padding != null && padding.IsEmpty)
			{
				throw RubyExceptions.CreateArgumentError("zero width padding");
			}
			if (padding == null)
			{
				padding = _DefaultPadding;
			}
			else
			{
				self.RequireCompatibleEncoding(padding);
			}
			int charCount = self.GetCharCount();
			if (charCount >= length)
			{
				return self;
			}
			int charCount2 = padding.GetCharCount();
			char[] array = new char[length];
			int num = (length - charCount) / 2;
			for (int i = 0; i < num; i++)
			{
				array[i] = padding.GetChar(i % charCount2);
			}
			for (int j = 0; j < charCount; j++)
			{
				array[num + j] = self.GetChar(j);
			}
			int num2 = length - charCount - num;
			for (int k = 0; k < num2; k++)
			{
				array[num + charCount + k] = padding.GetChar(k % charCount2);
			}
			return self.CreateInstance().Append(array).TaintBy(self)
				.TaintBy(padding);
		}

		private static MutableString ChompTrailingCarriageReturns(MutableString str, bool removeCarriageReturnsToo)
		{
			int num = str.GetCharCount();
			while (true)
			{
				if (num > 1)
				{
					if (str.GetChar(num - 1) == '\n')
					{
						num -= ((str.GetChar(num - 2) != '\r') ? 1 : 2);
						continue;
					}
					if (!removeCarriageReturnsToo || str.GetChar(num - 1) != '\r')
					{
						break;
					}
					num--;
					continue;
				}
				if (num > 0 && (str.GetChar(num - 1) == '\n' || str.GetChar(num - 1) == '\r'))
				{
					num--;
				}
				break;
			}
			return str.GetSlice(0, num);
		}

		private static MutableString InternalChomp(MutableString self, MutableString separator)
		{
			if (separator == null)
			{
				return self.Clone();
			}
			if (separator.IsEmpty)
			{
				return ChompTrailingCarriageReturns(self, false).TaintBy(self);
			}
			MutableString mutableString = self.Clone();
			int charCount = mutableString.GetCharCount();
			if (separator.StartsWith('\n') && separator.GetLength() == 1)
			{
				if (charCount > 1 && mutableString.GetChar(charCount - 2) == '\r' && mutableString.GetChar(charCount - 1) == '\n')
				{
					mutableString.Remove(charCount - 2, 2);
				}
				else if (charCount > 0 && (self.GetChar(charCount - 1) == '\n' || mutableString.GetChar(charCount - 1) == '\r'))
				{
					mutableString.Remove(charCount - 1, 1);
				}
			}
			else if (mutableString.EndsWith(separator))
			{
				int charCount2 = separator.GetCharCount();
				mutableString.Remove(charCount - charCount2, charCount2);
			}
			return mutableString;
		}

		[RubyMethod("chomp")]
		public static MutableString Chomp(RubyContext context, MutableString self)
		{
			return InternalChomp(self, context.InputSeparator);
		}

		[RubyMethod("chomp")]
		public static MutableString Chomp(MutableString self, [DefaultProtocol] MutableString separator)
		{
			return InternalChomp(self, separator);
		}

		[RubyMethod("chomp!")]
		public static MutableString ChompInPlace(RubyContext context, MutableString self)
		{
			return ChompInPlace(self, context.InputSeparator);
		}

		[RubyMethod("chomp!")]
		public static MutableString ChompInPlace(MutableString self, [DefaultProtocol] MutableString separator)
		{
			MutableString mutableString = InternalChomp(self, separator);
			if (mutableString.Equals(self) || mutableString == null)
			{
				self.RequireNotFrozen();
				return null;
			}
			self.Clear();
			self.Append(mutableString);
			return self;
		}

		private static MutableString ChopInteral(MutableString self)
		{
			int charCount = self.GetCharCount();
			if (charCount == 1 || self.GetChar(charCount - 2) != '\r' || self.GetChar(charCount - 1) != '\n')
			{
				self.Remove(charCount - 1, 1);
			}
			else
			{
				self.Remove(charCount - 2, 2);
			}
			return self;
		}

		[RubyMethod("chop!")]
		public static MutableString ChopInPlace(MutableString self)
		{
			self.RequireNotFrozen();
			if (!self.IsEmpty)
			{
				return ChopInteral(self);
			}
			return null;
		}

		[RubyMethod("chop")]
		public static MutableString Chop(MutableString self)
		{
			if (!self.IsEmpty)
			{
				return ChopInteral(self.Clone());
			}
			return self.CreateInstance().TaintBy(self);
		}

		public static string GetQuotedStringRepresentation(MutableString self, bool isDump, char quote)
		{
			return self.AppendRepresentation(new StringBuilder().Append(quote), null, MutableString.Escape.NonAscii | MutableString.Escape.Special, quote).Append(quote).ToString();
		}

		[RubyMethod("dump")]
		public static MutableString Dump(MutableString self)
		{
			return self.CreateInstance().Append(GetQuotedStringRepresentation(self, true, '"')).TaintBy(self);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyContext context, MutableString self)
		{
			return MutableString.Create(GetQuotedStringRepresentation(self, false, '"'), self.Encoding).TaintBy(self);
		}

		[RubyMethod("each_byte")]
		[RubyMethod("bytes")]
		public static Enumerator EachByte(MutableString self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => EachByte(block, self));
		}

		[RubyMethod("bytes")]
		[RubyMethod("each_byte")]
		public static object EachByte([NotNull] BlockParam block, MutableString self)
		{
			foreach (byte @byte in self.GetBytes())
			{
				object blockResult;
				if (block.Yield(ScriptingRuntimeHelpers.Int32ToObject(@byte), out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("each_char")]
		[RubyMethod("chars")]
		public static Enumerator EachChar(MutableString self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => EachChar(block, self));
		}

		[RubyMethod("chars")]
		[RubyMethod("each_char")]
		public static object EachChar([NotNull] BlockParam block, MutableString self)
		{
			MutableString.CharacterEnumerator characters = self.GetCharacters();
			while (characters.MoveNext())
			{
				object blockResult;
				if (block.Yield(characters.Current.ToMutableString(self.Encoding), out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("chr")]
		public static MutableString FirstChar(MutableString self)
		{
			if (self.IsEmpty)
			{
				return self.Clone();
			}
			MutableString.CharacterEnumerator characters = self.GetCharacters();
			characters.MoveNext();
			return characters.Current.ToMutableString(self.Encoding);
		}

		[RubyMethod("each_codepoint")]
		[RubyMethod("codepoints")]
		public static Enumerator EachCodePoint(MutableString self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => EachCodePoint(block, self));
		}

		[RubyMethod("codepoints")]
		[RubyMethod("each_codepoint")]
		public static object EachCodePoint([NotNull] BlockParam block, MutableString self)
		{
			MutableString.CharacterEnumerator characters = self.GetCharacters();
			while (characters.MoveNext())
			{
				if (!characters.Current.IsValid)
				{
					throw RubyExceptions.CreateArgumentError("invalid byte sequence in {0}", self.Encoding.Name);
				}
				object blockResult;
				if (block.Yield(ScriptingRuntimeHelpers.Int32ToObject(characters.Current.Codepoint), out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("lines")]
		[RubyMethod("each_line")]
		public static Enumerator EachLine(RubyContext context, MutableString self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => EachLine(block, self, context.InputSeparator));
		}

		[RubyMethod("lines")]
		[RubyMethod("each_line")]
		public static object EachLine(RubyContext context, [NotNull] BlockParam block, MutableString self)
		{
			return EachLine(block, self, context.InputSeparator);
		}

		[RubyMethod("lines")]
		[RubyMethod("each_line")]
		public static Enumerator EachLine(MutableString self, [DefaultProtocol] MutableString separator)
		{
			return new Enumerator((RubyScope _, BlockParam block) => EachLine(block, self, separator, 0));
		}

		[RubyMethod("lines")]
		[RubyMethod("each_line")]
		public static object EachLine([NotNull] BlockParam block, MutableString self, [DefaultProtocol] MutableString separator)
		{
			return EachLine(block, self, separator, 0);
		}

		public static object EachLine(BlockParam block, MutableString self, [DefaultProtocol] MutableString separator, int start)
		{
			self.TrackChanges();
			MutableString mutableString;
			if (separator == null || separator.IsEmpty)
			{
				separator = DefaultLineSeparator;
				mutableString = DefaultParagraphSeparator;
			}
			else
			{
				mutableString = null;
			}
			while (start < self.Length)
			{
				int num;
				if (mutableString == null)
				{
					num = self.IndexOf(separator, start);
					num = ((num < 0) ? self.Length : (num + separator.Length));
				}
				else
				{
					num = self.IndexOf(mutableString, start);
					if (num >= 0)
					{
						for (num += 2 * separator.Length; self.IndexOf(separator, num) == num; num += separator.Length)
						{
						}
					}
					else
					{
						num = self.Length;
					}
				}
				MutableString arg = self.CreateInstance().TaintBy(self).Append(self, start, num - start);
				object blockResult;
				if (block.Yield(arg, out blockResult))
				{
					return blockResult;
				}
				start = num;
			}
			RequireNoVersionChange(self);
			return self;
		}

		[RubyMethod("empty?")]
		public static bool IsEmpty(MutableString self)
		{
			return self.IsEmpty;
		}

		[RubyMethod("size")]
		[RubyMethod("length")]
		public static int GetCharacterCount(MutableString self)
		{
			return self.GetCharacterCount();
		}

		[RubyMethod("bytesize")]
		public static int GetByteCount(MutableString self)
		{
			return self.GetByteCount();
		}

		[RubyMethod("ascii_only?")]
		public static bool IsAscii(MutableString self)
		{
			return self.IsAscii();
		}

		[RubyMethod("encoding")]
		public static RubyEncoding GetEncoding(MutableString self)
		{
			return self.Encoding;
		}

		[RubyMethod("valid_encoding?")]
		public static bool ValidEncoding(MutableString self)
		{
			return !self.ContainsInvalidCharacters();
		}

		[RubyMethod("force_encoding")]
		public static MutableString ForceEncoding(MutableString self, [NotNull] RubyEncoding encoding)
		{
			self.ForceEncoding(encoding);
			return self;
		}

		[RubyMethod("force_encoding")]
		public static MutableString ForceEncoding(RubyContext context, MutableString self, [NotNull][DefaultProtocol] MutableString encodingName)
		{
			return ForceEncoding(self, context.GetRubyEncoding(encodingName));
		}

		[RubyMethod("encode")]
		public static MutableString Encode(ConversionStorage<IDictionary<object, object>> toHash, ConversionStorage<MutableString> toStr, MutableString self, [Optional] object toEncoding, [Optional] object fromEncoding, [DefaultProtocol] IDictionary<object, object> options)
		{
			return EncodeInPlace(toHash, toStr, self.Clone(), toEncoding, fromEncoding, options);
		}

		[RubyMethod("encode!")]
		public static MutableString EncodeInPlace(ConversionStorage<IDictionary<object, object>> toHash, ConversionStorage<MutableString> toStr, MutableString self, [Optional] object toEncoding, [Optional] object fromEncoding, [DefaultProtocol] IDictionary<object, object> options)
		{
			Protocols.TryConvertToOptions(toHash, ref options, ref toEncoding, ref fromEncoding);
			MutableString mutableString = null;
			MutableString mutableString2 = null;
			RubyEncoding rubyEncoding;
			if (toEncoding == Missing.Value)
			{
				rubyEncoding = toStr.Context.DefaultInternalEncoding;
				if (rubyEncoding == null)
				{
					return self;
				}
			}
			else
			{
				rubyEncoding = toEncoding as RubyEncoding;
				if (rubyEncoding == null)
				{
					mutableString = Protocols.CastToString(toStr, toEncoding);
				}
			}
			RubyEncoding rubyEncoding2;
			if (fromEncoding == Missing.Value)
			{
				rubyEncoding2 = self.Encoding;
			}
			else
			{
				rubyEncoding2 = fromEncoding as RubyEncoding;
				if (rubyEncoding2 == null)
				{
					mutableString2 = Protocols.CastToString(toStr, fromEncoding);
				}
			}
			try
			{
				if (mutableString2 != null)
				{
					rubyEncoding2 = toStr.Context.GetRubyEncoding(mutableString2);
				}
				if (mutableString != null)
				{
					rubyEncoding = toStr.Context.GetRubyEncoding(mutableString);
				}
			}
			catch (ArgumentException)
			{
				throw new ConverterNotFoundError(RubyExceptions.FormatMessage("code converter not found ({0} to {1})", (mutableString2 != null) ? mutableString2.ToAsciiString() : rubyEncoding2.Name, (mutableString != null) ? mutableString.ToAsciiString() : rubyEncoding.Name));
			}
			self.RequireNotFrozen();
			self.Transcode(rubyEncoding2, rubyEncoding);
			return self;
		}

		private static bool BlockReplaceFirst(ConversionStorage<MutableString> tosConversion, RubyScope scope, MutableString input, BlockParam block, RubyRegex pattern, out object blockResult, out MutableString result)
		{
			RubyClosureScope innerMostClosureScope = scope.GetInnerMostClosureScope();
			MatchData matchData = RegexpOps.Match(scope, pattern, input);
			if (matchData == null)
			{
				result = null;
				blockResult = null;
				innerMostClosureScope.CurrentMatch = null;
				return false;
			}
			result = input.Clone();
			innerMostClosureScope.CurrentMatch = matchData;
			if (block.Yield(matchData.GetValue(), out blockResult))
			{
				return true;
			}
			innerMostClosureScope.CurrentMatch = matchData;
			MutableString mutableString = Protocols.ConvertToString(tosConversion, blockResult);
			result.TaintBy(mutableString);
			result.Replace(matchData.Index, matchData.Length, mutableString);
			blockResult = null;
			return false;
		}

		private static bool BlockReplaceAll(ConversionStorage<MutableString> tosConversion, RubyScope scope, MutableString input, BlockParam block, RubyRegex regex, out object blockResult, out MutableString result)
		{
			RubyClosureScope innerMostClosureScope = scope.GetInnerMostClosureScope();
			IList<MatchData> list = regex.Matches(input);
			if (list.Count == 0)
			{
				result = null;
				blockResult = null;
				innerMostClosureScope.CurrentMatch = null;
				return false;
			}
			result = input.CreateInstance().TaintBy(input);
			int num = 0;
			using (IEnumerator<MatchData> enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					MatchData matchData = (innerMostClosureScope.CurrentMatch = enumerator.Current);
					input.TrackChanges();
					if (block.Yield(matchData.GetValue(), out blockResult))
					{
						return true;
					}
					if (input.HasChanged)
					{
						return false;
					}
					innerMostClosureScope.CurrentMatch = matchData;
					MutableString mutableString = Protocols.ConvertToString(tosConversion, blockResult);
					result.TaintBy(mutableString);
					result.Append(input, num, matchData.Index - num);
					result.Append(mutableString);
					num = matchData.Index + matchData.Length;
				}
			}
			result.Append(input, num, input.Length - num);
			blockResult = null;
			return false;
		}

		private static void AppendBackslashes(int backslashCount, MutableString result, int minBackslashes)
		{
			for (int i = 0; i < (backslashCount - 1 >> 1) + minBackslashes; i++)
			{
				result.Append('\\');
			}
		}

		private static void AppendReplacementExpression(ConversionStorage<MutableString> toS, BinaryOpStorage hashDefault, MutableString input, MatchData match, MutableString result, Union<IDictionary<object, object>, MutableString> replacement)
		{
			if (replacement.Second != null)
			{
				AppendReplacementExpression(input, match, result, replacement.Second);
				return;
			}
			object element = HashOps.GetElement(hashDefault, replacement.First, match.GetValue());
			if (element != null)
			{
				MutableString mutableString = Protocols.ConvertToString(toS, element);
				result.Append(mutableString).TaintBy(mutableString);
			}
		}

		private static void AppendReplacementExpression(MutableString input, MatchData match, MutableString result, MutableString replacement)
		{
			int num = 0;
			for (int i = 0; i < replacement.Length; i++)
			{
				char @char = replacement.GetChar(i);
				if (@char == '\\')
				{
					num++;
					continue;
				}
				if (num == 0)
				{
					result.Append(@char);
					continue;
				}
				AppendBackslashes(num, result, 0);
				if ((num & 1) == 1)
				{
					if (char.IsDigit(@char))
					{
						AppendGroupByIndex(match, @char - 48, result);
					}
					else
					{
						switch (@char)
						{
						case '&':
							AppendGroupByIndex(match, match.GroupCount - 1, result);
							break;
						case '`':
							result.Append(input, 0, match.Index);
							break;
						case '\'':
						{
							int num2 = match.Index + match.Length;
							result.Append(input, num2, input.GetLength() - num2);
							break;
						}
						case '+':
							AppendLastCharOfLastMatchGroup(match, result);
							break;
						default:
							result.Append('\\');
							result.Append(@char);
							break;
						}
					}
				}
				else
				{
					AppendBackslashes(num, result, 1);
					result.Append(@char);
				}
				num = 0;
			}
			AppendBackslashes(num, result, 1);
			result.TaintBy(replacement);
		}

		private static void AppendLastCharOfLastMatchGroup(MatchData match, MutableString result)
		{
			int num = match.GroupCount - 1;
			while (num > 0 && !match.GroupSuccess(num))
			{
				num--;
			}
			if (num > 0 && match.GroupSuccess(num))
			{
				int groupLength = match.GetGroupLength(num);
				if (groupLength > 0)
				{
					result.Append(match.OriginalString, match.GetGroupStart(num) + groupLength - 1, 1);
				}
			}
		}

		private static void AppendGroupByIndex(MatchData match, int index, MutableString result)
		{
			MutableString groupValue = match.GetGroupValue(index);
			if (groupValue != null)
			{
				result.Append(groupValue);
			}
		}

		private static MutableString ReplaceFirst(ConversionStorage<MutableString> toS, BinaryOpStorage hashDefault, RubyScope scope, MutableString input, Union<IDictionary<object, object>, MutableString> replacement, RubyRegex pattern)
		{
			MatchData matchData = RegexpOps.Match(scope, pattern, input);
			if (matchData == null)
			{
				return null;
			}
			MutableString mutableString = input.CreateInstance().TaintBy(input);
			mutableString.Append(input, 0, matchData.Index);
			AppendReplacementExpression(toS, hashDefault, input, matchData, mutableString, replacement);
			int num = matchData.Index + matchData.Length;
			mutableString.Append(input, num, input.Length - num);
			return mutableString;
		}

		private static MutableString ReplaceAll(ConversionStorage<MutableString> toS, BinaryOpStorage hashDefault, RubyScope scope, MutableString input, Union<IDictionary<object, object>, MutableString> replacement, RubyRegex regex)
		{
			RubyClosureScope innerMostClosureScope = scope.GetInnerMostClosureScope();
			IList<MatchData> list = regex.Matches(input);
			if (list.Count == 0)
			{
				innerMostClosureScope.CurrentMatch = null;
				return null;
			}
			MutableString mutableString = input.CreateInstance().TaintBy(input);
			int num = 0;
			foreach (MatchData item in list)
			{
				mutableString.Append(input, num, item.Index - num);
				AppendReplacementExpression(toS, hashDefault, input, item, mutableString, replacement);
				num = item.Index + item.Length;
			}
			mutableString.Append(input, num, input.Length - num);
			innerMostClosureScope.CurrentMatch = list[list.Count - 1];
			return mutableString;
		}

		[RubyMethod("sub")]
		public static object BlockReplaceFirst(ConversionStorage<MutableString> tosConversion, RubyScope scope, [NotNull] BlockParam block, MutableString self, [NotNull] RubyRegex pattern)
		{
			object blockResult;
			MutableString result;
			object obj;
			if (!BlockReplaceFirst(tosConversion, scope, self, block, pattern, out blockResult, out result))
			{
				obj = result;
				if (obj == null)
				{
					return self.Clone();
				}
			}
			else
			{
				obj = blockResult;
			}
			return obj;
		}

		[RubyMethod("gsub")]
		public static object BlockReplaceAll(ConversionStorage<MutableString> tosConversion, RubyScope scope, [NotNull] BlockParam block, MutableString self, [NotNull] RubyRegex pattern)
		{
			self.TrackChanges();
			object blockResult;
			MutableString result;
			object result2 = (BlockReplaceAll(tosConversion, scope, self, block, pattern, out blockResult, out result) ? blockResult : (result ?? self.Clone()));
			RequireNoVersionChange(self);
			return result2;
		}

		[RubyMethod("sub")]
		public static object BlockReplaceFirst(ConversionStorage<MutableString> tosConversion, RubyScope scope, [NotNull] BlockParam block, MutableString self, [NotNull] MutableString matchString)
		{
			RubyRegex pattern = new RubyRegex(MutableString.CreateMutable(Regex.Escape(matchString.ToString()), matchString.Encoding), RubyRegexOptions.NONE);
			object blockResult;
			MutableString result;
			object obj;
			if (!BlockReplaceFirst(tosConversion, scope, self, block, pattern, out blockResult, out result))
			{
				obj = result;
				if (obj == null)
				{
					return self.Clone();
				}
			}
			else
			{
				obj = blockResult;
			}
			return obj;
		}

		[RubyMethod("gsub")]
		public static object BlockReplaceAll(ConversionStorage<MutableString> tosConversion, RubyScope scope, [NotNull] BlockParam block, MutableString self, [NotNull] MutableString matchString)
		{
			RubyRegex regex = new RubyRegex(MutableString.CreateMutable(Regex.Escape(matchString.ToString()), matchString.Encoding), RubyRegexOptions.NONE);
			self.TrackChanges();
			object blockResult;
			MutableString result;
			object result2 = (BlockReplaceAll(tosConversion, scope, self, block, regex, out blockResult, out result) ? blockResult : (result ?? self.Clone()));
			RequireNoVersionChange(self);
			return result2;
		}

		[RubyMethod("sub")]
		public static MutableString ReplaceFirst(RubyScope scope, MutableString self, [NotNull][DefaultProtocol] RubyRegex pattern, [NotNull] MutableString replacement)
		{
			return ReplaceFirst(null, null, scope, self, replacement, pattern) ?? self.Clone();
		}

		[RubyMethod("gsub")]
		public static MutableString ReplaceAll(RubyScope scope, MutableString self, [DefaultProtocol][NotNull] RubyRegex pattern, [NotNull] MutableString replacement)
		{
			return ReplaceAll(null, null, scope, self, replacement, pattern) ?? self.Clone();
		}

		[RubyMethod("sub")]
		public static MutableString ReplaceFirst(ConversionStorage<MutableString> toS, BinaryOpStorage hashDefault, RubyScope scope, MutableString self, [NotNull][DefaultProtocol] RubyRegex pattern, [NotNull][DefaultProtocol] Union<IDictionary<object, object>, MutableString> replacement)
		{
			return ReplaceFirst(toS, hashDefault, scope, self, replacement, pattern) ?? self.Clone();
		}

		[RubyMethod("gsub")]
		public static MutableString ReplaceAll(ConversionStorage<MutableString> toS, BinaryOpStorage hashDefault, RubyScope scope, MutableString self, [NotNull][DefaultProtocol] RubyRegex pattern, [DefaultProtocol][NotNull] Union<IDictionary<object, object>, MutableString> replacement)
		{
			return ReplaceAll(toS, hashDefault, scope, self, replacement, pattern) ?? self.Clone();
		}

		private static object BlockReplaceInPlace(ConversionStorage<MutableString> tosConversion, RubyScope scope, BlockParam block, MutableString self, RubyRegex pattern, bool replaceAll)
		{
			self.RequireNotFrozen();
			self.TrackChanges();
			object blockResult;
			MutableString result;
			if (replaceAll ? BlockReplaceAll(tosConversion, scope, self, block, pattern, out blockResult, out result) : BlockReplaceFirst(tosConversion, scope, self, block, pattern, out blockResult, out result))
			{
				return blockResult;
			}
			if (result == null)
			{
				return null;
			}
			RequireNoVersionChange(self);
			self.Replace(0, self.Length, result);
			return self.TaintBy(result);
		}

		private static MutableString ReplaceInPlace(ConversionStorage<MutableString> toS, BinaryOpStorage hashDefault, RubyScope scope, MutableString self, RubyRegex pattern, Union<IDictionary<object, object>, MutableString> replacement, bool replaceAll)
		{
			self.RequireNotFrozen();
			MutableString mutableString = (replaceAll ? ReplaceAll(toS, hashDefault, scope, self, replacement, pattern) : ReplaceFirst(toS, hashDefault, scope, self, replacement, pattern));
			if (mutableString == null)
			{
				return null;
			}
			self.Replace(0, self.Length, mutableString);
			return self.TaintBy(mutableString);
		}

		[RubyMethod("sub!")]
		public static object BlockReplaceFirstInPlace(ConversionStorage<MutableString> tosConversion, RubyScope scope, [NotNull] BlockParam block, MutableString self, [DefaultProtocol][NotNull] RubyRegex pattern)
		{
			return BlockReplaceInPlace(tosConversion, scope, block, self, pattern, false);
		}

		[RubyMethod("gsub!")]
		public static object BlockReplaceAllInPlace(ConversionStorage<MutableString> tosConversion, RubyScope scope, [NotNull] BlockParam block, MutableString self, [NotNull][DefaultProtocol] RubyRegex pattern)
		{
			return BlockReplaceInPlace(tosConversion, scope, block, self, pattern, true);
		}

		[RubyMethod("sub!")]
		public static MutableString ReplaceFirstInPlace(RubyScope scope, MutableString self, [DefaultProtocol][NotNull] RubyRegex pattern, [NotNull][DefaultProtocol] MutableString replacement)
		{
			return ReplaceInPlace(null, null, scope, self, pattern, replacement, false);
		}

		[RubyMethod("gsub!")]
		public static MutableString ReplaceAllInPlace(RubyScope scope, MutableString self, [DefaultProtocol][NotNull] RubyRegex pattern, [NotNull][DefaultProtocol] MutableString replacement)
		{
			return ReplaceInPlace(null, null, scope, self, pattern, replacement, true);
		}

		[RubyMethod("sub!")]
		public static MutableString ReplaceFirstInPlace(ConversionStorage<MutableString> toS, BinaryOpStorage hashDefault, RubyScope scope, MutableString self, [NotNull][DefaultProtocol] RubyRegex pattern, [DefaultProtocol][NotNull] Union<IDictionary<object, object>, MutableString> replacement)
		{
			return ReplaceInPlace(toS, hashDefault, scope, self, pattern, replacement, false);
		}

		[RubyMethod("gsub!")]
		public static MutableString ReplaceAllInPlace(ConversionStorage<MutableString> toS, BinaryOpStorage hashDefault, RubyScope scope, MutableString self, [NotNull][DefaultProtocol] RubyRegex pattern, [NotNull][DefaultProtocol] Union<IDictionary<object, object>, MutableString> replacement)
		{
			return ReplaceInPlace(toS, hashDefault, scope, self, pattern, replacement, true);
		}

		[RubyMethod("index")]
		public static object Index(MutableString self, [DefaultProtocol][NotNull] MutableString substring, [Optional][DefaultProtocol] int start)
		{
			self.PrepareForCharacterRead();
			if (!NormalizeStart(self.GetCharCount(), ref start))
			{
				return null;
			}
			self.RequireCompatibleEncoding(substring);
			substring.PrepareForCharacterRead();
			int num = self.IndexOf(substring, start);
			if (num == -1)
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(num);
		}

		[RubyMethod("index")]
		public static object Index(RubyScope scope, MutableString self, [NotNull] RubyRegex regex, [Optional][DefaultProtocol] int start)
		{
			MatchData matchData = regex.Match(self, start, true);
			scope.GetInnerMostClosureScope().CurrentMatch = matchData;
			if (matchData == null)
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(matchData.Index);
		}

		[RubyMethod("rindex")]
		public static object LastIndexOf(MutableString self, [DefaultProtocol][NotNull] MutableString substring)
		{
			if (substring.IsEmpty)
			{
				self.PrepareForCharacterRead();
				return ScriptingRuntimeHelpers.Int32ToObject(self.GetCharCount());
			}
			return LastIndexOf(self, substring, -1);
		}

		[RubyMethod("rindex")]
		public static object LastIndexOf(MutableString self, [NotNull][DefaultProtocol] MutableString substring, [DefaultProtocol] int start)
		{
			self.PrepareForCharacterRead();
			int charCount = self.GetCharCount();
			start = IListOps.NormalizeIndex(charCount, start);
			if (start < 0)
			{
				return null;
			}
			if (substring.IsEmpty)
			{
				return ScriptingRuntimeHelpers.Int32ToObject((start >= charCount) ? charCount : start);
			}
			self.RequireCompatibleEncoding(substring);
			substring.PrepareForCharacterRead();
			int charCount2 = substring.GetCharCount();
			start = ((start <= charCount - charCount2) ? (start + (charCount2 - 1)) : (charCount - 1));
			int num = self.LastIndexOf(substring, start);
			if (num == -1)
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(num);
		}

		[RubyMethod("rindex")]
		public static object LastIndexOf(RubyScope scope, MutableString self, [NotNull] RubyRegex regex, [DefaultProtocol] int start)
		{
			MatchData matchData = regex.LastMatch(self, start);
			scope.GetInnerMostClosureScope().CurrentMatch = matchData;
			if (matchData == null)
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(matchData.Index);
		}

		private static bool NormalizeStart(int length, ref int start)
		{
			start = IListOps.NormalizeIndex(length, start);
			if (start < 0 || start > length)
			{
				return false;
			}
			return true;
		}

		[RubyMethod("start_with?")]
		public static bool StartsWith(RubyScope scope, MutableString self, [Optional][DefaultProtocol] MutableString subString)
		{
			if (subString == null || self.Length < subString.Length)
			{
				return false;
			}
			return self.GetSlice(0, subString.Length).Equals(subString);
		}

		[RubyMethod("end_with?")]
		public static bool EndsWith(RubyScope scope, MutableString self, [Optional][DefaultProtocol] MutableString subString)
		{
			if (subString == null || self.Length < subString.Length)
			{
				return false;
			}
			return self.EndsWith(subString.ConvertToString());
		}

		private static MutableString InternalDelete(MutableString self, MutableString[] ranges)
		{
			BitArray bitArray = new RangeParser(ranges).Parse();
			MutableString mutableString = self.CreateInstance().TaintBy(self);
			for (int i = 0; i < self.Length; i++)
			{
				if (!bitArray.Get(self.GetChar(i)))
				{
					mutableString.Append(self.GetChar(i));
				}
			}
			return mutableString;
		}

		private static MutableString InternalDeleteInPlace(MutableString self, MutableString[] ranges)
		{
			MutableString mutableString = InternalDelete(self, ranges);
			if (self.Equals(mutableString))
			{
				return null;
			}
			self.Clear();
			self.Append(mutableString);
			return self;
		}

		[RubyMethod("delete")]
		public static MutableString Delete(MutableString self, [NotNullItems][DefaultProtocol] params MutableString[] strs)
		{
			if (strs.Length == 0)
			{
				throw RubyExceptions.CreateArgumentError("wrong number of arguments");
			}
			return InternalDelete(self, strs);
		}

		[RubyMethod("delete!")]
		public static MutableString DeleteInPlace(MutableString self, [DefaultProtocol][NotNullItems] params MutableString[] strs)
		{
			self.RequireNotFrozen();
			if (strs.Length == 0)
			{
				throw RubyExceptions.CreateArgumentError("wrong number of arguments");
			}
			return InternalDeleteInPlace(self, strs);
		}

		[RubyMethod("clear")]
		public static MutableString Clear(MutableString self)
		{
			return self.Clear();
		}

		private static object InternalCount(MutableString self, MutableString[] ranges)
		{
			BitArray bitArray = new RangeParser(ranges).Parse();
			int num = 0;
			for (int i = 0; i < self.Length; i++)
			{
				if (bitArray.Get(self.GetChar(i)))
				{
					num++;
				}
			}
			return ScriptingRuntimeHelpers.Int32ToObject(num);
		}

		[RubyMethod("count")]
		public static object Count(RubyContext context, MutableString self, [DefaultProtocol][NotNullItems] params MutableString[] strs)
		{
			if (strs.Length == 0)
			{
				throw RubyExceptions.CreateArgumentError("wrong number of arguments");
			}
			return InternalCount(self, strs);
		}

		[RubyMethod("include?")]
		public static bool Include(MutableString str, [NotNull][DefaultProtocol] MutableString subString)
		{
			str.RequireCompatibleEncoding(subString);
			str.PrepareForCharacterRead();
			subString.PrepareForCharacterRead();
			return str.IndexOf(subString) != -1;
		}

		[RubyMethod("include?")]
		public static bool Include(MutableString str, int c)
		{
			return str.IndexOf((byte)(c % 256)) != -1;
		}

		[RubyMethod("insert")]
		public static MutableString Insert(MutableString self, [DefaultProtocol] int start, [DefaultProtocol][NotNull] MutableString value)
		{
			return self.Insert(NormalizeInsertIndex(start, self.GetLength()), value).TaintBy(value);
		}

		[RubyMethod("=~")]
		public static object Match(RubyScope scope, MutableString self, [NotNull] RubyRegex regex)
		{
			return RegexpOps.MatchIndex(scope, regex, self);
		}

		[RubyMethod("=~")]
		public static object Match(MutableString self, [NotNull] MutableString str)
		{
			throw RubyExceptions.CreateTypeError("type mismatch: String given");
		}

		[RubyMethod("=~")]
		public static object Match(CallSiteStorage<Func<CallSite, RubyScope, object, object, object>> storage, RubyScope scope, MutableString self, object obj)
		{
			CallSite<Func<CallSite, RubyScope, object, object, object>> callSite = storage.GetCallSite("=~", new RubyCallSignature(1, (RubyCallFlags)17));
			return callSite.Target(callSite, scope, obj, self);
		}

		[RubyMethod("match")]
		public static object Match(CallSiteStorage<Func<CallSite, RubyScope, object, object, object>> storage, RubyScope scope, MutableString self, [NotNull] RubyRegex regex)
		{
			CallSite<Func<CallSite, RubyScope, object, object, object>> callSite = storage.GetCallSite("match", new RubyCallSignature(1, (RubyCallFlags)17));
			return callSite.Target(callSite, scope, regex, self);
		}

		[RubyMethod("match")]
		public static object Match(CallSiteStorage<Func<CallSite, RubyScope, object, object, object>> storage, RubyScope scope, MutableString self, [NotNull][DefaultProtocol] MutableString pattern)
		{
			CallSite<Func<CallSite, RubyScope, object, object, object>> callSite = storage.GetCallSite("match", new RubyCallSignature(1, (RubyCallFlags)17));
			return callSite.Target(callSite, scope, new RubyRegex(pattern, RubyRegexOptions.NONE), self);
		}

		[RubyMethod("scan")]
		public static RubyArray Scan(RubyScope scope, MutableString self, [DefaultProtocol][NotNull] RubyRegex regex)
		{
			IList<MatchData> list = regex.Matches(self, false);
			RubyClosureScope innerMostClosureScope = scope.GetInnerMostClosureScope();
			RubyArray rubyArray = new RubyArray(list.Count);
			if (list.Count == 0)
			{
				innerMostClosureScope.CurrentMatch = null;
				return rubyArray;
			}
			foreach (MatchData item in list)
			{
				rubyArray.Add(MatchToScanResult(scope, self, regex, item));
			}
			innerMostClosureScope.CurrentMatch = list[list.Count - 1];
			return rubyArray;
		}

		[RubyMethod("scan")]
		public static object Scan(RubyScope scope, [NotNull] BlockParam block, MutableString self, [DefaultProtocol][NotNull] RubyRegex regex)
		{
			RubyClosureScope innerMostClosureScope = scope.GetInnerMostClosureScope();
			IList<MatchData> list = regex.Matches(self);
			if (list.Count == 0)
			{
				innerMostClosureScope.CurrentMatch = null;
				return self;
			}
			using (IEnumerator<MatchData> enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					MatchData matchData = (innerMostClosureScope.CurrentMatch = enumerator.Current);
					object blockResult;
					if (block.Yield(MatchToScanResult(scope, self, regex, matchData), out blockResult))
					{
						return blockResult;
					}
					innerMostClosureScope.CurrentMatch = matchData;
				}
				return self;
			}
		}

		private static object MatchToScanResult(RubyScope scope, MutableString self, RubyRegex regex, MatchData match)
		{
			if (match.GroupCount == 1)
			{
				return match.GetValue().TaintBy(regex, scope);
			}
			RubyArray rubyArray = new RubyArray(match.GroupCount - 1);
			for (int i = 1; i < match.GroupCount; i++)
			{
				MutableString groupValue = match.GetGroupValue(i);
				rubyArray.Add((groupValue != null) ? groupValue.TaintBy(regex, scope) : groupValue);
			}
			return rubyArray;
		}

		public static int GetIndexOfRightmostAlphaNumericCharacter(MutableString str, int index)
		{
			for (int num = index; num >= 0; num--)
			{
				if (char.IsLetterOrDigit(str.GetChar(num)))
				{
					return num;
				}
			}
			return -1;
		}

		public static void IncrementAlphaNumericChar(MutableString str, int index)
		{
			char @char = str.GetChar(index);
			if (@char == 'z' || @char == 'Z' || @char == '9')
			{
				int indexOfRightmostAlphaNumericCharacter = GetIndexOfRightmostAlphaNumericCharacter(str, index - 1);
				switch (@char)
				{
				case 'z':
					str.SetChar(index, 'a');
					if (indexOfRightmostAlphaNumericCharacter == -1)
					{
						str.Insert(index, 'a');
					}
					else
					{
						IncrementAlphaNumericChar(str, indexOfRightmostAlphaNumericCharacter);
					}
					break;
				case 'Z':
					str.SetChar(index, 'A');
					if (indexOfRightmostAlphaNumericCharacter == -1)
					{
						str.Insert(index, 'A');
					}
					else
					{
						IncrementAlphaNumericChar(str, indexOfRightmostAlphaNumericCharacter);
					}
					break;
				default:
					str.SetChar(index, '0');
					if (indexOfRightmostAlphaNumericCharacter == -1)
					{
						str.Insert(index, '1');
					}
					else
					{
						IncrementAlphaNumericChar(str, indexOfRightmostAlphaNumericCharacter);
					}
					break;
				}
			}
			else
			{
				IncrementChar(str, index);
			}
		}

		public static void IncrementChar(MutableString str, int index)
		{
			byte @byte = str.GetByte(index);
			if (@byte == byte.MaxValue)
			{
				str.SetByte(index, 0);
				if (index > 0)
				{
					IncrementChar(str, index - 1);
				}
				else
				{
					str.Insert(0, 1);
				}
			}
			else
			{
				str.SetByte(index, (byte)(@byte + 1));
			}
		}

		[RubyMethod("next!")]
		[RubyMethod("succ!")]
		public static MutableString SuccInPlace(MutableString self)
		{
			self.RequireNotFrozen();
			if (self.IsEmpty)
			{
				return self;
			}
			int indexOfRightmostAlphaNumericCharacter = GetIndexOfRightmostAlphaNumericCharacter(self, self.Length - 1);
			if (indexOfRightmostAlphaNumericCharacter == -1)
			{
				IncrementChar(self, self.Length - 1);
			}
			else
			{
				IncrementAlphaNumericChar(self, indexOfRightmostAlphaNumericCharacter);
			}
			return self;
		}

		[RubyMethod("next")]
		[RubyMethod("succ")]
		public static MutableString Succ(MutableString self)
		{
			return SuccInPlace(self.Clone());
		}

		private static RubyArray MakeRubyArray(MutableString self, MutableString[] elements)
		{
			return MakeRubyArray(self, elements, 0, elements.Length);
		}

		private static RubyArray MakeRubyArray(MutableString self, MutableString[] elements, int start, int count)
		{
			RubyArray rubyArray = new RubyArray(elements.Length);
			for (int i = 0; i < count; i++)
			{
				rubyArray.Add(self.CreateInstance().Append(elements[start + i]).TaintBy(self));
			}
			return rubyArray;
		}

		private static RubyArray WhitespaceSplit(MutableString str, int limit)
		{
			int maxComponents = ((limit <= 0) ? int.MaxValue : limit);
			MutableString[] array = str.Split(_WhiteSpaceSeparators, maxComponents, StringSplitOptions.RemoveEmptyEntries);
			RubyArray rubyArray = new RubyArray(array.Length + ((limit < 0) ? 1 : 0));
			MutableString[] array2 = array;
			foreach (MutableString value in array2)
			{
				rubyArray.Add(str.CreateInstance().Append(value).TaintBy(str));
			}
			if (limit < 0)
			{
				rubyArray.Add(str.CreateInstance().TaintBy(str));
			}
			return rubyArray;
		}

		private static RubyArray InternalSplit(MutableString str, MutableString separator, int limit)
		{
			RubyArray rubyArray;
			if (limit == 1)
			{
				rubyArray = new RubyArray(1);
				rubyArray.Add(str);
				return rubyArray;
			}
			if (separator == null || (separator.StartsWith(' ') && separator.GetLength() == 1))
			{
				return WhitespaceSplit(str, limit);
			}
			if (separator.IsEmpty)
			{
				return CharacterSplit(str, limit);
			}
			rubyArray = ((limit > 0) ? new RubyArray(limit + 1) : new RubyArray());
			str.PrepareForCharacterRead();
			separator.PrepareForCharacterRead();
			str.RequireCompatibleEncoding(separator);
			int charCount = separator.GetCharCount();
			int num = 0;
			int num2;
			while ((limit <= 0 || rubyArray.Count < limit - 1) && (num2 = str.IndexOf(separator, num)) != -1)
			{
				rubyArray.Add(str.CreateInstance().Append(str, num, num2 - num).TaintBy(str));
				num = num2 + charCount;
			}
			rubyArray.Add(str.CreateInstance().Append(str, num).TaintBy(str));
			if (limit == 0)
			{
				RemoveTrailingEmptyItems(rubyArray);
			}
			return rubyArray;
		}

		private static void RemoveTrailingEmptyItems(RubyArray array)
		{
			while (array.Count != 0 && ((MutableString)array[array.Count - 1]).IsEmpty)
			{
				array.RemoveAt(array.Count - 1);
			}
		}

		private static RubyArray CharacterSplit(MutableString str, int limit)
		{
			RubyArray rubyArray = new RubyArray();
			MutableString.CharacterEnumerator characters = str.GetCharacters();
			int num = 0;
			while ((limit <= 0 || rubyArray.Count < limit - 1) && characters.MoveNext())
			{
				rubyArray.Add(str.CreateInstance().Append(characters.Current).TaintBy(str));
				num++;
			}
			if (characters.HasMore || limit < 0)
			{
				rubyArray.Add(str.CreateInstance().AppendRemaining(characters).TaintBy(str));
			}
			return rubyArray;
		}

		[RubyMethod("split")]
		public static RubyArray Split(ConversionStorage<MutableString> stringCast, MutableString self)
		{
			return Split(stringCast, self, (MutableString)null, 0);
		}

		[RubyMethod("split")]
		public static RubyArray Split(ConversionStorage<MutableString> stringCast, MutableString self, [DefaultProtocol] MutableString separator, [Optional][DefaultProtocol] int limit)
		{
			if (separator == null)
			{
				object stringSeparator = stringCast.Context.StringSeparator;
				RubyRegex rubyRegex = stringSeparator as RubyRegex;
				if (rubyRegex != null)
				{
					return Split(stringCast, self, rubyRegex, limit);
				}
				if (stringSeparator != null)
				{
					separator = Protocols.CastToString(stringCast, stringSeparator);
				}
			}
			if (self.IsEmpty)
			{
				return new RubyArray();
			}
			return InternalSplit(self, separator, limit);
		}

		[RubyMethod("split")]
		public static RubyArray Split(ConversionStorage<MutableString> stringCast, MutableString self, [NotNull] RubyRegex regexp, [Optional][DefaultProtocol] int limit)
		{
			if (regexp.IsEmpty)
			{
				return InternalSplit(self, MutableString.FrozenEmpty, limit);
			}
			if (self.IsEmpty)
			{
				return new RubyArray();
			}
			if (limit == 0)
			{
				RubyArray rubyArray = MakeRubyArray(self, regexp.Split(self));
				while (rubyArray.Count != 0 && ((MutableString)rubyArray[rubyArray.Count - 1]).Length == 0)
				{
					rubyArray.RemoveAt(rubyArray.Count - 1);
				}
				return rubyArray;
			}
			if (limit == 1)
			{
				RubyArray rubyArray2 = new RubyArray(1);
				rubyArray2.Add(self);
				return rubyArray2;
			}
			if (limit < 0)
			{
				return MakeRubyArray(self, regexp.Split(self));
			}
			return MakeRubyArray(self, regexp.Split(self, limit));
		}

		[RubyMethod("strip")]
		public static MutableString Strip(RubyContext context, MutableString self)
		{
			return Strip(self, true, true);
		}

		[RubyMethod("lstrip")]
		public static MutableString StripLeft(RubyContext context, MutableString self)
		{
			return Strip(self, true, false);
		}

		[RubyMethod("rstrip")]
		public static MutableString StripRight(RubyContext context, MutableString self)
		{
			return Strip(self, false, true);
		}

		[RubyMethod("strip!")]
		public static MutableString StripInPlace(RubyContext context, MutableString self)
		{
			return StripInPlace(self, true, true);
		}

		[RubyMethod("lstrip!")]
		public static MutableString StripLeftInPlace(RubyContext context, MutableString self)
		{
			return StripInPlace(self, true, false);
		}

		[RubyMethod("rstrip!")]
		public static MutableString StripRightInPlace(RubyContext context, MutableString self)
		{
			return StripInPlace(self, false, true);
		}

		private static MutableString Strip(MutableString str, bool trimLeft, bool trimRight)
		{
			int leftIndex;
			int rightIndex;
			GetTrimRange(str, trimLeft, trimRight, out leftIndex, out rightIndex);
			return str.GetSlice(leftIndex, rightIndex - leftIndex).TaintBy(str);
		}

		public static MutableString StripInPlace(MutableString self, bool trimLeft, bool trimRight)
		{
			int leftIndex;
			int rightIndex;
			GetTrimRange(self, trimLeft, trimRight, out leftIndex, out rightIndex);
			int num = rightIndex - leftIndex;
			if (num == self.Length)
			{
				self.RequireNotFrozen();
				return null;
			}
			if (num == 0)
			{
				self.Clear();
			}
			else
			{
				self.Trim(leftIndex, num);
			}
			return self;
		}

		private static void GetTrimRange(MutableString str, bool left, bool right, out int leftIndex, out int rightIndex)
		{
			GetTrimRange(str.Length, (!left) ? null : ((Func<int, bool>)((int i) => char.IsWhiteSpace(str.GetChar(i)))), (!right) ? null : ((Func<int, bool>)delegate(int i)
			{
				char @char = str.GetChar(i);
				return char.IsWhiteSpace(@char) || @char == '\0';
			}), out leftIndex, out rightIndex);
		}

		internal static void GetTrimRange(int length, Func<int, bool> trimLeft, Func<int, bool> trimRight, out int leftIndex, out int rightIndex)
		{
			int i;
			if (trimLeft != null)
			{
				for (i = 0; i < length && trimLeft(i); i++)
				{
				}
			}
			else
			{
				i = 0;
			}
			int num;
			if (trimRight != null)
			{
				num = length - 1;
				while (num >= i && trimRight(num))
				{
					num--;
				}
				num++;
			}
			else
			{
				num = length;
			}
			leftIndex = i;
			rightIndex = num;
		}

		[RubyMethod("squeeze")]
		public static MutableString Squeeze(RubyContext context, MutableString self, [NotNullItems][DefaultProtocol] params MutableString[] args)
		{
			MutableString mutableString = self.Clone();
			SqueezeMutableString(mutableString, args);
			return mutableString;
		}

		[RubyMethod("squeeze!")]
		public static MutableString SqueezeInPlace(RubyContext context, MutableString self, [NotNullItems][DefaultProtocol] params MutableString[] args)
		{
			return SqueezeMutableString(self, args);
		}

		private static MutableString SqueezeMutableString(MutableString str, MutableString[] ranges)
		{
			BitArray bitArray = null;
			if (ranges.Length > 0)
			{
				bitArray = new RangeParser(ranges).Parse();
			}
			int num = 1;
			int num2 = 1;
			while (num < str.Length)
			{
				if (str.GetChar(num) == str.GetChar(num - 1) && (ranges.Length == 0 || bitArray.Get(str.GetChar(num))))
				{
					num++;
					continue;
				}
				str.SetChar(num2, str.GetChar(num));
				num++;
				num2++;
			}
			if (num > num2)
			{
				str.Remove(num2, num - num2);
			}
			if (num != num2)
			{
				return str;
			}
			return null;
		}

		[RubyMethod("to_i")]
		public static object ToInteger(MutableString self, [DefaultProtocol] int @base)
		{
			return ClrString.ToInteger(self.ConvertToString(), @base);
		}

		[RubyMethod("hex")]
		public static object ToIntegerHex(MutableString self)
		{
			return ClrString.ToIntegerHex(self.ConvertToString());
		}

		[RubyMethod("oct")]
		public static object ToIntegerOctal(MutableString self)
		{
			return ClrString.ToIntegerOctal(self.ConvertToString());
		}

		[RubyMethod("to_f")]
		public static double ToDouble(MutableString self)
		{
			return ClrString.ToDouble(self.ConvertToString());
		}

		[RubyMethod("to_str")]
		[RubyMethod("to_s")]
		public static MutableString ToS(MutableString self)
		{
			if (!(self.GetType() == typeof(MutableString)))
			{
				return MutableString.Create(self).TaintBy(self);
			}
			return self;
		}

		[RubyMethod("to_clr_string")]
		public static string ToClrString(MutableString str)
		{
			return str.ConvertToString();
		}

		[RubyMethod("intern")]
		[RubyMethod("to_sym")]
		public static RubySymbol ToSymbol(RubyContext context, MutableString self)
		{
			return context.CreateSymbol(self);
		}

		[RubyMethod("upto")]
		public static Enumerator UpTo(RangeOps.EachStorage storage, MutableString self, [NotNull][DefaultProtocol] MutableString endString)
		{
			return new Enumerator((RubyScope _, BlockParam block) => UpTo(storage, block, self, endString));
		}

		[RubyMethod("upto")]
		public static object UpTo(RangeOps.EachStorage storage, [NotNull] BlockParam block, MutableString self, [NotNull][DefaultProtocol] MutableString endString)
		{
			RangeOps.Each(storage, block, new Range(self, endString, false));
			return self;
		}

		[RubyMethod("replace")]
		public static MutableString Replace(MutableString self, [NotNull][DefaultProtocol] MutableString other)
		{
			if (object.ReferenceEquals(self, other))
			{
				self.RequireNotFrozen();
				return self;
			}
			self.Clear();
			self.Append(other);
			return self.TaintBy(other);
		}

		[RubyMethod("reverse")]
		public static MutableString GetReversed(MutableString self)
		{
			return self.Clone().Reverse();
		}

		[RubyMethod("reverse!")]
		public static MutableString Reverse(MutableString self)
		{
			self.RequireNotFrozen();
			if (self.IsEmpty)
			{
				return self;
			}
			return self.Reverse();
		}

		internal static MutableString Translate(MutableString src, MutableString from, MutableString to, bool inplace, bool squeeze, out bool anyCharacterMaps)
		{
			if (from.IsEmpty)
			{
				anyCharacterMaps = false;
				if (!inplace)
				{
					return src.Clone();
				}
				return src;
			}
			MutableString mutableString = ((!inplace) ? src.CreateInstance().TaintBy(src) : src);
			src.RequireCompatibleEncoding(from);
			mutableString.RequireCompatibleEncoding(to);
			from.PrepareForCharacterRead();
			to.PrepareForCharacterRead();
			CharacterMap map = CharacterMap.Create(from, to);
			if (to.IsEmpty)
			{
				anyCharacterMaps = MutableString.TranslateRemove(src, mutableString, map);
			}
			else if (squeeze)
			{
				anyCharacterMaps = MutableString.TranslateSqueeze(src, mutableString, map);
			}
			else
			{
				anyCharacterMaps = MutableString.Translate(src, mutableString, map);
			}
			return mutableString;
		}

		[RubyMethod("tr")]
		public static MutableString GetTranslated(MutableString self, [DefaultProtocol][NotNull] MutableString from, [NotNull][DefaultProtocol] MutableString to)
		{
			bool anyCharacterMaps;
			return Translate(self, from, to, false, false, out anyCharacterMaps);
		}

		[RubyMethod("tr!")]
		public static MutableString Translate(MutableString self, [NotNull][DefaultProtocol] MutableString from, [NotNull][DefaultProtocol] MutableString to)
		{
			self.RequireNotFrozen();
			bool anyCharacterMaps;
			Translate(self, from, to, true, false, out anyCharacterMaps);
			if (!anyCharacterMaps)
			{
				return null;
			}
			return self;
		}

		[RubyMethod("tr_s")]
		public static MutableString TrSqueeze(MutableString self, [NotNull][DefaultProtocol] MutableString from, [DefaultProtocol][NotNull] MutableString to)
		{
			bool anyCharacterMaps;
			return Translate(self, from, to, false, true, out anyCharacterMaps);
		}

		[RubyMethod("tr_s!")]
		public static MutableString TrSqueezeInPlace(MutableString self, [DefaultProtocol][NotNull] MutableString from, [DefaultProtocol][NotNull] MutableString to)
		{
			self.RequireNotFrozen();
			bool anyCharacterMaps;
			Translate(self, from, to, true, true, out anyCharacterMaps);
			if (!anyCharacterMaps)
			{
				return null;
			}
			return self;
		}

		[RubyMethod("ljust")]
		public static MutableString LeftJustify(MutableString self, [DefaultProtocol] int width)
		{
			return LeftJustify(self, width, _DefaultPadding);
		}

		[RubyMethod("ljust")]
		public static MutableString LeftJustify(MutableString self, [DefaultProtocol] int width, [DefaultProtocol][NotNull] MutableString padding)
		{
			if (padding.Length == 0)
			{
				throw RubyExceptions.CreateArgumentError("zero width padding");
			}
			int num = width - self.Length;
			if (num <= 0)
			{
				return self;
			}
			int num2 = num / padding.Length;
			int count = num % padding.Length;
			MutableString mutableString = self.Clone().TaintBy(padding);
			for (int i = 0; i < num2; i++)
			{
				mutableString.Append(padding);
			}
			mutableString.Append(padding, 0, count);
			return mutableString;
		}

		[RubyMethod("rjust")]
		public static MutableString RightJustify(MutableString self, [DefaultProtocol] int width)
		{
			return RightJustify(self, width, _DefaultPadding);
		}

		[RubyMethod("rjust")]
		public static MutableString RightJustify(MutableString self, [DefaultProtocol] int width, [NotNull][DefaultProtocol] MutableString padding)
		{
			if (padding.Length == 0)
			{
				throw RubyExceptions.CreateArgumentError("zero width padding");
			}
			int num = width - self.Length;
			if (num <= 0)
			{
				return self;
			}
			int num2 = num / padding.Length;
			int count = num % padding.Length;
			MutableString mutableString = self.CreateInstance().TaintBy(self).TaintBy(padding);
			for (int i = 0; i < num2; i++)
			{
				mutableString.Append(padding);
			}
			mutableString.Append(padding.GetSlice(0, count));
			mutableString.Append(self);
			return mutableString;
		}

		[RubyMethod("unpack")]
		public static RubyArray Unpack(MutableString self, [DefaultProtocol][NotNull] MutableString format)
		{
			return RubyEncoder.Unpack(self, format);
		}

		[RubyMethod("sum")]
		public static object GetChecksum(MutableString self, [DefaultProtocol] int bitCount)
		{
			int byteCount = self.GetByteCount();
			uint num = ((bitCount > 31) ? uint.MaxValue : ((uint)((1 << bitCount) - 1)));
			uint num2 = 0u;
			for (int i = 0; i < byteCount; i++)
			{
				byte @byte = self.GetByte(i);
				try
				{
					num2 = checked(num2 + @byte) & num;
				}
				catch (OverflowException)
				{
					return GetBigChecksum(self, i, num2, bitCount);
				}
			}
			if (num2 <= int.MaxValue)
			{
				return (int)num2;
			}
			return (BigInteger)num2;
		}

		private static BigInteger GetBigChecksum(MutableString self, int start, BigInteger sum, int bitCount)
		{
			BigInteger bigInteger = ((BigInteger)1 << bitCount) - 1;
			int byteCount = self.GetByteCount();
			for (int i = start; i < byteCount; i++)
			{
				sum = (sum + self.GetByte(i)) & bigInteger;
			}
			return sum;
		}

		private static void RequireNoVersionChange(MutableString self)
		{
			if (self.HasChanged)
			{
				throw RubyExceptions.CreateRuntimeError("string modified");
			}
		}
	}
}
