using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xaml;
using Dev2.DynamicServices;
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
            IDynamicServicesHost theHost = theWorkspace.Host;

            string resourceName;
            values.TryGetValue("ResourceName", out resourceName);

            if (string.IsNullOrEmpty(resourceName))
            {
                throw new InvalidDataContractException("ResourceName is empty or null");
            }

            string result = string.Format("<graph title=\"Dependency Graph Of {0}\">", resourceName) + FindDependenciesRecursive(resourceName, theHost) + "</graph>";


            return "<root>" + result + "</root>";
        }

        public string HandlesType()
        {
            return "FindDependencyService";
        }

        public DynamicService CreateServiceEntry()
        {
            //throw new System.NotImplementedException();
            DynamicService ds = new DynamicService() { Name = HandlesType(), DataListSpecification = "<DataList><ResourceName/></DataList>" };

            ServiceAction sa = new ServiceAction();
            sa.Name = HandlesType();
            sa.ActionType = enActionType.InvokeManagementDynamicService;
            sa.SourceMethod = HandlesType();

            ds.Actions.Add(sa);

            return ds;

        }

        #region Private Methods
        private string FindDependenciesRecursive(string resourceName, IDynamicServicesHost Host)
        {
            List<string> visitedServices = new List<string>();
            var sb = new StringBuilder();
            var reverseDependencyList = new List<string>();
            var brokenDependencies = new HashSet<string>();

            sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"false\">", resourceName));


            bool isService = false;
            IEnumerable<DynamicService> services;

            Host.LockServices();

            try
            {
                // ReSharper disable ImplicitlyCapturedClosure
                services = Host.Services.Where(c => c.Name == resourceName);
                // ReSharper restore ImplicitlyCapturedClosure
            }
            finally
            {
                Host.UnlockServices();
            }

            // ReSharper disable PossibleMultipleEnumeration
            if (services.Any())
            // ReSharper restore PossibleMultipleEnumeration
            {
                // ReSharper disable PossibleMultipleEnumeration
                foreach (DynamicService item in services)
                // ReSharper restore PossibleMultipleEnumeration
                {
                    item.Actions.ForEach(c => c.ServiceActionInputs.ForEach(d =>
                                                                            sb.Append(
                                                                                string.Format("<input name=\"{0}\" />"
                        , d.Name))));
                }

                isService = true;
            }

            //This is a service so we can find all reverse dependencies
            // i.e these are items that need this service
            if (isService)
            {
                IEnumerable<DynamicService> reverseDependencies;
                Host.LockServices();

                try
                {
                    reverseDependencies =
                        // ReSharper disable ImplicitlyCapturedClosure
                        // ReSharper disable UseMethodAny.0
                        // ReSharper disable ReplaceWithSingleCallToCount
                        Host.Services.Where(c => c.Actions.Where(d => d.ServiceName == resourceName).Count() > 0);
                    // ReSharper restore ReplaceWithSingleCallToCount
                    // ReSharper restore UseMethodAny.0
                    // ReSharper restore ImplicitlyCapturedClosure
                }
                finally
                {
                    Host.UnlockServices();
                }

                reverseDependencies.ToList().ForEach(c => { reverseDependencyList.Add(c.Name); });

                reverseDependencies.ToList().ForEach(c =>
                {
                    c.Actions.ForEach(e =>
                    {
                        var activity = e.WorkflowActivity as DynamicActivity;
                        if (activity != null)
                        {
                            Activity flowChart;
                            //flowChart = activity.Implementation.Invoke();

                            try
                            {
                                flowChart = activity.Implementation.Invoke();
                            }
                            catch (XamlObjectWriterException)
                            {
                                flowChart = null;

                                if (!reverseDependencyList.Contains(activity.Name))
                                {
                                    reverseDependencyList.Add(activity.Name);
                                    brokenDependencies.Add(activity.Name);
                                }

                                // This exception occurs due to invalid workflows that still reference the old ForEach activity. (AdditionalData)
                            }


                            var workflow = flowChart as Flowchart;
                            if (workflow != null)
                            {
                                //Activities that are used by this resource
                                foreach (dynamic fn in workflow.Nodes)
                                {
                                    if ((fn is FlowStep) && fn.Action.GetType().Name == "DsfActivity")
                                    {
                                        if (fn.Action.ServiceName == resourceName)
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

            IEnumerable<Source> sources;
            Host.LockSources();

            try
            {
                // ReSharper disable ImplicitlyCapturedClosure
                sources = Host.Sources.Where(c => c.Name == resourceName);
                // ReSharper restore ImplicitlyCapturedClosure
            }
            finally
            {
                Host.UnlockSources();
            }

            //Source Resource Name was passed in 
            //find all services that 
            var serviceUsingSource = new List<string>();
            // ReSharper disable PossibleMultipleEnumeration
            if (sources.Any())
            // ReSharper restore PossibleMultipleEnumeration
            {
                Host.LockServices();

                try
                {
                    // ReSharper disable PossibleMultipleEnumeration
                    sources.ToList().ForEach(e =>
                        // ReSharper restore PossibleMultipleEnumeration
                                             Host.Services.ForEach(c => c.Actions.ForEach(d =>
                                             {
                                                 if (d.SourceName == e.Name)
                                                 {
                                                     serviceUsingSource.Add(c.Name);
                                                 }
                                             })));
                }
                finally
                {
                    Host.UnlockServices();
                }

                serviceUsingSource.ForEach(c => sb.Append(string.Format("<dependency id=\"{0}\" />", c)));
            }


            IEnumerable<DynamicService> workflowServices =
                // ReSharper disable UseMethodAny.0
                // ReSharper disable PossibleMultipleEnumeration
                services.Where(c => c.Actions.Where(d => d.ActionType == enActionType.Workflow).Count() > 0);
            // ReSharper restore PossibleMultipleEnumeration
            // ReSharper restore UseMethodAny.0


            var svcNames = new List<string>();
            workflowServices.ToList().ForEach(resource =>
            {
                IEnumerable<ServiceAction> workflowMatch =
                    resource.Actions.Where(d => d.ActionType == enActionType.Workflow);
                // ReSharper disable PossibleMultipleEnumeration
                if (workflowMatch.Any())
                // ReSharper restore PossibleMultipleEnumeration
                {
                    // ReSharper disable PossibleMultipleEnumeration
                    workflowMatch.ToList().ForEach(e =>
                    // ReSharper restore PossibleMultipleEnumeration
                    {
                        var activity = e.WorkflowActivity as DynamicActivity;
                        if (activity != null)
                        {
                            Activity flowChart;


                            try
                            {
                                flowChart = activity.Implementation.Invoke();
                            }
                            catch (XamlObjectWriterException)
                            {
                                flowChart = null;
                                // This exception occurs due to invalid workflows that still reference the old ForEach activity. (AdditionalData)
                            }


                            var workflow = flowChart as Flowchart;
                            if (workflow != null)
                            {
                                foreach (dynamic fn in workflow.Nodes)
                                {
                                    if ((fn is FlowStep) && fn.Action.GetType().Name == "DsfActivity")
                                    {
                                        if (!svcNames.Contains(fn.Action.ServiceName))
                                        {
                                            svcNames.Add(fn.Action.ServiceName);
                                        }
                                        sb.Append(string.Format("<dependency id=\"{0}\" />",
                                                                fn.Action.ServiceName));
                                    }
                                }
                            }
                        }
                    });
                }
            });


            IEnumerable<DynamicService> dsfServices;
            Host.LockServices();

            try
            {
                // ReSharper disable ImplicitlyCapturedClosure
                dsfServices = Host.Services.Where(c => c.Actions.Any(d => d.SourceName == resourceName));
                // ReSharper restore ImplicitlyCapturedClosure
            }
            finally
            {
                Host.UnlockServices();
            }

            dsfServices.ToList().ForEach(c => { if (!svcNames.Contains(c.Name)) svcNames.Add(c.Name); });


            sb.Append("</node>");

            if (serviceUsingSource.Count() > 0)
            {
                serviceUsingSource.ForEach(
                    c => sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"false\"/>", c)));
            }

            if (reverseDependencyList.Count > 0)
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


            IEnumerable<string> circularref = visitedServices.Intersect(svcNames).ToList();
            if (circularref.Count() > 0)
            {
                var circ = new StringBuilder();
                circularref.ToList().ForEach(s => circ.Append(string.Format("<dep>{0}</dep>", s)));

                //Jurie.Smit removed - circular references allowed, should be graphed
                //throw new Exception(string.Format("Circular Reference Detected between {0} and {1}!", resourceName, circ));
            }

            svcNames.Except(circularref).ToList().ForEach(c => sb.Append(FindDependenciesRecursive(c, Host)));

            return sb.ToString();
        }

        #endregion



    }
}
