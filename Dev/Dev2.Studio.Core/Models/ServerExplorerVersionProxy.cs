
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Models
{
    public class ServerExplorerVersionProxy : IVersionRepository
    {
        public ICommunicationControllerFactory CommunicationControllerFactory { get; private set; }
        private readonly IEnvironmentConnection _connection;

        public ServerExplorerVersionProxy(IEnvironmentConnection connection, ICommunicationControllerFactory communicationControllerFactory)
        {
            CommunicationControllerFactory = communicationControllerFactory;
            _connection = connection;
        }
        public ServerExplorerVersionProxy(IEnvironmentConnection connection)
        {
            CommunicationControllerFactory = new CommunicationControllerFactory();
            _connection = connection;
        }
        public IEnvironmentConnection Connection
        {
            get { return _connection; }
        }

        #region Implementation of IVersionRepository

        public IList<IExplorerItem> GetVersions(Guid resourceId)
        {
            var workSpaceId = Guid.NewGuid();
            var controller = CommunicationControllerFactory.CreateController("GetVersions");
            controller.AddPayloadArgument("resourceId", resourceId.ToString());
            return controller.ExecuteCommand<IList<IExplorerItem>>(Connection, workSpaceId);
        }

        public StringBuilder GetVersion(IVersionInfo versionInfo)
        {
            var workSpaceId = Guid.NewGuid();
            var controller = CommunicationControllerFactory.CreateController("GetVersion");
            var serializer = new Dev2JsonSerializer();
            controller.AddPayloadArgument("versionInfo", serializer.SerializeToBuilder(versionInfo).ToString());
            var executeMessage = controller.ExecuteCommand<ExecuteMessage>(Connection, workSpaceId);

            if(executeMessage == null || executeMessage.HasError)
            {
                return null;
            }

            return executeMessage.Message;
        }

        public IExplorerItem GetLatestVersionNumber(Guid resourceId)
        {
            var workSpaceId = Guid.NewGuid();
            var controller = CommunicationControllerFactory.CreateController("GetLatestVersionNumber");
            controller.AddPayloadArgument("resourceId", resourceId.ToString());
            return controller.ExecuteCommand<IExplorerItem>(Connection, workSpaceId);
        }

        public IRollbackResult RollbackTo(Guid resourceId, string versionNumber)
        {
            var workSpaceId = Guid.NewGuid();
            var controller = CommunicationControllerFactory.CreateController("RollbackTo");
            controller.AddPayloadArgument("resourceId", resourceId.ToString());
            controller.AddPayloadArgument("versionNumber", versionNumber);

            var result = controller.ExecuteCommand<ExecuteMessage>(Connection, workSpaceId);

            if(result == null || result.HasError)
            {
                return null;
            }

            var serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<IRollbackResult>(result.Message);
        }

        public IList<IExplorerItem> DeleteVersion(Guid resourceId, string versionNumber)
        {
            var workSpaceId = Guid.NewGuid();
            var controller = CommunicationControllerFactory.CreateController("DeleteVersion");
            controller.AddPayloadArgument("resourceId", resourceId.ToString());
            controller.AddPayloadArgument("versionNumber", versionNumber);

            var result = controller.ExecuteCommand<ExecuteMessage>(Connection, workSpaceId);

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
