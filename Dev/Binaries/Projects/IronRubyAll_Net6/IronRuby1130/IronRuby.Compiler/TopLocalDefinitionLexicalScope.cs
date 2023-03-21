namespace IronRuby.Compiler
{
	internal sealed class TopLocalDefinitionLexicalScope : LexicalScope
	{
		public TopLocalDefinitionLexicalScope(LexicalScope outerScope)
			: base(outerScope)
		{
		}
	}
}
