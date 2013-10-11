using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
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
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string database;
            string tableName;
            values.TryGetValue("Database", out database);
            values.TryGetValue("Tablename", out tableName);

            var dbSource = JsonConvert.DeserializeObject<DbSource>(database);
            DataTable columnInfo;
            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                // Connect to the database then retrieve the schema information.
                connection.Open();
                columnInfo = connection.GetSchema("Columns", new[] { "DBName", null, tableName });
            }
            var dbColumns = new List<DbColumn>();
            if(columnInfo != null)
            {
                foreach(DataRow row in columnInfo.Rows)
                {
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
                    dbColumns.Add(dbColumn);
                }
            }
            return JsonConvert.SerializeObject(dbColumns);
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
                DataListSpecification = @"<DataList><Database/><Tablename/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>"
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