using System.Diagnostics;
using System.Text.RegularExpressions;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	public class MatchData : IDuplicable, IRubyObjectState
	{
		[DebuggerTypeProxy(typeof(RubyObjectDebugView))]
		[DebuggerDisplay("{_immediateClass.GetDebuggerDisplayValue(this),nq}", Type = "{_immediateClass.GetDebuggerDisplayType(),nq}")]
		public sealed class Subclass : MatchData, IRubyObject, IRubyObjectState
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

			public RubyInstanceData GetInstanceData()
			{
				return RubyOps.GetInstanceData(ref _instanceData);
			}

			public RubyInstanceData TryGetInstanceData()
			{
				return _instanceData;
			}

			public int BaseGetHashCode()
			{
				return base.GetHashCode();
			}

			public bool BaseEquals(object other)
			{
				return base.Equals(other);
			}

			public string BaseToString()
			{
				return base.ToString();
			}

			public Subclass(RubyClass rubyClass)
			{
				ImmediateClass = rubyClass;
			}

			protected override MatchData CreateInstance()
			{
				return new Subclass(ImmediateClass.NominalClass);
			}
		}

		private const int FrozenFlag = 1;

		private const int TaintedFlag = 2;

		private const int UntrustedFlag = 4;

		private int _flags;

		private Match _match;

		private MutableString _originalString;

		public MutableString OriginalString
		{
			get
			{
				return _originalString;
			}
		}

		public RubyEncoding Encoding
		{
			get
			{
				return _originalString.Encoding;
			}
		}

		public int GroupCount
		{
			get
			{
				return _match.Groups.Count;
			}
		}

		public int Index
		{
			get
			{
				return GetGroupStart(0);
			}
		}

		public int Length
		{
			get
			{
				return GetGroupLength(0);
			}
		}

		public bool IsFrozen
		{
			get
			{
				return (_flags & 1) != 0;
			}
		}

		public bool IsTainted
		{
			get
			{
				return (_flags & 2) != 0;
			}
			set
			{
				_flags = (_flags & -3) | (value ? 2 : 0);
			}
		}

		public bool IsUntrusted
		{
			get
			{
				return (_flags & 4) != 0;
			}
			set
			{
				_flags = (_flags & -5) | (value ? 4 : 0);
			}
		}

		private MatchData(Match match, MutableString originalString)
		{
			_match = match;
			_originalString = originalString;
			IsTainted = originalString.IsTainted;
			IsUntrusted = originalString.IsUntrusted;
		}

		public MatchData()
		{
			_originalString = MutableString.FrozenEmpty;
			_match = Match.Empty;
		}

		protected MatchData(MatchData data)
			: this(data._match, data._originalString)
		{
		}

		object IDuplicable.Duplicate(RubyContext context, bool copySingletonMembers)
		{
			MatchData matchData = CreateInstance();
			context.CopyInstanceData(this, matchData, copySingletonMembers);
			return matchData;
		}

		protected virtual MatchData CreateInstance()
		{
			return new MatchData();
		}

		public void InitializeFrom(MatchData other)
		{
			_match = other._match;
			_originalString = other._originalString;
		}

		internal static MatchData Create(Match match, MutableString input, bool freezeInput, string encodedInput)
		{
			if (!match.Success)
			{
				return null;
			}
			if (freezeInput)
			{
				input = input.Clone().Freeze();
			}
			return new MatchData(match, input);
		}

		public void Freeze()
		{
			_flags |= 1;
		}

		public bool GroupSuccess(int index)
		{
			return _match.Groups[index].Success;
		}

		public MutableString GetValue()
		{
			if (!_match.Success)
			{
				return null;
			}
			return _originalString.GetSlice(Index, Length).TaintBy(this);
		}

		public MutableString GetGroupValue(int index)
		{
			if (!GroupSuccess(index))
			{
				return null;
			}
			return _originalString.GetSlice(GetGroupStart(index), GetGroupLength(index)).TaintBy(this);
		}

		public MutableString AppendGroupValue(int index, MutableString result)
		{
			if (!GroupSuccess(index))
			{
				return null;
			}
			return result.Append(_originalString, GetGroupStart(index), GetGroupLength(index)).TaintBy(this);
		}

		public int GetGroupStart(int groupIndex)
		{
			ContractUtils.Requires(groupIndex >= 0);
			return _match.Groups[groupIndex].Index;
		}

		public int GetGroupLength(int groupIndex)
		{
			ContractUtils.Requires(groupIndex >= 0);
			return _match.Groups[groupIndex].Length;
		}

		public int GetGroupEnd(int groupIndex)
		{
			ContractUtils.Requires(groupIndex >= 0);
			return GetGroupStart(groupIndex) + GetGroupLength(groupIndex);
		}

		public void RequireExistingGroup(int index)
		{
			if (index >= _match.Groups.Count || index < 0)
			{
				throw RubyExceptions.CreateIndexError("index {0} out of matches", index);
			}
		}

		public MutableString GetPreMatch()
		{
			return _originalString.GetSlice(0, GetGroupStart(0)).TaintBy(this);
		}

		public MutableString GetPostMatch()
		{
			return _originalString.GetSlice(GetGroupEnd(0)).TaintBy(this);
		}
	}
}
