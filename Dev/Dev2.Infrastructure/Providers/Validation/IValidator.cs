using System.Collections.Generic;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation
{
    public interface IValidator
    {
        IEnumerable<IErrorInfo> ValidationErrors();
    }
}