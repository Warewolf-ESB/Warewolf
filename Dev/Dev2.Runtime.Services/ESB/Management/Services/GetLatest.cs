
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
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Get the latest services
    /// </summary>
    public class GetLatest : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage res = new ExecuteMessage { HasError = false };

            string editedItemsXml = null;

            StringBuilder tmp;
            values.TryGetValue("EditedItemsXml", out tmp);
            if(tmp != null)
            {
                editedItemsXml = tmp.ToString();
            }

            try
            {
                var editedItems = new List<string>();

                if(!string.IsNullOrWhiteSpace(editedItemsXml))
                {
                    editedItems.AddRange(XElement.Parse(editedItemsXml)
                        .Elements()
                        .Select(x => x.Attribute("ServiceName").Value));
                }

                WorkspaceRepository.Instance.GetLatest(theWorkspace, editedItems);
                res.SetMessage("Workspace updated " + DateTime.Now);
            }
            catch(Exception ex)
            {
                res.SetMessage("Error updating workspace " + DateTime.Now);
                Dev2Logger.Log.Error(ex);
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(res);
        }

        public DynamicService CreateServiceEntry()
        {
            var getLatestAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            var getLatestService = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><EditedItemsXml ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            getLatestService.Actions.Add(getLatestAction);

            return getLatestService;
        }

        public string HandlesType()
        {
            return "GetLatestService";
        }
    }
}
