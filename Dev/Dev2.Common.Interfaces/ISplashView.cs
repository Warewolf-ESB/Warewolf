using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Dev2.Common.Interfaces
{
    public interface ISplashView : IView
    {
        void Show(bool isDialog);

        void CloseSplash(bool studioShutdown);
    }
}