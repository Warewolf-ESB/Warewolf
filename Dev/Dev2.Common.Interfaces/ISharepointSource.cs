using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces
{
    public interface ISharepointSource : IResource
    {
        string Server { get; set; }
        AuthenticationType AuthenticationType { get; set; }

        string UserName { get; set; }
        string Password { get; set; }

    }

    
}