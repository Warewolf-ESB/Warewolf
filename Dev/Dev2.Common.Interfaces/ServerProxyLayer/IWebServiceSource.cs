﻿using System;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    public interface IWebServiceSource:IEquatable<IWebServiceSource>
    {
        string HostName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string DefaultQuery { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        Guid Id { get; set; }

    }
}
