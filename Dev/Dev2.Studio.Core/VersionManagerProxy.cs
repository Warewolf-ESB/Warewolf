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

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Interfaces;
using Warewolf.Data;
using Warewolf.Resource.Errors;

namespace Dev2.Studio.Core
{
    public class VersionManagerProxy : Dev2.Common.Interfaces.ServerProxyLayer.IVersionManager {
        readonly IEnvironmentConnection _connection;

        public VersionManagerProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection)
        {
            CommunicationControllerFactory = communicationControllerFactory;
            _connection = connection;
        }

        public ICommunicationControllerFactory CommunicationControllerFactory { get; set; }

        #region Implementation of IVersionManager

        void ShowServerDisconnectedPopup()
        {
            var controller = CustomContainer.Get<IPopupController>();
            controller?.Show(string.Format(ErrorResource.ServerDisconnected, _connection.DisplayName.Replace("(Connected)", "")) + Environment.NewLine +
                             ErrorResource.ServerReconnectForActions, ErrorResource.ServerDisconnectedHeader, MessageBoxButton.OK,
                MessageBoxImage.Error, "", false, true, false, false, false, false);
        }

        /// <summary>
        /// Get a list of versions of a resource
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <returns>the resource versions. N configured versions are stored on a server</returns>
        public IList<IExplorerItem> GetVersions(Guid resourceId)
        {
            if (!_connection.IsConnected)
            {
                ShowServerDisconnectedPopup();
                return new List<IExplorerItem>();
            }

            var workSpaceId = Guid.NewGuid();
            var controller = CommunicationControllerFactory.CreateController("GetVersions");
            controller.AddPayloadArgument("resourceId", resourceId.ToString());
            var items = controller.ExecuteCommand<IList<IExplorerItem>>(_connection, workSpaceId);
            return items;
        }


        public StringBuilder GetVersion(IVersionInfo versionInfo, Guid resourceId)
        {
            var workSpaceId = Guid.NewGuid();
            var controller = CommunicationControllerFactory.CreateController("GetVersion");
            var serializer = new Dev2JsonSerializer();
            controller.AddPayloadArgument("versionInfo", serializer.SerializeToBuilder(versionInfo).ToString());
            controller.AddPayloadArgument("resourceId", resourceId.ToString());
            var executeMessage = controller.ExecuteCommand<ExecuteMessage>(_connection, workSpaceId);

            if (executeMessage == null || executeMessage.HasError)
            {
                return null;
            }

            return executeMessage.Message;
        }


        /// <summary>
        /// rollback to a specific version 
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <param name="versionNumber">the version to rollback to</param>
        public IRollbackResult RollbackTo(Guid resourceId, string versionNumber)
        {
            var workSpaceId = resourceId;
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
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public IList<IExplorerItem> DeleteVersion(Guid resourceId, string versionNumber, string resourcePath)
        {
            var workSpaceId = Guid.NewGuid();
            var controller = CommunicationControllerFactory.CreateController("DeleteVersion");
            controller.AddPayloadArgument("resourceId", resourceId.ToString());
            controller.AddPayloadArgument("versionNumber", versionNumber);
            controller.AddPayloadArgument("resourcePath", resourcePath);
            var result = controller.ExecuteCommand<ExecuteMessage>(_connection, workSpaceId);

            if (result == null || result.HasError)
            {
                return null;
            }

            var serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<IList<IExplorerItem>>(result.Message);
        }

        #endregion
    }
}