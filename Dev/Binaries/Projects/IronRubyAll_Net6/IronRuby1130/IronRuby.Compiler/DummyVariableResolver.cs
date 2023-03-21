namespace IronRuby.Compiler
{
	internal sealed class DummyVariableResolver : ILexicalVariableResolver
	{
		public static readonly ILexicalVariableResolver AllVariableNames = new DummyVariableResolver(true);

		public static readonly ILexicalVariableResolver AllMethodNames = new DummyVariableResolver(false);

		private readonly bool _isVariable;

		private DummyVariableResolver(bool isVariable)
		{
			_isVariable = isVariable;
		}

		public bool IsLocalVariable(string identifier)
		{
			return _isVariable;
		}
	}
}
