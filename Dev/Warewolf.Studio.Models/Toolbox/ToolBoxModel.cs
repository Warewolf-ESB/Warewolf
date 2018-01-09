using System.Collections.Generic;
using Dev2;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Studio.Interfaces;



namespace Warewolf.Studio.Models.Toolbox
{
    public class ToolboxModel:IToolboxModel
    {
        readonly IServer _localServer;
        readonly IPluginProxy _pluginProxy;
        public ToolboxModel(IServer server, IServer localServer, IPluginProxy pluginProxy)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>{{"server",server},{"localServer",localServer}});
            _localServer = localServer;
            _pluginProxy = pluginProxy;
            Server = server;
        }

        public IServer Server { get; set; }


        #region Implementation of IToolboxModel

        /// <summary>
        /// gets the description of tools from the connected server
        /// </summary>
        /// <returns></returns>
        public IList<IToolDescriptor> GetTools()
        {
            return Server.LoadTools();
        }

        /// <summary>
        /// Is connected.
        /// </summary>
        /// <returns></returns>
        public bool IsEnabled()
        {
            return Server.IsConnected && _localServer.IsConnected; 
        }

#pragma warning disable 0067
        public event ServerDisconnected OnserverDisconnected;
#pragma warning restore 0067

        #endregion
    }
}
