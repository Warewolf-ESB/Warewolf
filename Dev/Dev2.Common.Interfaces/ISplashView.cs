#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.ViewEngines;
#else
using Microsoft.Practices.Prism.Mvvm;
#endif

namespace Dev2.Common.Interfaces
{
    public interface ISplashView : IView
    {
        void Show(bool isDialog);

        void CloseSplash(bool studioShutdown);
    }
}