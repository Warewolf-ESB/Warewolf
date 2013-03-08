using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find a resource by its id
    /// </summary>
    public class FindResourcesByID : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            IDynamicServicesHost theHost = theWorkspace.Host;
            StringBuilder result = new StringBuilder("<Nothing/>");
            string guidCsv;
            string type;

            values.TryGetValue("GuidCsv", out guidCsv);
            values.TryGetValue("Type", out type);

            if (guidCsv == null)
            {
                guidCsv = string.Empty;
            }

            if (type == null)
            {
                throw new ArgumentNullException("Type");
            }

            var guids = new List<Guid>();
            foreach (var guidStr in guidCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                Guid guid;
                if (Guid.TryParse(guidStr, out guid))
                {
                    guids.Add(guid);
                }
            }

            var resources = new List<DynamicServiceObjectBase>();
            if (guids.Count > 0)
            {
                switch (type)
                {
                    default: // Sources
                        theHost.LockSources();
                        try
                        {
                            resources.AddRange(theHost.Sources.Where(c => guids.Contains(c.ID)));
                        }
                        finally
                        {
                            theHost.UnlockSources();
                        }
                    break;
                }
            }

            resources.ForEach(c => result.Append(c.ResourceDefinition));
            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            var findResourcesByIDAction = new ServiceAction { Name = HandlesType(), SourceMethod =  HandlesType(), ActionType = enActionType.InvokeManagementDynamicService};

            var findResourcesByIDService = new DynamicService { Name = HandlesType(), DataListSpecification = "<root><GuidCsv/><Type/></root>" };
            findResourcesByIDService.Actions.Add(findResourcesByIDAction);



            return findResourcesByIDService;
        }

        public string HandlesType()
        {
            return "FindResourcesByID";
        }
    }
}
