using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xaml;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find dependencies for a service
    /// </summary>
    public class FindDependencies : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string resourceName;
            values.TryGetValue("ResourceName", out resourceName);

            if(string.IsNullOrEmpty(resourceName))
            {
                throw new InvalidDataContractException("ResourceName is empty or null");
            }

            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            var result = string.Format("<graph title=\"Dependency Graph Of {0}\">", resourceName) + FindDependenciesRecursive(resourceName, theWorkspace.ID) + "</graph>";
            return "<root>" + result + "</root>";
        }

        public string HandlesType()
        {
            return "FindDependencyService";
        }

        public DynamicService CreateServiceEntry()
        {
            var ds = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><ResourceName/></DataList>"
            };

            var sa = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            ds.Actions.Add(sa);

            return ds;

        }

        #region Private Methods

        // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
        private string FindDependenciesRecursive(string resourceName, Guid workspaceID)
        {
            var visitedServices = new List<string>();
            var sb = new StringBuilder();
            var reverseDependencyList = new List<string>();
            var brokenDependencies = new HashSet<string>();

            sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"false\">", resourceName));

            var allResources = ResourceCatalog.Instance.GetDynamicObjects(workspaceID);
            var allServices = allResources.OfType<DynamicService>().ToList();
            var allSources = allResources.OfType<Source>().ToList();

            var isService = false;
            var services = allServices.Where(s => s.Name == resourceName).ToList();

            if(services.Count > 0)
            {
                foreach(var item in services)
                {
                    item.Actions.ForEach(c => c.ServiceActionInputs.ForEach(
                        d => sb.Append(string.Format("<input name=\"{0}\" />", d.Name))));
                }
                isService = true;
            }

            //This is a service so we can find all reverse dependencies
            // i.e these are items that need this service
            if(isService)
            {
                // ReSharper disable ImplicitlyCapturedClosure
                var reverseDependencies = allResources.OfType<DynamicService>().Where(c => c.Actions.Count(d => d.ServiceName == resourceName) > 0).ToList();
                // ReSharper restore ImplicitlyCapturedClosure

                reverseDependencies.ForEach(c => reverseDependencyList.Add(c.Name));

                reverseDependencies.ForEach(c =>
                {
                    c.Actions.ForEach(e =>
                    {
                        var activity = e.WorkflowActivity as DynamicActivity;
                        if(activity != null)
                        {
                            Activity flowChart;
                            //flowChart = activity.Implementation.Invoke();

                            try
                            {
                                flowChart = activity.Implementation.Invoke();
                            }
                            catch(XamlObjectWriterException)
                            {
                                flowChart = null;

                                if(!reverseDependencyList.Contains(activity.Name))
                                {
                                    reverseDependencyList.Add(activity.Name);
                                    brokenDependencies.Add(activity.Name);
                                }

                                // This exception occurs due to invalid workflows that still reference the old ForEach activity. (AdditionalData)
                            }


                            var workflow = flowChart as Flowchart;
                            if(workflow != null)
                            {
                                //Activities that are used by this resource
                                foreach(dynamic fn in workflow.Nodes)
                                {
                                    if((fn is FlowStep) && fn.Action.GetType().Name == "DsfActivity")
                                    {
                                        if(fn.Action.ServiceName == resourceName)
                                        {
                                            reverseDependencyList.Add(c.Name);
                                        }
                                    }
                                }
                            }
                        }
                    });
                });
            }

            var sources = allSources.Where(r => r.Name == resourceName).ToList();

            //Source Resource Name was passed in 
            //find all services that 
            var serviceUsingSource = new List<string>();
            if(sources.Count > 0)
            {
                sources.ForEach(e => allServices.ForEach(c => c.Actions.ForEach(d =>
                {
                    if(d.SourceName == e.Name)
                    {
                        serviceUsingSource.Add(c.Name);
                    }
                })));

                serviceUsingSource.ForEach(c => sb.Append(string.Format("<dependency id=\"{0}\" />", c)));
            }

            var workflowServices = services.Where(c => c.Actions.Count(d => d.ActionType == enActionType.Workflow) > 0).ToList();


            var svcNames = new List<string>();
            workflowServices.ForEach(resource =>
            {
                var workflowMatch = resource.Actions.Where(d => d.ActionType == enActionType.Workflow).ToList();
                if(workflowMatch.Count > 0)
                {
                    workflowMatch.ForEach(e =>
                    {
                        var activity = e.WorkflowActivity as DynamicActivity;
                        if(activity != null)
                        {
                            Activity flowChart;


                            try
                            {
                                flowChart = activity.Implementation.Invoke();
                            }
                            catch(XamlObjectWriterException)
                            {
                                flowChart = null;
                                // This exception occurs due to invalid workflows that still reference the old ForEach activity. (AdditionalData)
                            }


                            var workflow = flowChart as Flowchart;
                            if(workflow != null)
                            {
                                foreach(dynamic fn in workflow.Nodes)
                                {
                                    if((fn is FlowStep) && fn.Action.GetType().Name == "DsfActivity")
                                    {
                                        if(!svcNames.Contains(fn.Action.ServiceName))
                                        {
                                            svcNames.Add(fn.Action.ServiceName);
                                        }
                                        sb.Append(string.Format("<dependency id=\"{0}\" />", fn.Action.ServiceName));
                                    }
                                }
                            }
                        }
                    });
                }
            });

            var dsfServices = allServices.Where(c => c.Actions.Any(d => d.SourceName == resourceName)).ToList();
            dsfServices.ForEach(c => { if(!svcNames.Contains(c.Name)) svcNames.Add(c.Name); });


            sb.Append("</node>");

            if(serviceUsingSource.Count > 0)
            {
                serviceUsingSource.ForEach(
                    c => sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"false\"/>", c)));
            }

            if(reverseDependencyList.Count > 0)
            {
                reverseDependencyList.ForEach(c =>
                {
                    sb.Append(
                        string.Format(
                            "<node id=\"{0}\" x=\"\" y=\"\" broken=\"" +
                            (brokenDependencies.Contains(c) ? "true" : "false") + "\">", c));
                    sb.Append(string.Format("<dependency id=\"{0}\" />", resourceName));
                    sb.Append("</node>");
                });
            }

            visitedServices.Add(resourceName);


            var circularref = visitedServices.Intersect(svcNames).ToList();
            if(circularref.Count > 0)
            {
                var circ = new StringBuilder();
                circularref.ForEach(s => circ.Append(string.Format("<dep>{0}</dep>", s)));

                //Jurie.Smit removed - circular references allowed, should be graphed
                //throw new Exception(string.Format("Circular Reference Detected between {0} and {1}!", resourceName, circ));
            }

            svcNames.Except(circularref).ToList().ForEach(c => sb.Append(FindDependenciesRecursive(c, workspaceID)));

            return sb.ToString();
        }

        #endregion



    }
}
