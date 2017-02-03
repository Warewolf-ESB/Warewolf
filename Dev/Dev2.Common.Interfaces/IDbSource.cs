using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces
{
    public interface IDb : IResource
    {
        enSourceType ServerType { get; set; }
        string Server { get; set; }
        string DatabaseName { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string UserID { get; set; }
        string Password { get; set; }
    }
}