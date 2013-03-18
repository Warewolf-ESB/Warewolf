using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Unlimited.Framework;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Update a workspace Item
    /// </summary>
    public class UpdateWorkspaceItem : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {

            string itemXml;
            string roles;

            values.TryGetValue("ItemXml", out itemXml);
            values.TryGetValue("Roles", out roles);


            dynamic xmlResponse = new UnlimitedObject(Resources.DynamicService_ServiceResponseTag);
            if (string.IsNullOrEmpty(itemXml))
            {
                xmlResponse.Error = "Invalid workspace item definition";
            }
            else
            {
                try
                {
                    string cleanXML = itemXml.Replace("&gt;", ">").Replace("&lt;", "<");

                    var workspaceItem = new WorkspaceItem(XElement.Parse(cleanXML));
                    if (workspaceItem.WorkspaceID != theWorkspace.ID)
                    {
                        xmlResponse.Error = "Cannot update a workspace item from another workspace";
                    }
                    else
                    {
                        theWorkspace.Update(workspaceItem, roles);
                        xmlResponse.Response = "Workspace item updated";
                    }
                }
                catch (Exception ex)
                {
                    xmlResponse.Error = "Error updating workspace item";
                    xmlResponse.ErrorDetail = ex.Message;
                    xmlResponse.ErrorStackTrace = ex.StackTrace;
                }
            }

            return xmlResponse.XmlString;
        }

        public DynamicService CreateServiceEntry()
        {
            var workspaceItemService = new DynamicService { Name = HandlesType(), DataListSpecification = "<root><ItemXml/><Roles/></root>" };

            var workspaceItemAction = new ServiceAction {  Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType()};
            workspaceItemService.Actions.Add(workspaceItemAction);

            return workspaceItemService;
        }

        public string HandlesType()
        {
            return "UpdateWorkspaceItemService";
        }
    }
}
