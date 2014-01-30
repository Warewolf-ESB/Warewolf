using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class RuleSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RuleSet()
        {
            Rules = new List<RuleBase>();
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

        
    }
}