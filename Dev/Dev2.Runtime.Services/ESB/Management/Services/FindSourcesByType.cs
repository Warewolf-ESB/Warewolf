using System;
using System.Collections.Generic;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
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
            string type;
            values.TryGetValue("Type", out type);

            if(string.IsNullOrEmpty(type))
            {
                // ReSharper disable NotResolvedInText
                throw new ArgumentNullException("type");
                // ReSharper restore NotResolvedInText
            }

            enSourceType sourceType;
            if(!Enum.TryParse(type, true, out sourceType))
            {
                sourceType = enSourceType.Unknown;
            }

            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            var result = ResourceCatalog.Instance.GetPayload(theWorkspace.ID, sourceType);
            return result;

        }

        public DynamicService CreateServiceEntry()
        {
            var findSourcesByTypeAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            var findSourcesByTypeService = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><Type/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };
            findSourcesByTypeService.Actions.Add(findSourcesByTypeAction);

            return findSourcesByTypeService;
        }

        public string HandlesType()
        {
            return "FindSourcesByType";
        }
    }
}
