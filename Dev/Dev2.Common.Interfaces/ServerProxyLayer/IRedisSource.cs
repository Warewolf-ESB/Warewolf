using System;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    public interface IRedisSource:IEquatable<IRedisSource>
    {
        string HostName { get; set; }
        string Password { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        Guid Id { get; set; }

    }
}
