using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    public interface IShellViewModel
    {
        void AddService(IResource resource);
        void NewResource(ResourceType? type);
    }
}