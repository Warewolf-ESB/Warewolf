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
    public class ResetPerformanceCounters : IEsbManagementEndpoint
    {
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
            return "ResetPerformanceCounters";
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
                Manager.ResetCounters();
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


        private IPerformanceCounterRepository Manager => CustomContainer.Get<IPerformanceCounterRepository>();

        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(),  DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }
    }
}