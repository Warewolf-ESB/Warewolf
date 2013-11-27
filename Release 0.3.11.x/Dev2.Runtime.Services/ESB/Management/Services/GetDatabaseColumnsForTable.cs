using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using Dev2.DynamicServices;
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
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            if(values == null)
            {
                throw new InvalidDataContractException("No parameter values provided.");
            }
            string database;
            string tableName;
            values.TryGetValue("Database", out database);
            values.TryGetValue("TableName", out tableName);

            if(string.IsNullOrEmpty(database))
            {
                return new DbColumnList("No database set.").ToString();
            }
            if(string.IsNullOrEmpty(tableName))
            {
                return new DbColumnList("No table name set.").ToString();
            }

            try
            {
                var dbSource = JsonConvert.DeserializeObject<DbSource>(database);

                DataTable columnInfo;
                using(var connection = new SqlConnection(dbSource.ConnectionString))
                {
                    // Connect to the database then retrieve the schema information.
                    connection.Open();

                    // See http://msdn.microsoft.com/en-us/library/cc716722.aspx for restrictions
                    var restrictions = new string[4];
                    restrictions[0] = dbSource.DatabaseName;
                    restrictions[2] = tableName.Trim(new[] { '"' });
                    columnInfo = connection.GetSchema("Columns", restrictions);
                }

                var dbColumns = new DbColumnList();

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
                        dbColumns.Items.Add(dbColumn);
                    }
                }
                return dbColumns.ToString();
            }
            catch(Exception ex)
            {
                return new DbColumnList(ex).ToString();
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
                DataListSpecification = @"<DataList><Database/><TableName/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>"
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