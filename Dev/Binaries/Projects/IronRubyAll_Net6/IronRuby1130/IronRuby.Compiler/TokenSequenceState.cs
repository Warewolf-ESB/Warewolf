namespace IronRuby.Compiler
{
	internal class TokenSequenceState
	{
		internal static readonly TokenSequenceState None = new TokenSequenceState();

		internal virtual Tokens TokenizeAndMark(Tokenizer tokenizer)
		{
			Tokens result = tokenizer.Tokenize();
			tokenizer.CaptureTokenSpan();
			return result;
		}

		public override string ToString()
		{
			return "";
		}
	}
}
