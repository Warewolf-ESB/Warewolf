namespace IronRuby.Compiler
{
	internal sealed class MethodLexicalScope : LexicalScope
	{
		public MethodLexicalScope(LexicalScope outerScope)
			: base(outerScope)
		{
		}
	}
}
