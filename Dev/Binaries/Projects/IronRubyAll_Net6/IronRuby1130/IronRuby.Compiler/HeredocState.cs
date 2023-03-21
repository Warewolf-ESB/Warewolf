using System;

namespace IronRuby.Compiler
{
	internal sealed class HeredocState : HeredocStateBase, IEquatable<HeredocState>
	{
		private readonly int _resumePosition;

		private readonly int _resumeLineLength;

		private readonly char[] _resumeLine;

		private readonly int _firstLine;

		private readonly int _firstLineIndex;

		public int ResumePosition
		{
			get
			{
				return _resumePosition;
			}
		}

		public char[] ResumeLine
		{
			get
			{
				return _resumeLine;
			}
		}

		public int ResumeLineLength
		{
			get
			{
				return _resumeLineLength;
			}
		}

		public int FirstLine
		{
			get
			{
				return _firstLine;
			}
		}

		public int FirstLineIndex
		{
			get
			{
				return _firstLineIndex;
			}
		}

		internal HeredocState(StringProperties properties, string label, int resumePosition, char[] resumeLine, int resumeLineLength, int firstLine, int firstLineIndex)
			: base(properties, label)
		{
			_resumePosition = resumePosition;
			_resumeLine = resumeLine;
			_resumeLineLength = resumeLineLength;
			_firstLine = firstLine;
			_firstLineIndex = firstLineIndex;
		}

		public override bool Equals(object other)
		{
			return Equals(other as HeredocState);
		}

		public bool Equals(HeredocState other)
		{
			if (!object.ReferenceEquals(other, this))
			{
				if (other != null && Equals((HeredocStateBase)other) && _resumePosition == other._resumePosition && _firstLine == other._firstLine)
				{
					return _firstLineIndex == other._firstLineIndex;
				}
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return GetBaseHashCode() ^ _resumePosition ^ _firstLine ^ _firstLineIndex;
		}

		internal override Tokens TokenizeAndMark(Tokenizer tokenizer)
		{
			return tokenizer.TokenizeAndMarkHeredoc(this);
		}

		internal override Tokens Finish(Tokenizer tokenizer, int labelStart)
		{
			return tokenizer.FinishHeredoc(this, labelStart);
		}

		public override string ToString()
		{
			return string.Format("Heredoc({0},'{1}',{2},'{3}')", base.Properties, base.Label, _resumePosition, new string(_resumeLine));
		}
	}
}
