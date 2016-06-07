using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Toolbox
{
    public delegate void ServerDisconnected(object sender);
    public interface IToolboxModel
    {
        //IServer Server{get;}
        /// <summary>
        /// gets the description of tools from the connected server
        /// </summary>
        /// <returns></returns>
        IList<IToolDescriptor> GetTools();

        /// <summary>
        /// Is connected.
        /// </summary>
        /// <returns></returns>
        bool IsEnabled();

        event ServerDisconnected OnserverDisconnected;

    }
}