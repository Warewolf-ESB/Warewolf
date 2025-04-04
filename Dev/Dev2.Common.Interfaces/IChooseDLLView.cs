#pragma warning disable
#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.ViewEngines;
#else
using Prism.Mvvm;
#endif


namespace Dev2.Common.Interfaces
{
    public interface IChooseDLLView : IView
    {
        void ShowView(IDLLChooser chooser);
        void RequestClose();
    }
}
