using System;
using System.Collections.Generic;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public interface IRuleSet
    {
        void Add(RuleBase rule);

        List<IActionableErrorInfo> ValidateRules();

        List<IActionableErrorInfo> ValidateRules(string labelText, Action doError);

        List<RuleBase> Rules { get; set; }
    }
}