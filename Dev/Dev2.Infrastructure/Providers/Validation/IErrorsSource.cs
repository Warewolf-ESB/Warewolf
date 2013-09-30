using System.Collections.Generic;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation
{
    public interface IErrorsSource
    {
        IList<IActionableErrorInfo> Errors { get; set; }
    }
}
