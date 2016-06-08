using System;
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
        /// <returns>The version of the Server. Default version text of "less than 0.4.19.1" is returned
        /// if the server is older than that version.</returns>
        public string GetServerVersion()
        {
            var controller = CommunicationControllerFactory.CreateController("GetServerVersion");
            var version = controller.ExecuteCommand<string>(Connection, Guid.Empty);
            return string.IsNullOrEmpty(version) ? Resources.Languages.Core.LessThanServerVersion : version;
        }

        /// <summary>
        /// Gets the Warewolf Server version
        /// </summary>
        /// <returns>The version of the Server. Default version text of "less than 0.4.19.1" is returned
        /// if the server is older than that version.</returns>
        public string GetServerInformationalVersion()
        {
            var controller = CommunicationControllerFactory.CreateController("GetServerInformationalVersion");
            var version = controller.ExecuteCommand<string>(Connection, Guid.Empty);
            return string.IsNullOrEmpty(version) ? Resources.Languages.Core.LessThanServerVersion : version;
        }

        #endregion

        public string GetMinSupportedServerVersion()
        {
            var controller = CommunicationControllerFactory.CreateController("GetMinSupportedVersion");
            var version = controller.ExecuteCommand<string>(Connection, Guid.Empty);
            return version;
        }
    }
}
