using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;
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

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            if(string.IsNullOrEmpty(database))
            {
                var res = new DbColumnList("No database set.");
                return serializer.SerializeToBuilder(res);
            }
            if(string.IsNullOrEmpty(tableName))
            {
                var res = new DbColumnList("No table name set.");
                return serializer.SerializeToBuilder(res);
            }

            try
            {
                var dbSource = JsonConvert.DeserializeObject<DbSource>(database);
                dbSource = ResourceCatalog.Instance.GetResource<DbSource>(theWorkspace.ID, dbSource.ResourceID);
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
                return serializer.SerializeToBuilder(dbColumns);
            }
            catch(Exception ex)
            {
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