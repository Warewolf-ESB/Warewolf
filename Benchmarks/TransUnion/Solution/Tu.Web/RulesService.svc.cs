
using System;
using Tu.Rules;

namespace Tu
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RulesService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select RulesService.svc or RulesService.svc.cs at the Solution Explorer and start debugging.
    public class RulesService : IRulesService
    {
        readonly IRuleSet _ruleSet;

        public RulesService()
            : this(new RuleSet())
        {
        }

        public RulesService(IRuleSet ruleSet)
        {
            if(ruleSet == null)
            {
                throw new ArgumentNullException("ruleSet");
            }
            _ruleSet = ruleSet;
        }

        public ValidationResult IsValid(string ruleName, string fieldName, object fieldValue)
        {
            if(string.IsNullOrEmpty(ruleName))
            {
                throw new ArgumentNullException("ruleName");
            }
            if(string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }

            var result = new ValidationResult();
            var rule = _ruleSet.GetRule(ruleName, fieldName);

            if(rule != null)
            {
                result.IsValid = rule.IsValid(fieldValue);
                result.Errors.AddRange(rule.Errors);
            }
            else
            {
                result.IsValid = true;
            }
            return result;
        }
    }
}
