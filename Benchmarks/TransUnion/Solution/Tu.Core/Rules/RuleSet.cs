using System;
using Tu.Extensions;

namespace Tu.Rules
{
    public class RuleSet : IRuleSet
    {
        readonly Validator _validator = new Validator(new RegexUtilities());

        public IRule GetRule(string ruleName, string fieldName)
        {
            var type = Type.GetType(string.Format("Tu.Rules.{0}", ruleName));
            if(type != null)
            {
                var rule = (IRule)Activator.CreateInstance(type, _validator, fieldName);
                return rule;
            }
            return null;
        }
    }
}