
using Dev2.Common.Interfaces.Infrastructure;

namespace Dev2.Providers.Errors
{
    public interface IActionableErrorInfo : IErrorInfo
    {
        void Do();
    }
}