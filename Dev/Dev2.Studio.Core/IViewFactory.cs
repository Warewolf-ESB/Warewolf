
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Dev2.Studio.Core
{
    public interface IViewFactory
    {
        IView GetViewGivenServerResourceType(string resourceModel);
    }
}