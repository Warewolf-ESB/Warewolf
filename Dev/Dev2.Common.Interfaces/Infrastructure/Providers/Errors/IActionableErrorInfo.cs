
namespace Dev2.Common.Interfaces.Infrastructure.Providers.Errors
{
    public interface IActionableErrorInfo : IErrorInfo
    {
        void Do();
    }
}