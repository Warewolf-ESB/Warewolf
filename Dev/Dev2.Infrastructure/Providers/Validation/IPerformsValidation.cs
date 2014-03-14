using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;

namespace Dev2.Providers.Validation
{
    public interface IPerformsValidation : IDataErrorInfo
    {
        Dictionary<string, List<IActionableErrorInfo>> Errors { get; set; }

        bool Validate(string propertyName, IRuleSet ruleSet);

        bool Validate(string propertyName, string datalist);
    }
}