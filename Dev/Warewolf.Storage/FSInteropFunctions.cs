namespace Warewolf.Storage
{
    public static class FsInteropFunctions
    {
        public static LanguageAST.LanguageExpression ParseLanguageExpression(string sourceName, int update) => EvaluationFunctions.parseLanguageExpression(sourceName, update);

        public static LanguageAST.LanguageExpression ParseLanguageExpressionWithoutUpdate(string expression) => EvaluationFunctions.parseLanguageExpressionWithoutUpdate(expression);

        public static string LanguageExpressionToString(LanguageAST.LanguageExpression exp) => EvaluationFunctions.languageExpressionToString(exp);

        public static string PositionColumn => EvaluationFunctions.PositionColumn;
    }
}
