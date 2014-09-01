using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Common.Interfaces.Infrastructure.Providers.Validation
{
    public interface IPerformsValidation : IDataErrorInfo
    {
        Dictionary<string, List<IActionableErrorInfo>> Errors { get; set; }

        bool Validate(string propertyName, IRuleSet ruleSet);

        bool Validate(string propertyName, string datalist);
    }
}