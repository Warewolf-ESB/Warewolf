using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Studio.Core
{
    public interface IViewFactory
    {
        IView GetViewGivenServerResourceType(string resourceModel);
    }
}