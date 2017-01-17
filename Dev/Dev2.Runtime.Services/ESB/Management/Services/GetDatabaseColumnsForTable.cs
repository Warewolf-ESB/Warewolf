/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Oracle.ManagedDataAccess.Client;
using System.Data.Odbc;
using Dev2.Services.Security;
using MySql.Data.MySqlClient;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    // NOTE: Only use for design time in studio as errors will NOT be forwarded!
    public class GetDatabaseColumnsForTable : IEsbManagementEndpoint
    {
        #region Implementation of ISpookyLoadable<string>

        public string HandlesType()
        {
            return "GetDatabaseColumnsForTableService";
        }

        #endregion

        #region Implementation of IEsbManagementEndpoint

        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if(values == null)
            {
                throw new InvalidDataContractException(ErrorResource.NoParameter);
            }
            string database = null;
            string tableName = null;
            string schema = null;
            StringBuilder tmp;
            values.TryGetValue("Database", out tmp);
            if(tmp != null)
            {
                database = tmp.ToString();
            }
            values.TryGetValue("TableName", out tmp);
            if(tmp != null)
            {
                tableName = tmp.ToString();
            }

            values.TryGetValue("Schema", out tmp);
            if(tmp != null)
            {
                schema = tmp.ToString();
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            if(string.IsNullOrEmpty(database))
            {
                var res = new DbColumnList("No database set.");
                Dev2Logger.Debug("No database set.");
                return serializer.SerializeToBuilder(res);
            }
            if(string.IsNullOrEmpty(tableName))
            {
                var res = new DbColumnList("No table name set.");
                Dev2Logger.Debug("No table name set.");
                return serializer.SerializeToBuilder(res);
            }
            Dev2Logger.Info($"Get Database Columns For Table. Database:{database} Schema:{schema} Table{tableName}");
            try
            {
                var dbSource = serializer.Deserialize<DbSource>(database);
                var runtTimedbSource = ResourceCatalog.Instance.GetResource<DbSource>(theWorkspace.ID, dbSource.ResourceID);
                DataTable columnInfo;
                switch (dbSource.ServerType)
                {
                    case enSourceType.MySqlDatabase:
                    {
                        using (var connection = new MySqlConnection(runtTimedbSource.ConnectionString))
                        {
                            // Connect to the database then retrieve the schema information.
                            connection.Open();
                            var sql = @"select  * from  " + tableName.Trim('"').Replace("[","").Replace("]","") + " Limit 1 ";

                                                     using (var sqlcmd = new MySqlCommand(sql, connection))
                            {
                                // force it closed so we just get the proper schema ;)
                                using (var sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection))
                                {
                                    columnInfo = sdr.GetSchemaTable();
                                }
                            }
                        }
                        break;
                    }
                    case enSourceType.Oracle:
                        {
                            using (var connection = new OracleConnection(runtTimedbSource.ConnectionString))
                            {
                                // Connect to the database then retrieve the schema information.
                                connection.Open();
                                var sql = @"select  * from  " + tableName.Trim('"').Replace("[", "").Replace("]", "") + " Limit 1 ";

                                using (var sqlcmd = new OracleCommand(sql, connection))
                                {
                                    // force it closed so we just get the proper schema ;)
                                    using (var sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection))
                                    {
                                        columnInfo = sdr.GetSchemaTable();
                                    }
                                }
                            }
                            break;
                        }
                    case enSourceType.ODBC:
                        {
                            using (var connection = new OdbcConnection(runtTimedbSource.ConnectionString))
                            {
                                // Connect to the database then retrieve the schema information.
                                connection.Open();
                                var sql = @"select  * from  " + tableName.Trim('"').Replace("[", "").Replace("]", "") + " Limit 1 ";

                                using (var sqlcmd = new OdbcCommand(sql, connection))
                                {
                                    // force it closed so we just get the proper schema ;)
                                    using (var sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection))
                                    {
                                        columnInfo = sdr.GetSchemaTable();
                                    }
                                }
                            }
                            break;
                        }

                    default:
                        {
                            using (var connection = new SqlConnection(runtTimedbSource.ConnectionString))
                            {
                                // Connect to the database then retrieve the schema information.
                                connection.Open();

                                // GUTTED TO RETURN ALL REQUIRED DATA ;)
                                if (schema == null)
                                {
                                    schema = string.Empty;
                                }
                                var sql = @"select top 1 * from " + schema.Trim('"') + "." + tableName.Trim('"');

                                using (var sqlcmd = new SqlCommand(sql, connection))
                                {
                                    // force it closed so we just get the proper schema ;)
                                    using (var sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection))
                                    {
                                        columnInfo = sdr.GetSchemaTable();
                                    }
                                }
                            }
                            break;
                        }
                }

                var dbColumns = new DbColumnList();

                if(columnInfo != null)
                {
                    foreach(DataRow row in columnInfo.Rows)
                    {
                        var columnName = row["ColumnName"] as string;
                        var isNullable = row["AllowDBNull"] is bool && (bool)row["AllowDBNull"];
                        var isIdentity = row["IsIdentity"] is bool && (bool)row["IsIdentity"];
                        var dbColumn = new DbColumn { ColumnName = columnName, IsNullable = isNullable, IsAutoIncrement = isIdentity };

                        SqlDbType sqlDataType;
                        var typeValue = dbSource.ServerType == enSourceType.SqlDatabase? row["DataTypeName"] as string:((Type)row["DataType"]).Name;
                        if(Enum.TryParse(typeValue, true, out sqlDataType))
                        {
                            dbColumn.SqlDataType = sqlDataType;
                        }

                        var columnLength = row["ColumnSize"] as int? ?? -1;
                        dbColumn.MaxLength = columnLength;
                        dbColumns.Items.Add(dbColumn);
                    }
                }
                return serializer.SerializeToBuilder(dbColumns);
            }
            catch(Exception ex)
            {
                Dev2Logger.Error(ex);
                var res = new DbColumnList(ex);
                return serializer.SerializeToBuilder(res);
            }
        }

        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        public DynamicService CreateServiceEntry()
        {
            var ds = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><Database ColumnIODirection=\"Input\"/><TableName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };

            var sa = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            ds.Actions.Add(sa);

            return ds;
        }

        #endregion

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }
    }
}
