#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
        public IList<IToolDescriptor> GetTools() => Server.LoadTools();

        /// <summary>
        /// Is connected.
        /// </summary>
        /// <returns></returns>
        public bool IsEnabled() => Server.IsConnected && _localServer.IsConnected;

#pragma warning disable 0067
        public event ServerDisconnected OnserverDisconnected;
#pragma warning restore 0067

        #endregion
    }
}
