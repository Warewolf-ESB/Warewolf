namespace IronRuby.Compiler
{
	internal sealed class ParserTables
	{
		public State[] States;

		public int[] Rules;

		public int ErrorToken;

		public int EofToken;
	}
}
