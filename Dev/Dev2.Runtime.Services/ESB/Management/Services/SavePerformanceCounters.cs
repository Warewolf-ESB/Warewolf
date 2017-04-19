using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SavePerformanceCounters : IEsbManagementEndpoint
    {
        private IPerformanceCounterRepository _manager;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Administrator;
        }

        public string HandlesType()
        {
            return "SavePerformanceCounters";
        }
        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Manager.Save(serializer.Deserialize<IPerformanceCounterTo>(values["PerformanceCounterTo"]));
                msg.HasError = false;
                msg.Message = new StringBuilder();
            }
            catch(Exception err)
            {

                msg.HasError = true;
                msg.Message = new StringBuilder( err.Message);
            }

            return serializer.SerializeToBuilder(msg);
        }


        public IPerformanceCounterRepository Manager
        {
            private get { return _manager ?? CustomContainer.Get<IPerformanceCounterRepository>(); }
            set { _manager = value; }

        }
        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(),  DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><PerformanceCounterTo ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }
    }
}
