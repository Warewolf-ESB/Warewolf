using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Common.Interfaces.Infrastructure.Providers.Validation
{
    public interface IRuleSet
    {
        void Add(IRuleBase rule);

        List<IActionableErrorInfo> ValidateRules();

        List<IActionableErrorInfo> ValidateRules(string labelText, Action doError);

        List<IRuleBase> Rules { get; set; }
    }
}