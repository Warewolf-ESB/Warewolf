using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.StringScanner
{
	[RubyClass("StringScanner")]
	public sealed class StringScanner : RubyObject
	{
		private MutableString _scanString;

		private int _previousPosition;

		private int _currentPosition;

		private int _foundPosition;

		private MutableString _lastMatch;

		private MatchData _lastMatchingGroups;

		private int PreviousPosition
		{
			get
			{
				return _previousPosition;
			}
			set
			{
				_previousPosition = value;
			}
		}

		private int CurrentPosition
		{
			get
			{
				return _currentPosition;
			}
			set
			{
				_currentPosition = value;
			}
		}

		private int Length
		{
			get
			{
				return _scanString.Length;
			}
		}

		private MutableString ScanString
		{
			get
			{
				return _scanString;
			}
			set
			{
				_scanString = value;
			}
		}

		private int FoundPosition
		{
			get
			{
				return _foundPosition;
			}
			set
			{
				_foundPosition = value;
			}
		}

		private MutableString LastMatch
		{
			get
			{
				return _lastMatch;
			}
			set
			{
				_lastMatch = value;
			}
		}

		private MatchData LastMatchingGroups
		{
			get
			{
				return _lastMatchingGroups;
			}
		}

		public StringScanner(RubyClass rubyClass)
			: base(rubyClass)
		{
			_scanString = MutableString.FrozenEmpty;
		}

		public StringScanner(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		protected override RubyObject CreateInstance()
		{
			return new StringScanner(base.ImmediateClass.NominalClass);
		}

		private void InitializeFrom(StringScanner other)
		{
			_currentPosition = other._currentPosition;
			_foundPosition = other._foundPosition;
			_lastMatch = other._lastMatch;
			_lastMatchingGroups = other._lastMatchingGroups;
			_previousPosition = other._previousPosition;
			_scanString = other.ScanString;
		}

		[RubyConstructor]
		public static StringScanner Create(RubyClass self, [NotNull][DefaultProtocol] MutableString scan, [Optional] object ignored)
		{
			StringScanner stringScanner = new StringScanner(self);
			stringScanner.ScanString = scan;
			stringScanner.Reset();
			return stringScanner;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static void Reinitialize(StringScanner self, [NotNull][DefaultProtocol] MutableString scan, [Optional] object ignored)
		{
			self.ScanString = scan;
			self.Reset();
		}

		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		public static void InitializeFrom(StringScanner self, [NotNull][DefaultProtocol] StringScanner other)
		{
			self.InitializeFrom(other);
		}

		[RubyMethod("must_C_version", RubyMethodAttributes.PublicSingleton)]
		public static object MustCVersion(object self)
		{
			return self;
		}

		[RubyMethod("concat")]
		[RubyMethod("<<")]
		public static StringScanner Concat(StringScanner self, MutableString str)
		{
			self.ScanString.Append(str);
			return self;
		}

		[RubyMethod("[]")]
		public static MutableString GetMatchSubgroup(StringScanner self, int subgroup)
		{
			if (subgroup == 0 && self.LastMatch != null)
			{
				return MutableString.Create(self.LastMatch);
			}
			if (self.LastMatchingGroups == null)
			{
				return null;
			}
			if (subgroup < 0)
			{
				subgroup = self.LastMatchingGroups.GroupCount - subgroup;
			}
			if (subgroup >= self.LastMatchingGroups.GroupCount)
			{
				return null;
			}
			return self.LastMatchingGroups.GetGroupValue(subgroup);
		}

		[RubyMethod("beginning_of_line?")]
		[RubyMethod("bol?")]
		public static bool BeginningOfLine(StringScanner self)
		{
			if (self.CurrentPosition != 0)
			{
				return self.ScanString.GetChar(self.CurrentPosition - 1) == '\n';
			}
			return true;
		}

		[RubyMethod("check")]
		public static MutableString Check(StringScanner self, [NotNull] RubyRegex pattern)
		{
			return ScanFull(self, pattern, false, true) as MutableString;
		}

		[RubyMethod("check_until")]
		public static MutableString CheckUntil(StringScanner self, [NotNull] RubyRegex pattern)
		{
			return SearchFull(self, pattern, false, true) as MutableString;
		}

		[RubyMethod("empty?")]
		[RubyMethod("eos?")]
		public static bool EndOfLine(StringScanner self)
		{
			return self.CurrentPosition >= self.Length;
		}

		[RubyMethod("exist?")]
		public static int? Exist(StringScanner self, [NotNull] RubyRegex pattern)
		{
			if (!self.Match(pattern, false, false))
			{
				return null;
			}
			return self.FoundPosition + self.LastMatch.Length;
		}

		[RubyMethod("get_byte")]
		[RubyMethod("getbyte")]
		public static MutableString GetByte(StringScanner self)
		{
			if (self.CurrentPosition >= self.Length)
			{
				return null;
			}
			self.PreviousPosition = self.CurrentPosition;
			self.FoundPosition = self.CurrentPosition;
			self.LastMatch = self.ScanString.GetSlice(self.CurrentPosition++, 1);
			return MutableString.Create(self.LastMatch);
		}

		[RubyMethod("getch")]
		public static MutableString GetChar(StringScanner self)
		{
			if (self.CurrentPosition >= self.Length)
			{
				return null;
			}
			self.PreviousPosition = self.CurrentPosition;
			self.FoundPosition = self.CurrentPosition;
			self.LastMatch = self.ScanString.GetSlice(self.CurrentPosition++, 1);
			return MutableString.Create(self.LastMatch);
		}

		[RubyMethod("inspect")]
		[RubyMethod("to_s")]
		public static MutableString ToString(StringScanner self)
		{
			return MutableString.Create(self.ToString(), self._scanString.Encoding);
		}

		[RubyMethod("match?")]
		public static int? Match(StringScanner self, [NotNull] RubyRegex pattern)
		{
			if (!self.Match(pattern, true, false))
			{
				return null;
			}
			return self.LastMatch.GetLength();
		}

		[RubyMethod("matched")]
		public static MutableString Matched(StringScanner self)
		{
			if (self.LastMatch == null)
			{
				return null;
			}
			return MutableString.Create(self.LastMatch);
		}

		[RubyMethod("matched?")]
		public static bool WasMatched(StringScanner self)
		{
			return self.LastMatch != null;
		}

		[RubyMethod("matchedsize")]
		[RubyMethod("matched_size")]
		public static int? MatchedSize(StringScanner self)
		{
			if (self.LastMatch == null)
			{
				return null;
			}
			return self.LastMatch.Length;
		}

		[RubyMethod("peep")]
		[RubyMethod("peek")]
		public static MutableString Peek(StringScanner self, int len)
		{
			if (len < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative string size (or size too big)");
			}
			int num = self.Length - self.CurrentPosition;
			if (len > num)
			{
				len = num;
			}
			if (self.CurrentPosition >= self.Length || len == 0)
			{
				return MutableString.CreateEmpty();
			}
			return self.ScanString.GetSlice(self.CurrentPosition, len);
		}

		[RubyMethod("pointer")]
		[RubyMethod("pos")]
		public static int GetCurrentPosition(StringScanner self)
		{
			return self.CurrentPosition;
		}

		[RubyMethod("pos=")]
		[RubyMethod("pointer=")]
		public static int SetCurrentPosition(StringScanner self, int newPosition)
		{
			int num = newPosition;
			if (num < 0)
			{
				num = self.Length - self.CurrentPosition;
			}
			if (num > self.Length)
			{
				throw RubyExceptions.CreateRangeError("index out of range");
			}
			self.CurrentPosition = num;
			return newPosition;
		}

		[RubyMethod("post_match")]
		public static MutableString PostMatch(StringScanner self)
		{
			if (self.LastMatch == null)
			{
				return null;
			}
			int num = self.FoundPosition + self.LastMatch.Length;
			int num2 = self.Length - num;
			if (num2 <= 0)
			{
				return MutableString.CreateEmpty();
			}
			return self.ScanString.GetSlice(num, num2);
		}

		[RubyMethod("pre_match")]
		public static MutableString PreMatch(StringScanner self)
		{
			if (self.LastMatch == null)
			{
				return null;
			}
			return self.ScanString.GetSlice(0, self.FoundPosition);
		}

		[RubyMethod("reset")]
		public static StringScanner Reset(StringScanner self)
		{
			self.Reset();
			return self;
		}

		[RubyMethod("rest")]
		public static MutableString Rest(StringScanner self)
		{
			int num = self.Length - self.CurrentPosition;
			if (num <= 0)
			{
				return MutableString.CreateEmpty();
			}
			return self.ScanString.GetSlice(self.CurrentPosition, num);
		}

		[RubyMethod("rest?")]
		public static bool IsRestLeft(StringScanner self)
		{
			return self.CurrentPosition < self.Length;
		}

		[RubyMethod("rest_size")]
		[RubyMethod("restsize")]
		public static int RestSize(StringScanner self)
		{
			if (self.CurrentPosition >= self.Length)
			{
				return 0;
			}
			return self.Length - self.CurrentPosition;
		}

		[RubyMethod("scan")]
		public static object Scan(StringScanner self, [NotNull] RubyRegex pattern)
		{
			return ScanFull(self, pattern, true, true);
		}

		[RubyMethod("scan_full")]
		public static object ScanFull(StringScanner self, [NotNull] RubyRegex pattern, bool advancePointer, bool returnString)
		{
			if (self.Match(pattern, true, advancePointer))
			{
				if (returnString)
				{
					return MutableString.Create(self.LastMatch);
				}
				return ScriptingRuntimeHelpers.Int32ToObject(self.LastMatch.Length);
			}
			return null;
		}

		[RubyMethod("scan_until")]
		public static object ScanUntil(StringScanner self, [NotNull] RubyRegex pattern)
		{
			return SearchFull(self, pattern, true, true);
		}

		[RubyMethod("search_full")]
		public static object SearchFull(StringScanner self, [NotNull] RubyRegex pattern, bool advancePointer, bool returnString)
		{
			if (self.Match(pattern, false, advancePointer))
			{
				int num = self.LastMatch.Length + (self.FoundPosition - self.PreviousPosition);
				if (returnString)
				{
					return self.ScanString.GetSlice(self.PreviousPosition, num);
				}
				return ScriptingRuntimeHelpers.Int32ToObject(num);
			}
			return null;
		}

		[RubyMethod("skip")]
		public static int? Skip(StringScanner self, [NotNull] RubyRegex pattern)
		{
			if (!self.Match(pattern, true, true))
			{
				return null;
			}
			return self.CurrentPosition - self.PreviousPosition;
		}

		[RubyMethod("skip_until")]
		public static int? SkipUntil(StringScanner self, [NotNull] RubyRegex pattern)
		{
			if (!self.Match(pattern, false, true))
			{
				return null;
			}
			return self.CurrentPosition - self.PreviousPosition;
		}

		[RubyMethod("string")]
		public static MutableString GetString(StringScanner self)
		{
			return self.ScanString;
		}

		[RubyMethod("string=")]
		public static MutableString SetString(RubyContext context, StringScanner self, [NotNull] MutableString str)
		{
			self.ScanString = (MutableString)KernelOps.Freeze(context, MutableString.Create(str));
			self.Reset();
			return str;
		}

		[RubyMethod("clear")]
		[RubyMethod("terminate")]
		public static StringScanner Clear(StringScanner self)
		{
			self.Reset();
			self.CurrentPosition = self.Length;
			return self;
		}

		[RubyMethod("unscan")]
		public static StringScanner Unscan(StringScanner self)
		{
			if (self.LastMatch == null)
			{
				throw RubyExceptions.CreateRangeError("unscan failed: previous match had failed");
			}
			int previousPosition = self.PreviousPosition;
			self.Reset();
			self.CurrentPosition = previousPosition;
			return self;
		}

		private bool Match(RubyRegex pattern, bool currentPositionOnly, bool advancePosition)
		{
			MatchData matchData = pattern.Match(_scanString, _currentPosition, false);
			_lastMatch = null;
			_lastMatchingGroups = null;
			_foundPosition = 0;
			if (matchData == null)
			{
				return false;
			}
			if (currentPositionOnly && matchData.Index != _currentPosition)
			{
				return false;
			}
			int num = matchData.Index - _currentPosition + matchData.Length;
			_foundPosition = matchData.Index;
			_previousPosition = _currentPosition;
			_lastMatch = _scanString.GetSlice(_foundPosition, matchData.Length);
			_lastMatchingGroups = matchData;
			if (advancePosition)
			{
				_currentPosition += num;
			}
			return true;
		}

		private void Reset()
		{
			_previousPosition = 0;
			_currentPosition = 0;
			_foundPosition = 0;
			_lastMatch = null;
			_lastMatchingGroups = null;
		}

		public override string ToString()
		{
			byte[] array = ScanString.ToByteArray();
			StringBuilder stringBuilder = new StringBuilder("#<StringScanner ");
			if (CurrentPosition >= Length || CurrentPosition < 0)
			{
				stringBuilder.Append("fin >");
				return stringBuilder.ToString();
			}
			stringBuilder.AppendFormat("{0}/{1}", CurrentPosition, array.Length);
			if (CurrentPosition > 0)
			{
				stringBuilder.Append(" \"");
				int num = CurrentPosition;
				if (num > 5)
				{
					num = 5;
					stringBuilder.Append("...");
				}
				for (int i = CurrentPosition - num; i < CurrentPosition; i++)
				{
					MutableString.AppendCharRepresentation(stringBuilder, array[i], -1, MutableString.Escape.NonAscii | MutableString.Escape.Special, 34, -1);
				}
				stringBuilder.Append('"');
			}
			stringBuilder.Append(" @ ");
			if (CurrentPosition < array.Length)
			{
				int num2 = array.Length - CurrentPosition;
				bool flag = false;
				if (num2 > 5)
				{
					num2 = 5;
					flag = true;
				}
				stringBuilder.Append('"');
				for (int j = CurrentPosition; j < CurrentPosition + num2; j++)
				{
					MutableString.AppendCharRepresentation(stringBuilder, array[j], -1, MutableString.Escape.NonAscii | MutableString.Escape.Special, 34, -1);
				}
				if (flag)
				{
					stringBuilder.Append("...");
				}
				stringBuilder.Append('"');
			}
			stringBuilder.Append('>');
			return stringBuilder.ToString();
		}
	}
}
