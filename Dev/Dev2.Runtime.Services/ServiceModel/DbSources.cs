
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    public class DbSources : ExceptionManager
    {
        #region Get

        // POST: Service/dbSources/Get
        public DbSource Get(string resourceId, Guid workspaceId, Guid dataListId)
        {
            var result = new DbSource { ResourceID = Guid.Empty, ResourceType = ResourceType.DbSource, AuthenticationType = AuthenticationType.Windows };
            try
            {
                var xmlStr = ResourceCatalog.Instance.GetResourceContents(workspaceId, Guid.Parse(resourceId)).ToString();
                if(!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    result = new DbSource(xml);
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

        #region Save

        // POST: Service/DbSources/Save
        public string Save(string args, Guid workspaceId, Guid dataListId)
        {
            try
            {
                var databaseSourceDetails = JsonConvert.DeserializeObject<DbSource>(args);

                // Setup ports using default
                switch(databaseSourceDetails.ServerType)
                {
                    case enSourceType.SqlDatabase:
                        {
                            databaseSourceDetails.Port = 1433;
                            break;
                        }
                }

                ResourceCatalog.Instance.SaveResource(workspaceId, databaseSourceDetails);
                if(workspaceId != GlobalConstants.ServerWorkspaceID)
                {
                    //2012.03.12: Ashley Lewis - BUG 9208
                    ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, databaseSourceDetails);
                }

                return databaseSourceDetails.ToString();
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new DatabaseValidationResult { IsValid = false, ErrorMessage = ex.Message }.ToString();
            }
        }

        #endregion

        #region Search

        // POST: Service/DbSources/Search
        public string Search(string term, Guid workspaceId, Guid dataListId)
        {
            var results = GetComputerNames.ComputerNames.FindAll(s => s.Contains(term));

            return JsonConvert.SerializeObject(results);
        }

        #endregion

        #region Test

        // POST: Service/DbSources/Test
        public DatabaseValidationResult Test(string args, Guid workspaceId, Guid dataListId)
        {
            var result = new DatabaseValidationResult
            {
                IsValid = false,
                ErrorMessage = "Unknown connection type."
            };

            try
            {
                var dbSourceDetails = JsonConvert.DeserializeObject<DbSource>(args);
                switch(dbSourceDetails.ResourceType)
                {
                    case ResourceType.DbSource:
                        result.ErrorMessage = null;
                        result = DoDatabaseValidation(dbSourceDetails);
                        break;
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                result.ErrorMessage = ex.Message;
            }
            return result;
        }

        #endregion

        #region Get database list

        public  virtual DatabaseValidationResult DoDatabaseValidation(DbSource dbSourceDetails)
        {
            var result = new DatabaseValidationResult();

            switch(dbSourceDetails.ServerType)
            {
                case enSourceType.SqlDatabase:
                    var broker = CreateDatabaseBroker();
                    result.DatabaseList = broker.GetDatabases(dbSourceDetails);
                    break;
                default:
                    result.IsValid = false;
                    break;
            }
            return result;
        }

        #endregion

        protected virtual SqlDatabaseBroker CreateDatabaseBroker()
        {
            return new SqlDatabaseBroker();
        }
    }
}
