using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    public interface IRabbitMQ : IResource
    {
        string HostName { get; set; }
        int Port { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string VirtualHost { get; set; }
    }
}