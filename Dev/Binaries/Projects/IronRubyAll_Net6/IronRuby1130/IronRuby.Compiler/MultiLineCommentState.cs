namespace IronRuby.Compiler
{
	internal sealed class MultiLineCommentState : TokenSequenceState
	{
		internal static readonly MultiLineCommentState Instance = new MultiLineCommentState();

		private MultiLineCommentState()
		{
		}

		internal override Tokens TokenizeAndMark(Tokenizer tokenizer)
		{
			tokenizer.MarkTokenStart();
			Tokens result = tokenizer.TokenizeMultiLineComment(false);
			tokenizer.CaptureTokenSpan();
			return result;
		}
	}
}
