using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchDbHelpText
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
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            if (values == null)
            {
                throw new InvalidDataContractException("No parameter values provided.");
            }
            string database="";
            StringBuilder tmp;
            values.TryGetValue("Database", out tmp);
            if (tmp != null)
            {
                database = tmp.ToString();
            }

            if (tmp == null || string.IsNullOrEmpty(database))
            {
                var res = new DbTableList("No database set.");
                Dev2Logger.Log.Debug("No database set.");
                return serializer.SerializeToBuilder(res);
            }

            var procName = values["procName"].ToString();
            Guid dbSource;
            DbSource runtimeDbSource;
            try
            {
                 dbSource = Guid.Parse(database);
                runtimeDbSource = ResourceCatalog.Instance.GetResource<DbSource>(theWorkspace.ID, dbSource);
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error(e);
                var res = new DbTableList("Invalid JSON data for Database parameter. Exception: {0}", e.Message);
                return serializer.SerializeToBuilder(res);
            }
            if (runtimeDbSource == null)
            {
                var res = new DbTableList("Invalid Database source");
                Dev2Logger.Log.Debug("Invalid Database source");
                return serializer.SerializeToBuilder(res);
            }
            if (string.IsNullOrEmpty(runtimeDbSource.DatabaseName) || string.IsNullOrEmpty(runtimeDbSource.Server))
            {
                var res = new DbTableList("Invalid database sent {0}.", database);
                Dev2Logger.Log.Debug(String.Format("Invalid database sent {0}.", database));
                return serializer.SerializeToBuilder(res);
            }

            try
            {
                Dev2Logger.Log.Info("Get Database Tables. " + runtimeDbSource.DatabaseName);
                using (var connection = new SqlConnection(runtimeDbSource.ConnectionString))
                {
                    connection.Open();
                    using(var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = string.Format("sp_helptext '{0}'", procName);
                        
                        var output = (string) cmd.ExecuteScalar();
                        return new StringBuilder(output);
                    }
                }
               
            }
            catch (Exception ex)
            {
                
                return serializer.SerializeToBuilder(ex.Message);
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
                DataListSpecification = new StringBuilder("<DataList><Database ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
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
