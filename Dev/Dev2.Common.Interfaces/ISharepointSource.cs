using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    public interface ISharepointSource:IResource
    {
        string Server { get; set; }
    }

    
}