using System;

namespace IronRuby.Compiler
{
	internal sealed class CodeState : TokenSequenceState, IEquatable<CodeState>
	{
		internal readonly LexicalState LexicalState;

		internal readonly byte CommandMode;

		internal readonly byte WhitespaceSeen;

		public CodeState(LexicalState lexicalState, byte commandMode, byte whitespaceSeen)
		{
			LexicalState = lexicalState;
			CommandMode = commandMode;
			WhitespaceSeen = whitespaceSeen;
		}

		public override bool Equals(object other)
		{
			return Equals(other as CodeState);
		}

		public bool Equals(CodeState other)
		{
			if (!object.ReferenceEquals(other, this))
			{
				if (other != null && LexicalState == other.LexicalState && CommandMode == other.CommandMode)
				{
					return WhitespaceSeen == other.WhitespaceSeen;
				}
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return (int)LexicalState ^ (int)CommandMode ^ WhitespaceSeen;
		}
	}
}
