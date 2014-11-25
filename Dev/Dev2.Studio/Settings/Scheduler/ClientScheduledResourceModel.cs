
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
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Settings.Scheduler
{
    public class ClientScheduledResourceModel : IScheduledResourceModel
    {
        private readonly IEnvironmentModel _model;
        ObservableCollection<IScheduledResource> _scheduledResources;

        public ClientScheduledResourceModel([Annotations.NotNull] IEnvironmentModel model)
        {
            if(model == null)
            {
                throw new ArgumentNullException("model");
            }
            _model = model;
        }

        public ObservableCollection<IScheduledResource> ScheduledResources
        {
            get
            {
                return _scheduledResources ?? (_scheduledResources = GetScheduledResources());
            }
            set
            {
                _scheduledResources = value;
            }
        }

        public ObservableCollection<IScheduledResource> GetScheduledResources()
        {
            var controller = new CommunicationController { ServiceName = "GetScheduledResources" };
            return controller.ExecuteCommand<ObservableCollection<IScheduledResource>>(_model.Connection, _model.Connection.WorkspaceID);
        }

        public void DeleteSchedule(IScheduledResource resource)
        {
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var builder = jsonSerializer.SerializeToBuilder(resource);
            var controller = new CommunicationController { ServiceName = "DeleteScheduledResourceService" };
            controller.AddPayloadArgument("Resource", builder);
            controller.ExecuteCommand<string>(_model.Connection, _model.Connection.WorkspaceID);
        }

        public bool Save(IScheduledResource resource, out string errorMessage)
        {
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var builder = jsonSerializer.SerializeToBuilder(resource);
            var controller = new CommunicationController { ServiceName = "SaveScheduledResourceService" };
            controller.AddPayloadArgument("Resource", builder);
            controller.AddPayloadArgument("PreviousResource", resource.OldName);
            controller.AddPayloadArgument("UserName", resource.UserName);
            controller.AddPayloadArgument("Password", resource.Password);
            var executeCommand = controller.ExecuteCommand<ExecuteMessage>(_model.Connection, _model.Connection.WorkspaceID);
            errorMessage = "";
            if(executeCommand != null)
            {
                resource.IsDirty = executeCommand.HasError;
                errorMessage = executeCommand.Message.ToString();
                return !executeCommand.HasError;
            }
            return true;
        }

        public void Save(IScheduledResource resource, string userName, string password)
        {
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var builder = jsonSerializer.SerializeToBuilder(resource);
            var controller = new CommunicationController { ServiceName = "SaveScheduledResourceService" };
            controller.AddPayloadArgument("Resource", builder);
            controller.AddPayloadArgument("UserName", userName);
            controller.AddPayloadArgument("Password", password);
            controller.ExecuteCommand<string>(_model.Connection, _model.Connection.WorkspaceID);
            resource.IsDirty = false;
        }

        public IList<IResourceHistory> CreateHistory(IScheduledResource resource)
        {
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var builder = jsonSerializer.SerializeToBuilder(resource);
            var controller = new CommunicationController { ServiceName = "GetScheduledResourceHistoryService" };
            controller.AddPayloadArgument("Resource", builder);
            return controller.ExecuteCommand<IList<IResourceHistory>>(_model.Connection, _model.Connection.WorkspaceID);
        }

        public void Dispose()
        {

        }
    }
}
