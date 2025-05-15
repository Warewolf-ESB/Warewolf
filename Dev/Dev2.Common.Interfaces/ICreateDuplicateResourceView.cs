#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.ViewEngines;
#else
using Prism.Mvvm;
#endif

namespace Dev2.Common.Interfaces
{
    public interface ICreateDuplicateResourceView : IView
    {
        void ShowView();

        void CloseView();
    }
}