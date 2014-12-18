using System.Globalization;
using Dev2.Common;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;

namespace Warewolf.Studio.ServerProxyLayer
{
    public class AdminManagerProxy : ProxyBase,IAdminManager
    {
        #region Implementation of IAdminManager


        public AdminManagerProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection) : base(communicationControllerFactory, connection) { }

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
    }
}
