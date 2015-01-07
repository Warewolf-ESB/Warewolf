using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Toolbox;

namespace Dev2.Common.Interfaces
{
    public interface IServer
    {
        bool Connect();
        IList<IResource> Load();
        IList<IConnection> GetServerConnections();
        IList<IToolDescriptor> LoadTools();
 
    }
}