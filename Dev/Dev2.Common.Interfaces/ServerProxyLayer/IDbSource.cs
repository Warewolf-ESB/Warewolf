using System;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Runtime.ServiceModel;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    public interface IDbSource
    {
        string ServerName { get; set; }
        enSourceType Type { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string DbName{get;set;}
        string Name { get; set; }
        string Path { get; set; }
        Guid Id { get; set; }

    }


}