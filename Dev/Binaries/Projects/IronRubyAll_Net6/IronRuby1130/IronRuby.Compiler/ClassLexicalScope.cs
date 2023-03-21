namespace IronRuby.Compiler
{
	internal sealed class ClassLexicalScope : LexicalScope
	{
		public ClassLexicalScope(LexicalScope outerScope)
			: base(outerScope)
		{
		}
	}
}
