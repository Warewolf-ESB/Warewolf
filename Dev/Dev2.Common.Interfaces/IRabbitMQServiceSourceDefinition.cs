using System;



namespace Dev2.Common.Interfaces
{
    public interface IRabbitMQServiceSourceDefinition : IEquatable<IRabbitMQServiceSourceDefinition>
    {
        string HostName { get; set; }
        int Port { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string VirtualHost { get; set; }
        string ResourcePath { get; set; }
        Guid ResourceID { get; set; }
        string ResourceName { get; set; }
    }
}