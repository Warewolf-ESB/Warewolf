using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dev2.DynamicServices;
using Unlimited.Framework;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find resources in the service catalog
    /// </summary>
    public class FindResource : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            StringBuilder result = new StringBuilder();
            IDynamicServicesHost theHost = theWorkspace.Host;

            string resourceName;
            string type;
            string roles;

            values.TryGetValue("ResourceName", out resourceName);
            values.TryGetValue("ResourceType", out type);
            values.TryGetValue("Roles", out roles);

            if(string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(type) || string.IsNullOrEmpty(roles))
            {
                throw new InvalidDataContractException("Missing ResourceName or Type or Roles");
            }

            if(resourceName == "*")
            {
                resourceName = string.Empty;
            }

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
                }
                break;

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
                }
                break;


                case "Source":
                {
                    IEnumerable<Source> sources;
                    theHost.LockSources();

                    try
                    {
                        sources = theHost.Sources.Where(c => c.Name.Contains(resourceName));
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
                        reserved = theHost.ReservedServices.Where(c => c.Name.Contains(resourceName));
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

            return "<Payload>"+result.ToString()+"</Payload>";
        }

        public  DynamicService CreateServiceEntry()
        {
            DynamicService findServices = new DynamicService();
            findServices.Name = HandlesType();
            findServices.DataListSpecification = "<root><ResourceType/><Roles/><ResourceName/></root>";

            ServiceAction findServiceAction = new ServiceAction();
            findServiceAction.Name = HandlesType();
            findServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findServiceAction.SourceMethod = HandlesType();

            findServices.Actions.Add(findServiceAction);

            return findServices;
        }

        public string HandlesType()
        {
            return "FindResourceService";
        }
    }
}
