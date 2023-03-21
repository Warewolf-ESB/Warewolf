using System;

namespace IronRuby.Compiler
{
	internal sealed class VerbatimHeredocState : HeredocStateBase, IEquatable<VerbatimHeredocState>
	{
		public VerbatimHeredocState(StringProperties properties, string label)
			: base(properties, label)
		{
		}

		public override bool Equals(object other)
		{
			return Equals(other as VerbatimHeredocState);
		}

		public bool Equals(VerbatimHeredocState other)
		{
			if (!object.ReferenceEquals(other, this))
			{
				if (other != null)
				{
					return Equals((HeredocStateBase)other);
				}
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return GetBaseHashCode();
		}

		internal override Tokens TokenizeAndMark(Tokenizer tokenizer)
		{
			return tokenizer.TokenizeAndMarkHeredoc(this);
		}

		internal override Tokens Finish(Tokenizer tokenizer, int labelStart)
		{
			return tokenizer.FinishVerbatimHeredoc(this, labelStart);
		}

		public override string ToString()
		{
			return string.Format("VerbatimHeredoc({0},'{1}')", base.Properties, base.Label);
		}
	}
}
