#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Syndication;
using System.Text;
using System.Windows.Markup;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.X6;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Newtonsoft.Json;
using ServiceStack;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveResourceJSON : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            if (requestArgs != null && requestArgs.Count > 0)
            {
                requestArgs.TryGetValue("ResourceXml", out StringBuilder resourceDefinition);
                if (resourceDefinition != null && resourceDefinition.Length > 0)
                {
                    var serializer = new Dev2JsonSerializer();
                    resourceDefinition = new StringBuilder(serializer.Deserialize<CompressedExecuteMessage>(resourceDefinition).GetDecompressedMessage());
                    var xml = resourceDefinition.ToXElement();
                    var resource = new Resource(xml);
                    var res = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resource.ResourceID);
                    if (res != null)
                    {
                        return res.ResourceID;
                    }
                }
            }
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                Dev2Logger.Info("Save Resource JSON Service", GlobalConstants.WarewolfInfo);

                var workspaceIdString = string.Empty;
                values.TryGetValue("savePath", out StringBuilder savePathValue);
                if (savePathValue == null)
                {
                    throw new InvalidDataContractException("SavePath is missing");
                }
                values.TryGetValue("ResourceJSON", out StringBuilder resourceDefinition);
                values.TryGetValue("WorkspaceID", out StringBuilder tmp);
                values.TryGetValue("Reason", out StringBuilder reason);

                if (tmp != null)
                {
                    workspaceIdString = tmp.ToString();
                }
                if (!Guid.TryParse(workspaceIdString, out Guid workspaceId))
                {
                    workspaceId = theWorkspace.ID;
                }

                if (resourceDefinition == null || resourceDefinition.Length == 0 || string.IsNullOrEmpty(resourceDefinition?.ToString()))
                {
                    throw new InvalidDataContractException("ResourceJSON is missing");
                }

                values.TryGetValue("ResourceXaml", out StringBuilder resourceXaml);

                if (resourceXaml == null || resourceXaml.Length == 0)
                {
                    throw new InvalidDataContractException("ResourceXaml is missing");
                }

                var x6Graph = JsonConvert.DeserializeObject<X6WorkflowSaveModel>(resourceDefinition.ToString());
                var workflow = new Workflow(new StringBuilder(x6Graph.WorkflowXml).ToXElement(), true);
                workflow.XamlDefinition = resourceXaml;
                var serviceXaml = workflow.ToServiceDefinition();

                var serializer = new Dev2JsonSerializer();                
                var res = new ExecuteMessage { HasError = false };

                var saveResult = ResourceCatalog.Instance.SaveResource(workspaceId, serviceXaml, savePathValue.ToString(), reason.ToString());
                
                if (workspaceId == GlobalConstants.ServerWorkspaceID && theWorkspace.ID != workspaceId)
                {
                    ResourceCatalog.Instance.SaveResource(theWorkspace.ID, serviceXaml, savePathValue.ToString(), reason.ToString());
                }
                
                res.SetMessage(saveResult.Message + " " + DateTime.Now);

                return serializer.SerializeToBuilder(res);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ResourceXml ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => ServiceName();
        public static string ServiceName() => "SaveResourceJSONService";
    }
}