using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    // ReSharper disable InconsistentNaming
    public interface IRabbitMQSource : IResource
    // ReSharper restore InconsistentNaming
    {
        string Host { get; set; }
        int Port { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string VirtualHost { get; set; }
    }
}