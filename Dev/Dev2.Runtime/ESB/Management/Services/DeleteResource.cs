using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Dev2.DynamicServices;
using System.Text;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Delete a resource ;)
    /// </summary>
    public class DeleteResource : IEsbManagementEndpoint
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

            if(string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(type))
            {
                throw new InvalidDataContractException("ResourceName or Type is missing from the request");
            }

            if (resourceName == "*")
            {
                result.Append("Delete resources does not accept wildcards.");
            }
            else
            {
                switch (type)
                {
                    case "WorkflowService":
                    {
                            IEnumerable<DynamicService> services;

                            theHost.LockServices();

                            try
                            {
                                services = theHost.Services.Where(c => c.Name.Equals(resourceName, StringComparison.CurrentCultureIgnoreCase));
                            }
                            finally
                            {
                                theHost.UnlockServices();
                            }

                            IEnumerable<DynamicService> workflowServices = services.Where(c => c.Actions.Where(d => d.ActionType == enActionType.Workflow).Count() > 0);
                            List<DynamicService> match = workflowServices.ToList();

                            if (match.Count != 1)
                            {
                                if (match.Count == 0)
                                {
                                    result.Append("<Result>WorkflowService \"" + resourceName + "\" was not found.</Result>");
                                }
                                else
                                {
                                    result.Append("<Result>Multiple matches found for WorkflowService \"" + resourceName + "\".</Result>");
                                }
                            }
                            else
                            {
                                // ReSharper disable RedundantArgumentDefaultValue
                                result.Append("<Result>"+theHost.RemoveDynamicService(match[0], roles, true)+"</Result>");
                                // ReSharper restore RedundantArgumentDefaultValue
                            }

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

                            IEnumerable<DynamicService> svcs =
                            svc.Where(c => c.Actions.Where(d => d.ActionType != enActionType.Workflow).Count() > 0);
                            List<DynamicService> match = svcs.ToList();

                            if (match.Count != 1)
                            {
                                if (match.Count == 0)
                                {
                                    result.Append("<Result>Service \"" + resourceName + "\" was not found.<Result>");
                                }
                                else
                                {
                                    result.Append("<Result>Multiple matches found for Service \"" + resourceName + "\".</Result>");
                                }
                            }
                            else
                            {
                                // ReSharper disable RedundantArgumentDefaultValue
                                result.Append("<Result>" + theHost.RemoveDynamicService(match[0], roles, true) + "</Result>");
                                // ReSharper restore RedundantArgumentDefaultValue
                            }

                    }
                    break;


                    case "Source":
                    {
                        IEnumerable<Source> sources;
                        theHost.LockSources();

                        try
                        {
                            //Juries - Bug cant delete resources when more than one contains the name
                            //Shoot me if everything uses a fuzzy lookup.
                            sources = theHost.Sources.Where(c => c.Name.Equals(resourceName, StringComparison.CurrentCultureIgnoreCase));
                        }
                        finally
                        {
                            theHost.UnlockSources();
                        }

                        List<Source> match = sources.ToList();

                        if (match.Count != 1)
                        {
                            if (match.Count == 0)
                            {
                                result.Append("<Result>Source \"" + resourceName + "\" was not found.</Result>");
                            }
                            else
                            {
                                result.Append("<Result>Multiple matches found for Source \"" + resourceName + "\".</Result>");
                            }
                        }
                        else
                        {
                            // ReSharper disable RedundantArgumentDefaultValue
                            result.Append("<Result>" + theHost.RemoveSource(match[0], roles, true) + "</Result>");
                            // ReSharper restore RedundantArgumentDefaultValue
                        }
                    }
                    break;
                        
                }

            }

            return result.ToString();

        }

        public string HandlesType()
        {
            return "DeleteResourceService";
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService deleteResourceService = new DynamicService();
            deleteResourceService.Name = HandlesType();
            deleteResourceService.DataListSpecification = "<root><ResourceName/><ResourceType/><Roles/></root>";

            ServiceAction deleteResourceAction = new ServiceAction();
            deleteResourceAction.Name = HandlesType();
            deleteResourceAction.ActionType = enActionType.InvokeManagementDynamicService;
            deleteResourceAction.SourceMethod = HandlesType();

            deleteResourceService.Actions.Add(deleteResourceAction);

            return deleteResourceService;
        }

        #region Private Methods

        /// <summary>
        /// Processes the workflow services.
        /// </summary>
        /// <param name="theHost">The host.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="roles">The roles.</param>
        /// <returns></returns>
        private string ProcessWorkflowServices(IDynamicServicesHost theHost, string resourceName, string roles)
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
                services.Where(
                    c => c.Actions.Any(d => d.ActionType == enActionType.Workflow))
                        .Where(
                            c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
            List<DynamicService> match = workflowServices.ToList();

            if (match.Count != 1)
            {
                if (match.Count == 0)
                {
                    return "WorkflowService \"" + resourceName + "\" was not found.";
                }

                return "Multiple matches found for WorkflowService \"" + resourceName + "\".";

            }

            return theHost.RemoveDynamicService(match[0], roles, true);

        }

        /// <summary>
        /// Processes the services.
        /// </summary>
        /// <param name="theHost">The host.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="roles">The roles.</param>
        /// <returns></returns>
        private string ProcessServices(IDynamicServicesHost theHost, string resourceName, string roles)
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
            List<DynamicService> match = svcs.ToList();

            if (match.Count != 1)
            {
                if (match.Count == 0)
                {
                    return "Service \"" + resourceName + "\" was not found.";
                }

                return "Multiple matches found for Service \"" + resourceName + "\".";
            }

            return theHost.RemoveDynamicService(match[0], roles, true);

        }

        /// <summary>
        /// Processes the sources.
        /// </summary>
        /// <param name="theHost">The host.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="roles">The roles.</param>
        /// <returns></returns>
        private string ProcessSources(IDynamicServicesHost theHost, string resourceName, string roles)
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

            List<Source> match = sources.ToList();

            if (match.Count != 1)
            {
                if (match.Count == 0)
                {
                    return "Source \"" + resourceName + "\" was not found.";
                }

                return "Multiple matches found for Source \"" + resourceName + "\".";
            }

            return theHost.RemoveSource(match[0], roles, true);

        }

        #endregion


       
    }
}
