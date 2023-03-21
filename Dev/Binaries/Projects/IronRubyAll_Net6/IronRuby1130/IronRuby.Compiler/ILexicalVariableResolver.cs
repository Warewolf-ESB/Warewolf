namespace IronRuby.Compiler
{
	public interface ILexicalVariableResolver
	{
		bool IsLocalVariable(string identifier);
	}
}
