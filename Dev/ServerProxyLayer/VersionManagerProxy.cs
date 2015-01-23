using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Communication;

namespace Warewolf.Studio.ServerProxyLayer
{
    public class VersionManagerProxy : Dev2.Common.Interfaces.ServerProxyLayer.IVersionManager {
        IEnvironmentConnection _connection;

        public VersionManagerProxy(IEnvironmentConnection connection, ICommunicationControllerFactory communicationControllerFactory)
        {
            CommunicationControllerFactory = communicationControllerFactory;
            _connection = connection;
        }

        public ICommunicationControllerFactory CommunicationControllerFactory { get; set; }

        #region Implementation of IVersionManager

        /// <summary>
        /// Get a list of versions of a resource
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <returns>the resource versions. N configured versions are stored on a server</returns>
        public IList<IExplorerItem> GetVersions(Guid resourceId)
        {
            var workSpaceId = Guid.NewGuid();
            var controller = CommunicationControllerFactory.CreateController("GetVersions");
            controller.AddPayloadArgument("resourceId", resourceId.ToString());
            return controller.ExecuteCommand<IList<IExplorerItem>>(_connection, workSpaceId);
        }

        /// <summary>
        /// Get the heavy weight resource
        /// </summary>
        /// <param name="version">the version to fetch</param>
        /// <returns>a resource that can be displayed on the design surface</returns>
        public StringBuilder GetVersion(IVersionInfo version)
        {
            return null;
        }

        /// <summary>
        /// rollback to a specific version 
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <param name="versionNumber">the version to rollback to</param>
        public IRollbackResult RollbackTo(Guid resourceId, string versionNumber)
        {
            var workSpaceId = Guid.NewGuid();
            var controller = CommunicationControllerFactory.CreateController("RollbackTo");
            controller.AddPayloadArgument("resourceId", resourceId.ToString());
            controller.AddPayloadArgument("versionNumber", versionNumber);

            var result = controller.ExecuteCommand<ExecuteMessage>(_connection, workSpaceId);

            if (result == null || result.HasError)
            {
                return null;
            }

            var serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<IRollbackResult>(result.Message);
        }

        /// <summary>
        /// Delete a version o a resource
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <param name="versionNumber">the version to delete</param>
        /// <returns></returns>
        public IList<IExplorerItem> DeleteVersion(Guid resourceId, string versionNumber)
        {
            return null;
        }

        #endregion
    }
}