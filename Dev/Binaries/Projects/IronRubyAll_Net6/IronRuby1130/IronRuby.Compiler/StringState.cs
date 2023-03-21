using System;

namespace IronRuby.Compiler
{
	internal sealed class StringState : TokenSequenceState, IEquatable<StringState>
	{
		private readonly int _nestingLevel;

		private readonly StringProperties _properties;

		private readonly char _terminator;

		private readonly char _openingParenthesis;

		public StringProperties Properties
		{
			get
			{
				return _properties;
			}
		}

		public int NestingLevel
		{
			get
			{
				return _nestingLevel;
			}
		}

		public char TerminatingCharacter
		{
			get
			{
				return _terminator;
			}
		}

		public char OpeningParenthesis
		{
			get
			{
				return _openingParenthesis;
			}
		}

		public StringState(StringProperties properties, char terminator)
			: this(properties, terminator, '\0', 0)
		{
		}

		public StringState(StringProperties properties, char terminator, char openingParenthesis, int nestingLevel)
		{
			_properties = properties;
			_terminator = terminator;
			_openingParenthesis = openingParenthesis;
			_nestingLevel = nestingLevel;
		}

		public override bool Equals(object other)
		{
			return Equals(other as StringState);
		}

		public bool Equals(StringState other)
		{
			if (!object.ReferenceEquals(other, this))
			{
				if (other != null && _nestingLevel == other._nestingLevel && _properties == other._properties && _terminator == other._terminator)
				{
					return _openingParenthesis == other._openingParenthesis;
				}
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return _nestingLevel ^ (int)_properties ^ _terminator ^ _openingParenthesis;
		}

		public StringState SetNesting(int level)
		{
			if (_nestingLevel != level)
			{
				return new StringState(_properties, _terminator, _openingParenthesis, level);
			}
			return this;
		}

		public override string ToString()
		{
			return string.Format("StringTerminator({0},{1},{2},{3},{4})", _properties, (int)_terminator, (int)_openingParenthesis, 0, _nestingLevel);
		}

		internal override Tokens TokenizeAndMark(Tokenizer tokenizer)
		{
			Tokens result = tokenizer.TokenizeString(this);
			tokenizer.CaptureTokenSpan();
			return result;
		}
	}
}
