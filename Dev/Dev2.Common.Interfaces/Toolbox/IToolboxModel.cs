using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Toolbox
{
    public delegate void ServerDisconnected(object sender);
    public interface IToolboxModel
    {
        IList<IToolDescriptor> GetTools();
        
        bool IsEnabled();

        event ServerDisconnected OnserverDisconnected;
    }
}