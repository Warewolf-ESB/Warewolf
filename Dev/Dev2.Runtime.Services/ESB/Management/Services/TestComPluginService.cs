#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestComPluginService : EsbManagementEndpointBase
    {
        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Test ComPlugin Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue("ComPluginService", out StringBuilder resourceDefinition);
                var src = serializer.Deserialize<IComPluginService>(resourceDefinition);

                var parameters = src.Inputs?.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList() ?? new List<MethodParameter>();
                
                var pluginsrc = ResourceCatalog.Instance.GetResource<ComPluginSource>(GlobalConstants.ServerWorkspaceID, src.Source.Id);
                var res = new ComPluginService
                {
                    Method = new ServiceMethod(src.Action.Method, src.Name, parameters, new OutputDescription(), new List<MethodOutput>(), "test"),
                    Namespace = src.Action.FullName,
                    ResourceName = src.Name,
                    ResourceID = src.Id,
                    Source = pluginsrc
                };

                var result = _pluginServices.Value.Test(serializer.SerializeToBuilder(res).ToString(), out string serializedResult);

                if (serializedResult.StartsWith("Exception: "))
                {
                    msg.HasError = true;
                    msg.Message = new StringBuilder(serializedResult);
                    Dev2Logger.Error(serializedResult, GlobalConstants.WarewolfError);
                }
                else
                {
                    msg.HasError = false;
                    msg.Message = serializer.SerializeToBuilder(new RecordsetListWrapper { Description = result.Description, RecordsetList = result, SerializedResult = serializedResult });
                }
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ComPluginService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        readonly Lazy<IComPluginServices> _pluginServices = new Lazy<IComPluginServices>(() => new ComPluginServices());

        public override string HandlesType() => "TestComPluginService";
    }
}