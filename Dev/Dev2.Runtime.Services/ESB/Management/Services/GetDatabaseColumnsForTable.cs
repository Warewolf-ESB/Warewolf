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
using Newtonsoft.Json;

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
                throw new InvalidDataContractException("No parameter values provided.");
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
                Dev2Logger.Log.Debug("No database set.");
                return serializer.SerializeToBuilder(res);
            }
            if(string.IsNullOrEmpty(tableName))
            {
                var res = new DbColumnList("No table name set.");
                Dev2Logger.Log.Debug("No table name set.");
                return serializer.SerializeToBuilder(res);
            }
            Dev2Logger.Log.Info(String.Format("Get Database Columns For Table. Database:{0} Schema:{1} Table{2}" ,database,schema,tableName));
            try
            {
                var dbSource = JsonConvert.DeserializeObject<DbSource>(database);
                var runtTimedbSource = ResourceCatalog.Instance.GetResource<DbSource>(theWorkspace.ID, dbSource.ResourceID);
                DataTable columnInfo;
                using(var connection = new SqlConnection(runtTimedbSource.ConnectionString))
                {
                    // Connect to the database then retrieve the schema information.
                    connection.Open();

                    // GUTTED TO RETURN ALL REQUIRED DATA ;)
                    if(schema == null)
                    {
                        schema = string.Empty;
                    }

                    var sql = @"select top 1 * from " + schema.Trim(new[] { '"' }) + "." + tableName.Trim(new[] { '"' });
                    using(var sqlcmd = new SqlCommand(sql, connection))
                    {
                        // force it closed so we just get the proper schema ;)
                        using(var sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            columnInfo = sdr.GetSchemaTable();
                        }
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
                        var typeValue = row["DataTypeName"] as string;
                        if(Enum.TryParse(typeValue, true, out sqlDataType))
                        {
                            dbColumn.SqlDataType = sqlDataType;
                        }

                        var columnLength = row["ColumnSize"] is int ? (int)row["ColumnSize"] : -1;
                        dbColumn.MaxLength = columnLength;
                        dbColumns.Items.Add(dbColumn);
                    }
                }
                return serializer.SerializeToBuilder(dbColumns);
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error(ex);
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
                DataListSpecification = "<DataList><Database ColumnIODirection=\"Input\"/><TableName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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
    }
}