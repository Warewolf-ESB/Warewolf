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
using Dev2.Communication;
using Dev2.Studio.Interfaces;
using Warewolf.Esb;

namespace Warewolf.EsbClient
{
    public interface IClusterLeaderProxy
    {
        /// <summary>
        ///  Connect to a previously registered Leader to begin following their change notifications
        /// </summary>
        /// <param name="server"></param>
        void Follow(IServer server, string secureKey);
    }

    public class ClusterLeaderProxy : IClusterLeaderProxy
    {
        readonly IEnvironmentConnection _environmentConnection;

        public ClusterLeaderProxy(IEnvironmentConnection environmentConnection)
        {
            _environmentConnection = environmentConnection;
        }

        public void Follow(IServer server, string secureKey)
        {
            /*
                        var communicationController = new CommunicationController
                        {
                            ServiceName = nameof(Service.GetResourceById)
                        };
                        communicationController.AddPayloadArgument(Service.GetResourceById.WorkspaceId, secureKey);
                        var connectionSuccessResult = communicationController.ExecuteCommand<ClusterJoinResponse>(_environmentConnection, GlobalConstants.ServerWorkspaceID);
                        // if change notification request is accepted then keep the signalr connection open while waiting for change notifications.

                        */

        }
    }

    public class ChangeNotification
    {
    }
    public class EventRequest<T> : ICatalogSubscribeRequest
    {
        private readonly Guid _workspaceId;

        public EventRequest(Guid workspaceId)
        {
            _workspaceId = workspaceId;
        }

        public IEsbRequest Build()
        {
            return new EsbExecuteRequest();
        }
    }
}
