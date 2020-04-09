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
using System.Data;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestDbService : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Info("Test DB Connection Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue("DbService", out StringBuilder resourceDefinition);

                var src = serializer.Deserialize<IDatabaseService>(resourceDefinition);

                var parameters = src.Inputs?.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value }).ToList() ?? new List<MethodParameter>();
                
                var source = ResourceCatalog.Instance.GetResource<DbSource>(GlobalConstants.ServerWorkspaceID, src.Source.Id) ?? new DbSource
                             {
                                 DatabaseName = src.Source.DbName,
                                 ResourceID = src.Source.Id,
                                 ServerType = src.Source.Type,
                                 ResourceType = "DbSource"
                             };

                var res = new DbService
                {
                    Method = new ServiceMethod(src.Name, src.Name, parameters, new OutputDescription(), new List<MethodOutput>(), src.Action.ExecuteAction),
                    ResourceName = src.Name,
                    ResourceID = src.Id,
                    Source = source,
                    CommandTimeout = src.CommandTimeout
                };

                var services = new ServiceModel.Services();
                Recordset output = null;
                Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.OrginalExecutingUser, () =>
                {
                    output = services.DbTest(res, GlobalConstants.ServerWorkspaceID, Guid.Empty);
                    if (output.HasErrors)
                    {
                        msg.HasError = true;
                        var errorMessage = output.ErrorMessage;
                        msg.Message = new StringBuilder(errorMessage);
                        Dev2Logger.Error(errorMessage, GlobalConstants.WarewolfError);
                    }
                    else
                    {
                        var result = ToDataTable(output);
                        msg.HasError = false;
                        msg.Message = serializer.SerializeToBuilder(result);
                    }
                });
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);

            }

            return serializer.SerializeToBuilder(msg);
        }

        DataTable ToDataTable(Recordset output)
        {
            var dt = new DataTable(output.Name);

            foreach (var recordsetField in output.Fields)
            {
                dt.Columns.Add(new DataColumn(recordsetField.Name));
            }

            foreach(var row in output.Records)
            {
                var data = new List<object>();
                data.AddRange(row.Cells.Select(a => a.Value));
                dt.Rows.Add(data.ToArray());
            }
            return dt;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><DbSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "TestDbService";
    }
}
