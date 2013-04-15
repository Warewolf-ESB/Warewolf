using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Unlimited.Framework;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Get the latest services
    /// </summary>
    public class GetLatest : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {

            StringBuilder result = new StringBuilder();

            string editedItemsXml;

            values.TryGetValue("EditedItemsXml", out editedItemsXml);

            try
            {
                var editedItems = new List<string>();

                if (!string.IsNullOrWhiteSpace(editedItemsXml))
                {
                    editedItems.AddRange(XElement.Parse(editedItemsXml)
                        .Elements()
                        .Select(x => x.Attribute("ServiceName").Value));
                }

                WorkspaceRepository.Instance.GetLatest(theWorkspace, editedItems);
                return "<Result>Workspace updated</Result>";
            }
            catch (Exception ex)
            {
                result.Append("<Error>Error updating workspace</Error>");
                TraceWriter.WriteTrace(ex.StackTrace);
            }

            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            var getLatestAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType()};

            var getLatestService = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><EditedItemsXml/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };
            getLatestService.Actions.Add(getLatestAction);

            return getLatestService;
        }

        public string HandlesType()
        {
            return "GetLatestService";
        }
    }
}
