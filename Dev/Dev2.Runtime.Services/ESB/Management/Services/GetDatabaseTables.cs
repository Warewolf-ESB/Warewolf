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
            if(values == null)
            {
                throw new InvalidDataContractException("No parameter values provided.");
            }
            string database;
            DbSource dbSource = null;
            values.TryGetValue("Database", out database);
            if(string.IsNullOrEmpty(database))
            {
                throw new InvalidDataContractException("No database set.");
            }
            try
            {
                dbSource = JsonConvert.DeserializeObject<DbSource>(database);
            }
            catch(Exception e)
            {
                throw new InvalidDataContractException(string.Format("Invalid JSON data for Database parameter. Exception: {0}", e.Message));
            }
            if(String.IsNullOrEmpty(dbSource.DatabaseName) || String.IsNullOrEmpty(dbSource.Server))
            {
                throw new InvalidDataContractException(string.Format("Invalid database sent {0}.", database));
            }
            DataTable columnInfo = null;
            var tables = new List<DbTable>();
            using(var connection = new SqlConnection(dbSource.ConnectionString))
            {
                try
                {
                    connection.Open();
                    columnInfo = connection.GetSchema("Tables");
                }
                catch(Exception e)
                {
                    var dbTable = new DbTable { TableName = e.Message, Columns = new List<DbColumn>() };
                    tables.Add(dbTable);
                }
            }
            if(columnInfo != null)
            {
                foreach(DataRow row in columnInfo.Rows)
                {
                    var tableName = row["TABLE_NAME"] as string;
                    var dbTable = tables.Find(table => table.TableName == tableName);
                    if(dbTable == null)
                    {
                        dbTable = new DbTable { TableName = tableName, Columns = new List<DbColumn>() };
                        tables.Add(dbTable);
                    }
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
