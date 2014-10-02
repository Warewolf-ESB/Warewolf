
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;

namespace Dev2.Providers.Validation.Rules
{
    public class RuleSet : IRuleSet
    {
        public RuleSet(IEnumerable<RuleBase> rules = null)
        {
            Rules = new List<IRuleBase>();
            if(rules != null)
            {
                Rules.AddRange(rules.Where(r => r != null));
            }
        }

        public void Add(IRuleBase rule)
        {
            if(rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            Rules.Add(rule);
        }

        public List<IRuleBase> Rules { get; set; }

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
