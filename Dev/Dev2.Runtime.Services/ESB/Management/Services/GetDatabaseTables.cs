using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Newtonsoft.Json;
using ServiceStack.Common.Extensions;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetDatabaseTables : IEsbManagementEndpoint
    {
        #region Implementation of ISpookyLoadable<string>

        public string HandlesType()
        {
            return "GetDatabaseTablesService";
        }

        #endregion

        #region Implementation of IEsbManagementEndpoint

        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string database;
            values.TryGetValue("Database", out database);

            var dbSource = JsonConvert.DeserializeObject<DbSource>(database);
            DataTable tableInfo = null;
            DataTable columnInfo = null;
            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                // Connect to the database then retrieve the schema information.
                connection.Open();
                columnInfo = connection.GetSchema("Columns");
            }
            var tables = new List<DbTable>();
            if(columnInfo != null)
            {
                foreach(DataRow row in columnInfo.Rows)
                {
                    var tableName = row["TABLE_NAME"] as string;
                    var dbTable = tables.Find(table => table.TableName == tableName);
                    if(dbTable == null)
                    {
                        dbTable = new DbTable();
                        dbTable.TableName = tableName;
                        dbTable.Columns = new List<DbColumn>();
                        tables.Add(dbTable);
                    }
                    var columnName = row["COLUMN_NAME"] as string;
                    var dbColumn = new DbColumn { ColumnName = columnName };

                    SqlDbType sqlDataType;
                    var typeValue = row["DATA_TYPE"] as string;
                    if(Enum.TryParse(typeValue, true, out sqlDataType))
                    {
                        dbColumn.SqlDataType = sqlDataType;
                    }
                    var columnLength = row["CHARACTER_MAXIMUM_LENGTH"] is int ? (int)row["CHARACTER_MAXIMUM_LENGTH"] : -1;
                    dbColumn.MaxLength = columnLength;
                    dbTable.Columns.Add(dbColumn);
                }
            }
            return JsonConvert.SerializeObject(tables);
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
                DataListSpecification = @"<DataList><Database/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>"
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
