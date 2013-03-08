using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using enActionType = Dev2.DynamicServices.enActionType;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// List resource for binding method
    /// </summary>
    public class FindResourceForBinding : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {

            IDynamicServicesHost theHost = theWorkspace.Host;

            string resourceName;
            string type;
            string roles;

            values.TryGetValue("ResourceName", out resourceName);
            values.TryGetValue("Type", out type);
            values.TryGetValue("Roles", out roles);

            if(string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(type) || string.IsNullOrEmpty(roles))
            {
                throw new InvalidDataContractException("Missing arguements");
            }

            if (resourceName == "*")
            {
                resourceName = string.Empty;
            }

            StringBuilder result = new StringBuilder();

            switch (type)
            {
                case "WorkflowService":
                    {
                        IEnumerable<DynamicService> services;
                        theHost.LockServices();

                        try
                        {
                            services = theHost.Services.Where(c => c.Name.Contains(resourceName));
                        }
                        finally
                        {
                            theHost.UnlockServices();
                        }

                        IEnumerable<DynamicService> workflowServices =
                            services.Where(c => c.Actions.Any(d => d.ActionType == enActionType.Workflow));

                        workflowServices.ToList()
                                        .ForEach(
                                            c =>
                                            result.Append(c.ResourceDefinition));
                        break;
                    }
                case "Service":
                    {
                        IEnumerable<DynamicService> svc;
                        theHost.LockServices();

                        try
                        {
                            svc = theHost.Services.Where(c => c.Name.Contains(resourceName));
                        }
                        finally
                        {
                            theHost.UnlockServices();
                        }


                        IEnumerable<DynamicService> svcs = svc.Where(c => c.Actions.Any(d => d.ActionType != enActionType.Workflow));

                        svcs.ToList()
                            .ForEach(
                                c =>
                                result.Append(c.ResourceDefinition));
                        break;
                    }
            }


            // now extract the attributed junk for the server to properly use
            var attributes = new[] { "Name" };
            var childTags = new[] { "Category" };
            string returnValue = DataListUtil.ExtractAttributeFromTagAndMakeRecordset(result.ToString(), "Service",
                                                                                      attributes, childTags);
            returnValue = returnValue.Replace("<Service>", "<Dev2Service>").Replace("</Service>", "</Dev2Service>");

            return returnValue;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findServicesBinder = new DynamicService();
            findServicesBinder.Name = HandlesType();
            findServicesBinder.DataListSpecification = "<root><Type/><Roles/><ResourceName/></root>";

            ServiceAction findServiceActionBinder = new ServiceAction();
            findServiceActionBinder.Name = HandlesType();
            findServiceActionBinder.ActionType = enActionType.InvokeManagementDynamicService;
            findServiceActionBinder.SourceMethod = HandlesType();

            findServicesBinder.Actions.Add(findServiceActionBinder);

            return findServicesBinder;
        }

        public string HandlesType()
        {
            return "ListResourcesForBindingService";
        }
    }
}
