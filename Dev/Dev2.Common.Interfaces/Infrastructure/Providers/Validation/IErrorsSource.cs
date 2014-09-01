using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Common.Interfaces.Infrastructure.Providers.Validation
{
    public interface IErrorsSource
    {
        List<IActionableErrorInfo> Errors { get; set; }
    }
}
