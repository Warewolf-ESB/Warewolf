using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class RuleSet
    {
        public RuleSet(IEnumerable<RuleBase> rules = null)
        {
            Rules = new List<RuleBase>();
            if(rules != null)
            {
                Rules.AddRange(rules.Where(r => r != null));
            }
        }

        public void Add(RuleBase rule)
        {
            if(rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            Rules.Add(rule);
        }

        internal List<RuleBase> Rules { get; set; }

        public List<IActionableErrorInfo> ValidateRules()
        {
            var errors = from rule in Rules
                         let errorTO = rule.Check()
                         where errorTO != null
                         select errorTO;
            return errors.ToList();
        }

        public List<IActionableErrorInfo> ValidateRules(string labelText, Action doError)
        {
            var result = new List<IActionableErrorInfo>();
            foreach(var rule in Rules)
            {
                rule.LabelText = labelText;
                rule.DoError = doError;
                var err = rule.Check();
                if(err != null)
                {
                    result.Add(err);
                }
            }
            return result;
        }
    }
}