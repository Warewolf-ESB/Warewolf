using System.Collections.Generic;
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
            string database;
            string tableName;
            values.TryGetValue("Database", out database);

            var dbSource = JsonConvert.DeserializeObject<DbSource>(database);

            var tables = new List<DbTable>();

            for(var i = 0; i < 10; i++)
            {
                tables.AddRange(new List<DbTable>
                {
                    new DbTable
                    {
                        TableName = "Table" + i, Columns = new List<DbColumn>
                        {
                            new DbColumn { ColumnName = "Column" + i + "_1", DataType = typeof(string), MaxLength = 50 },
                            new DbColumn { ColumnName = "Column" + i + "_2", DataType = typeof(int) },
                            new DbColumn { ColumnName = "Column" + i + "_3", DataType = typeof(double) },
                            new DbColumn { ColumnName = "Column" + i + "_4", DataType = typeof(float) }
                        }
                    },
                });
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
