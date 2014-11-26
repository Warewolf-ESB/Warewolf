
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Update a workspace Item
    /// </summary>
    public class UpdateWorkspaceItem : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {

            StringBuilder itemXml;
            string isLocal = string.Empty;

            StringBuilder tmp;
            values.TryGetValue("ItemXml", out itemXml);
            values.TryGetValue("IsLocalSave", out tmp);
            if (tmp != null)
            {
                isLocal = tmp.ToString();
            }

            bool isLocalSave;

            bool.TryParse(isLocal, out isLocalSave);

            var res = new ExecuteMessage { HasError = false};

            if(itemXml == null || itemXml.Length == 0)
            {
                res.SetMessage("Invalid workspace item definition " + DateTime.Now);
                res.HasError = true;
            }
            else
            {
                try
                {
                    XElement xe = itemXml.ToXElement();

                    var workspaceItem = new WorkspaceItem(xe);
                    if (workspaceItem.WorkspaceID != theWorkspace.ID)
                    {
                        res.SetMessage("Cannot update a workspace item from another workspace " + DateTime.Now);
                        res.HasError = true;
                    }
                    else
                    {
                        theWorkspace.Update(workspaceItem, isLocalSave);
                        res.SetMessage("Workspace item updated " + DateTime.Now);
                    }
                }
                catch(Exception ex)
                {
                    res.SetMessage("Error updating workspace item " + DateTime.Now);
                    res.SetMessage(ex.Message);
                    res.SetMessage(ex.StackTrace);
                    res.HasError = true;
                }
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(res);
        }

        public DynamicService CreateServiceEntry()
        {
            var workspaceItemService = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><IsLocalSave ColumnIODirection=\"Input\"/><ItemXml ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var workspaceItemAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            workspaceItemService.Actions.Add(workspaceItemAction);

            return workspaceItemService;
        }

        public string HandlesType()
        {
            return "UpdateWorkspaceItemService";
        }
    }
}
