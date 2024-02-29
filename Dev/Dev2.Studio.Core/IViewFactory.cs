#if NETFRAMEWORK
using Microsoft.Practices.Prism.Mvvm;
#else
using Microsoft.AspNetCore.Mvc.ViewEngines;
#endif

namespace Dev2.Studio.Core
{
    public interface IViewFactory
    {
        IView GetViewGivenServerResourceType(string resourceModel);
    }
}