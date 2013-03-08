using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Gets a resource definition
    /// </summary>
    public class GetResource : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            IDynamicServicesHost theHost = theWorkspace.Host;
            StringBuilder result = new StringBuilder();

            string resourceType;
            string resourceName;
            string roles;

            values.TryGetValue("ResourceType", out resourceType);
            values.TryGetValue("ResourceName", out resourceName);
            values.TryGetValue("Roles", out roles);

            if(string.IsNullOrEmpty(resourceType) || string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(roles))
            {
                throw new InvalidDataContractException("ResourceType or ResourceName or Roles not provided");
            }

            switch(resourceType)
            {
                case "WorkflowService":
                {
                    IEnumerable<DynamicService> services;
                    theHost.LockServices();

                    try
                    {
                        services = theHost.Services.Where(c => c.Name == resourceName);
                    }
                    finally
                    {
                        theHost.UnlockServices();
                    }

                    IEnumerable<DynamicService> workflowServices = services.Where(c => c.Actions.Any(d => d.ActionType == enActionType.Workflow));

                    workflowServices.ToList()
                                    .ForEach(
                                        c =>
                                        result.Append(c.ResourceDefinition));
                }
                break;

                case "Service":
                {
                    IEnumerable<DynamicService> svc;
                    theHost.LockServices();

                    try
                    {
                        svc = theHost.Services.Where(c => c.Name == resourceName);
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
                }
                break;


                case "Source":
                {
                    IEnumerable<Source> sources;
                    theHost.LockSources();

                    try
                    {
                        sources = theHost.Sources.Where(c => c.Name == resourceName);
                    }
                    finally
                    {
                        theHost.UnlockSources();
                    }

                    sources.ToList()
                           .ForEach(
                               c =>
                               result.Append(c.ResourceDefinition));
                }
                break;

                case "ReservedService":
                {
                    IEnumerable<DynamicService> reserved;
                    theHost.LockReservedServices();

                    try
                    {
                        reserved = theHost.ReservedServices.Where(c => c.Name == resourceName);
                    }
                    finally
                    {
                        theHost.UnlockReservedServices();
                    }

                    reserved.ToList()
                            .ForEach(
                                c =>
                                result.Append("<ReservedName>" + c.Name + "</ReservedName>"));
                }
                break;
            }

            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService getResourceServicesBinder = new DynamicService();
            getResourceServicesBinder.Name = HandlesType();
            getResourceServicesBinder.DataListSpecification = "<root><ResourceType/><Roles/><ResourceName/></root>";

            ServiceAction getResourceServiceActionBinder = new ServiceAction();
            getResourceServiceActionBinder.Name = HandlesType();
            getResourceServiceActionBinder.SourceMethod = HandlesType();
            getResourceServiceActionBinder.ActionType = enActionType.InvokeManagementDynamicService;

            getResourceServicesBinder.Actions.Add(getResourceServiceActionBinder);

            return getResourceServicesBinder;
        }

        public string HandlesType()
        {
            return "GetResourceService";
        }
    }
}
