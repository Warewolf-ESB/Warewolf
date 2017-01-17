using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;

namespace Warewolf.Studio.ServerProxyLayer
{
    public class AdminManagerProxy : ProxyBase,IAdminManager
    {
        #region Implementation of IAdminManager


        public AdminManagerProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection) : base(communicationControllerFactory, connection) { }

        /// <summary>
        /// Gets the Warewolf Server version
        /// </summary>
        /// <returns>The version of the Server. Default version text of "Not Available." is returned
        /// if the server is older than that version.</returns>
        public string GetServerVersion()
        {
            var controller = CommunicationControllerFactory.CreateController("GetServerVersion");
            var version = controller.ExecuteCommand<string>(Connection, Guid.Empty);
            return string.IsNullOrEmpty(version) ? Resources.Languages.Core.ServerVersionUnavailable : version;
        }

        #endregion

        public string GetMinSupportedServerVersion()
        {
            var controller = CommunicationControllerFactory.CreateController("GetMinSupportedVersion");
            var version = controller.ExecuteCommand<string>(Connection, Guid.Empty);
            return version;
        }

        public Dictionary<string,string> GetServerInformation()
        {
            var controller = CommunicationControllerFactory.CreateController("GetServerInformation");
            var information = controller.ExecuteCommand<Dictionary<string, string>>(Connection, Guid.Empty);
            return information;
        }
    }
}
