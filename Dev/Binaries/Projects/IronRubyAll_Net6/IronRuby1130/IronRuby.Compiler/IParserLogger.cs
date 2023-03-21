namespace IronRuby.Compiler
{
	internal interface IParserLogger
	{
		void BeforeReduction(int ruleId, int rhsLength);

		void BeforeShift(int stateId, int tokenId, bool isErrorShift);

		void BeforeGoto(int stateId, int ruleId);

		void StateEntered();

		void NextToken(int tokenId);
	}
}
