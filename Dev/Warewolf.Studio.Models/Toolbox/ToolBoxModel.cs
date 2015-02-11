using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Toolbox;

namespace Warewolf.Studio.Models.Toolbox
{
    public class ToolboxModel:IToolboxModel
    {
        readonly IServer _localServer;
        readonly IPluginProxy _pluginProxy;
        public ToolboxModel(IServer server, IServer localServer, IPluginProxy pluginProxy)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>{{"server",server},{"localServer",localServer},{"pluginProxy",pluginProxy}});
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
        /// Checks if a tool is supported by confirming if the local server has the same tool
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public bool IsToolSupported(IToolDescriptor tool)
        {
            VerifyArgument.IsNotNull("tool",tool);
            return _localServer.LoadTools().Contains(tool);
        }

        ///  <summary>
        ///  Loads a tool onto a server. This will copy  dll onto a server.Refreshes the server tools afterwards.
        /// Only tools available on the local server can be deployed to another server
        ///  </summary>
        /// <param name="tool"></param>
        /// <param name="dllBytes"></param>
        public void LoadTool(IToolDescriptor tool, byte[] dllBytes)
        {
            VerifyArgument.IsNotNull("tool",tool);
            VerifyArgument.IsNotNull("dllBytes", dllBytes);
            _pluginProxy.LoadTool(tool, dllBytes);
            Server.ReloadTools();
         }

        /// <summary>
        /// Delete a tool from the connected server. this will delete the dll associated with the tool and refresh the server
        /// </summary>
        /// <param name="tool"></param>
        public void DeleteTool(IToolDescriptor tool)
        {
            VerifyArgument.IsNotNull("tool",tool);
            _pluginProxy.DeleteTool(tool);
            Server.ReloadTools();
        }

        /// <summary>
        /// filter the list of available tools
        /// </summary>
        /// <param name="search">the string that the name must contain</param>
        /// <returns></returns>
        public IList<IToolDescriptor> Filter(string search)
        {
            VerifyArgument.IsNotNull("search", search);
            // pure calls
            // ReSharper disable MaximumChainedReferences
            return Server.LoadTools().Where(a => a.Name.Contains(search)).ToList();
            // ReSharper restore MaximumChainedReferences
        }

        /// <summary>
        /// Is connected.
        /// </summary>
        /// <returns></returns>
        public bool IsEnabled()
        {
            return Server.IsConnected() && _localServer.IsConnected(); 
        }

        public event ServerDisconnected OnserverDisconnected;

        #endregion
    }
}
