using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface INewItemMessage
    {
        IExplorerItemViewModel Parent{get;}

        ResourceType Type { get;  }

    }
}