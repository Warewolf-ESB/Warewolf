/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Scheduler;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveScheduledResource : IEsbManagementEndpoint
    {
        private IServerSchedulerFactory _schedulerFactory;
        ISecurityWrapper _securityWrapper;
        private IResourceCatalog _catalog;
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        public string HandlesType()
        {
            return "SaveScheduledResourceService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var result = new ExecuteMessage { HasError = false };
            StringBuilder tmp;
            values.TryGetValue("Resource", out tmp);
            var serializer = new Dev2JsonSerializer();
            try
            {
                if (tmp != null)
                {

                    var res = serializer.Deserialize<IScheduledResource>(tmp);
                    Dev2Logger.Info("Save Scheduled Resource. Scheduled Resource:" +res);
                    using(var model = SchedulerFactory.CreateModel(GlobalConstants.SchedulerFolderId, SecurityWrapper))
                    {
                        StringBuilder userName;
                        StringBuilder password;

                        values.TryGetValue("UserName", out userName);
                        values.TryGetValue("Password", out password);
                        if(userName == null || password == null)
                        {
                            result.Message.Append(ErrorResource.NoUserNameAndPassword);
                            result.HasError = true;
                        }
                        else
                        {
                            StringBuilder previousTask;
                            values.TryGetValue("PreviousResource", out previousTask);

                            model.Save(res, userName.ToString(), password.ToString());
                            if(!string.IsNullOrEmpty(previousTask?.ToString()) && previousTask.ToString() != res.Name)
                            {
                                model.DeleteSchedule(new ScheduledResource(previousTask.ToString(), SchedulerStatus.Disabled, DateTime.MaxValue, null, null,Guid.NewGuid().ToString()));
                            }
                        }
                    }
                }
                else
                {
                    result.Message.Append(ErrorResource.NoResourceSelected);
                    result.HasError = true;
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Error(e);
                result.Message.Append($"Error while saving: {e.Message.Remove(e.Message.IndexOf('.'))}");
                result.HasError = true;
            }
            return serializer.SerializeToBuilder(result);
        }

        public DynamicService CreateServiceEntry()
        {
            var addScheduledResourceService = new DynamicService
                {
                    Name = HandlesType(),
                    DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
                };

            var addScheduledResourceAction = new ServiceAction
                {
                    Name = HandlesType(),
                    ActionType = enActionType.InvokeManagementDynamicService,
                    SourceMethod = HandlesType()
                };


            addScheduledResourceService.Actions.Add(addScheduledResourceAction);

            return addScheduledResourceService;
        }
        public IServerSchedulerFactory SchedulerFactory
        {
            get { return _schedulerFactory ?? new ServerSchedulerFactory(a => ResourceCatalogue.GetResourcePath(GlobalConstants.ServerWorkspaceID, a.ResourceId)); }
            set { _schedulerFactory = value; }
        }

        public IResourceCatalog ResourceCatalogue
        {
            get { return _catalog ?? ResourceCatalog.Instance; }
            set { _catalog = value; }
        }

        public ISecurityWrapper SecurityWrapper
        {
            get
            {
                return _securityWrapper ?? new SecurityWrapper(ServerAuthorizationService.Instance);
            }
            set
            {
                _securityWrapper = value;
            }
        }
    }
}
