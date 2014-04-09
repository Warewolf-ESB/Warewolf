using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Security;
using Dev2.Scheduler;
using Dev2.Scheduler.Interfaces;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveScheduledResource : IEsbManagementEndpoint
    {
        private IServerSchedulerFactory _schedulerFactory;
        ISecurityWrapper _securityWrapper;

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

                if(tmp != null)
                {

                    var res = serializer.Deserialize<IScheduledResource>(tmp);

                    using(var model = SchedulerFactory.CreateModel(GlobalConstants.SchedulerFolderId, SecurityWrapper))
                    {
                        StringBuilder userName;
                        StringBuilder password;

                        values.TryGetValue("UserName", out userName);
                        values.TryGetValue("Password", out password);
                        if(userName == null || password == null)
                        {
                            result.Message.Append("No UserName or password provided");
                            result.HasError = true;
                        }
                        else
                        {
                            StringBuilder previousTask;
                            values.TryGetValue("PreviousResource", out previousTask);

                            model.Save(res, userName.ToString(), password.ToString());
                            if(previousTask != null && !String.IsNullOrEmpty(previousTask.ToString()) && (previousTask.ToString() != res.Name))
                            {
                                model.DeleteSchedule(new ScheduledResource(previousTask.ToString(), SchedulerStatus.Disabled, DateTime.MaxValue, null, null));
                            }
                        }
                    }
                }
                else
                {
                    result.Message.Append("No Resource Selected");
                    result.HasError = true;
                }
            }
            catch(Exception e)
            {
                result.Message.Append(string.Format("Error while saving: {0}", e.Message.Remove(e.Message.IndexOf('.'))));
                result.HasError = true;
            }
            return serializer.SerializeToBuilder(result);
        }

        public DynamicService CreateServiceEntry()
        {
            var addScheduledResourceService = new DynamicService
                {
                    Name = HandlesType(),
                    DataListSpecification =
                        "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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
            get { return _schedulerFactory ?? new ServerSchedulerFactory(); }
            set { _schedulerFactory = value; }
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
