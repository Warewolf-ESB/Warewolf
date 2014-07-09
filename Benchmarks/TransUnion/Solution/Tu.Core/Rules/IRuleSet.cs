namespace Tu.Rules
{
    public interface IRuleSet
    {
        IRule GetRule(string ruleName, string fieldName);
    }
}