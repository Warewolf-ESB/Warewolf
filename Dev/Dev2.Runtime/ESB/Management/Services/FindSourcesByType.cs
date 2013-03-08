using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find resources by type
    /// </summary>
    public class FindSourcesByType : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            IDynamicServicesHost theHost = theWorkspace.Host;
            StringBuilder result = new StringBuilder();
            string type;
            values.TryGetValue("Type", out type);

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("Type");
            }

            enSourceType sourceType;
            if (!Enum.TryParse(type, true, out sourceType))
            {
                sourceType = enSourceType.Unknown;
            }

            theHost.LockSources();
            var resources = new List<DynamicServiceObjectBase>();
            try
            {
                resources.AddRange(theHost.Sources.Where(c => c.Type == sourceType));
            }
            finally
            {
                theHost.UnlockSources();
            }

            resources.ForEach(c => result.Append(c.ResourceDefinition));

            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            var findSourcesByTypeAction = new ServiceAction {Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType()};

            var findSourcesByTypeService = new DynamicService { Name = HandlesType(), DataListSpecification = "<root><Type/></root>" };
            findSourcesByTypeService.Actions.Add(findSourcesByTypeAction);

            return findSourcesByTypeService;
        }

        public string HandlesType()
        {
            return "FindSourcesByType";
        }
    }
}
