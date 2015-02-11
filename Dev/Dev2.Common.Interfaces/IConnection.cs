using Dev2.Common.Interfaces.Runtime.ServiceModel;

namespace Dev2.Common.Interfaces
{
    public interface IConnection
    {
        string Address { get; set; }
        string WebAddress { get; }
        AuthenticationType AuthenticationType { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        int WebServerPort { get; set; }
    }
}