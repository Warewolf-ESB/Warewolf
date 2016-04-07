using System;
using System.Globalization;
using Dev2.Common;
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

        /// <summary>
        /// Get the execution queue depth. ie the number of items waiting for execution
        /// </summary>
        /// <returns></returns>
        public int GetCurrentQueueDepth()
        {
            var controller = CommunicationControllerFactory.CreateController("GetCurrentQueueDepthService");
            return controller.ExecuteCommand<int>(Connection, GlobalConstants.ServerWorkspaceID);
        }

        /// <summary>
        /// Get the maximum queue depth before warewolf will reject new requests
        /// </summary>
        /// <returns></returns>
        public int GetMaxQueueDepth()
        {
            var controller = CommunicationControllerFactory.CreateController("GetMaxQueueDepthService");
            return controller.ExecuteCommand<int>(Connection, GlobalConstants.ServerWorkspaceID);
        }

        /// <summary>
        /// Get the maximum number of concurrent executions available on a warewolf server
        /// </summary>
        /// <returns></returns>
        public int GetMaxThreadCount()
        {
            var controller = CommunicationControllerFactory.CreateController("GetMaxThreadCountService");
            return controller.ExecuteCommand<int>(Connection, GlobalConstants.ServerWorkspaceID);
        }

        /// <summary>
        /// Set the maximum queue depth before warewolf rejects messages
        /// </summary>
        public void SetMaxQueueDepth(int depth)
        {
            var controller = CommunicationControllerFactory.CreateController("SetMaxQueueDepthService");
            controller.AddPayloadArgument("maxDepth", depth.ToString(CultureInfo.InvariantCulture));
            controller.ExecuteCommand<string>(Connection, GlobalConstants.ServerWorkspaceID);
        }

        /// <summary>
        /// Set the maximum number of concurrent execution available 
        /// </summary>
        public void SetMaxThreadCount(int count)
        {
            var controller = CommunicationControllerFactory.CreateController("SetMaxThreadCountService");
            controller.AddPayloadArgument("maxThreadCount", count.ToString(CultureInfo.InvariantCulture));
            controller.ExecuteCommand<string>(Connection, GlobalConstants.ServerWorkspaceID);
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
