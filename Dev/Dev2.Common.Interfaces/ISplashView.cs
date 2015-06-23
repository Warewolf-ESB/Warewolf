using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface ISplashView : IView
    {
        void Show(bool isDialog);

        void CloseSplash();
    }
}