using System;

namespace Dev2.Common.Interfaces.Infrastructure.Providers.Validation
{
    public interface IRuleBase
    {
        string LabelText { get; set; }
        string ErrorText { get; set; }
        Action DoError { get; set; }

        Dev2.Common.Interfaces.Infrastructure.Providers.Errors.IActionableErrorInfo Check();
    }
}