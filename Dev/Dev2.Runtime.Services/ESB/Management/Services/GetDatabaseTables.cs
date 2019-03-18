#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using MySql.Data.MySqlClient;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetDatabaseTables : DefaultEsbManagementEndpoint
    {
        #region Implementation of DefaultEsbManagementEndpoint
        
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            if (values == null)
            {
                throw new InvalidDataContractException(ErrorResource.NoParameter);
            }
            string database = null;
            values.TryGetValue("Database", out StringBuilder tmp);
            if (tmp != null)
            {
                database = tmp.ToString();
            }

            if (string.IsNullOrEmpty(database))
            {
                var res = new DbTableList("No database set.");
                Dev2Logger.Debug("No database set.", GlobalConstants.WarewolfDebug);
                return serializer.SerializeToBuilder(res);
            }

            DbSource dbSource;
            DbSource runtimeDbSource = null;
            try
            {
                dbSource = serializer.Deserialize<DbSource>(database);

                if (dbSource.ResourceID != Guid.Empty)
                {
                    runtimeDbSource = ResourceCatalog.Instance.GetResource<DbSource>(theWorkspace.ID, dbSource.ResourceID);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                var res = new DbTableList("Invalid JSON data for Database parameter. Exception: {0}", e.Message);
                return serializer.SerializeToBuilder(res);
            }
            if (runtimeDbSource == null)
            {
                var res = new DbTableList("Invalid Database source");
                Dev2Logger.Debug("Invalid Database source", GlobalConstants.WarewolfDebug);
                return serializer.SerializeToBuilder(res);
            }
            if (string.IsNullOrEmpty(runtimeDbSource.DatabaseName) || string.IsNullOrEmpty(runtimeDbSource.Server))
            {
                var res = new DbTableList("Invalid database sent {0}.", database);
                Dev2Logger.Debug($"Invalid database sent {database}.", GlobalConstants.WarewolfDebug);
                return serializer.SerializeToBuilder(res);
            }

            try
            {
                return serializer.SerializeToBuilder(TryExecute(dbSource));
            }
            catch (Exception ex)
            {
                var tables = new DbTableList(ex);
                return serializer.SerializeToBuilder(tables);
            }
        }

        static DbTableList TryExecute(DbSource dbSource)
        {
            Dev2Logger.Info("Get Database Tables. " + dbSource.DatabaseName, GlobalConstants.WarewolfInfo);
            var tables = new DbTableList();
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.OrginalExecutingUser, () =>
            {
                DataTable columnInfo = null;
                switch (dbSource.ServerType)
                {
                    case enSourceType.SqlDatabase:
                        {
                            using (var connection = new SqlConnection(dbSource.ConnectionString))
                            {
                                connection.Open();
                                columnInfo = connection.GetSchema("Tables");
                            }
                            break;
                        }
                    default:
                        {
                            using (var connection = new MySqlConnection(dbSource.ConnectionString))
                            {
                                connection.Open();
                                columnInfo = connection.GetSchema("Tables");
                            }
                            break;
                        }
                }

                if (columnInfo != null)
                {
                    foreach (DataRow row in columnInfo.Rows)
                    {
                        var tableName = row["TABLE_NAME"] as string;
                        var schema = row["TABLE_SCHEMA"] as string;
                        tableName = '[' + tableName + ']';
                        var dbTable = tables.Items.Find(table => table.TableName == tableName && table.Schema == schema);
                        if (dbTable == null)
                        {
                            dbTable = new DbTable { Schema = schema, TableName = tableName, Columns = new List<IDbColumn>() };
                            tables.Items.Add(dbTable);
                        }
                    }
                }
                if (tables.Items.Count == 0)
                {
                    tables.HasErrors = true;
                    const string ErrorFormat = "The login provided in the database source uses {0} and most probably does not have permissions to perform the following query: "
                                          + "\r\n\r\n{1}SELECT * FROM INFORMATION_SCHEMA.TABLES;{2}";

                    if (dbSource.AuthenticationType == AuthenticationType.User)
                    {
                        tables.Errors = string.Format(ErrorFormat,
                            "SQL Authentication (User: '" + dbSource.UserID + "')",
                            "EXECUTE AS USER = '" + dbSource.UserID + "';\r\n",
                            "\r\nREVERT;");
                    }
                    else
                    {
                        tables.Errors = string.Format(ErrorFormat, "Windows Authentication", "", "");
                    }
                }
            });
            return tables;
        }

        #endregion

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Database ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetDatabaseTablesService";
    }
}
