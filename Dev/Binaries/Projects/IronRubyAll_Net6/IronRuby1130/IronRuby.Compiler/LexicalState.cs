namespace IronRuby.Compiler
{
	internal enum LexicalState : byte
	{
		EXPR_BEG,
		EXPR_END,
		EXPR_ARG,
		EXPR_CMDARG,
		EXPR_ENDARG,
		EXPR_MID,
		EXPR_FNAME,
		EXPR_ENDFN,
		EXPR_DOT,
		EXPR_CLASS,
		EXPR_VALUE
	}
}
