#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
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
