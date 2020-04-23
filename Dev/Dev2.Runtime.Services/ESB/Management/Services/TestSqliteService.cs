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
    public class TestSqliteService : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Info("Test Sqlite Service", GlobalConstants.WarewolfInfo);

				values.TryGetValue("SqlQuery", out StringBuilder QueryString);

				var source = new SqliteDBSource();

                var res = new SqliteDBService
				{
                    Method = new ServiceMethod("TestService", "TestService", QueryString.ToString(), new OutputDescription(), new List<MethodOutput>()),
                    ResourceName = "TestService",
                    ResourceID = Guid.NewGuid(),
                    Source = source
                };

                var services = new ServiceModel.Services();
                Recordset output = null;
                Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.OrginalExecutingUser, () =>
                {
                    output = services.SqliteDbTest(res, GlobalConstants.ServerWorkspaceID, Guid.Empty);
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

        public string HandlesType() => "TestSqliteService";
    }
}
