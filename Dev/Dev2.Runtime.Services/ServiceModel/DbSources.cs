using System;
using System.DirectoryServices;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Newtonsoft.Json;
using DbSource = Dev2.Runtime.ServiceModel.Data.DbSource;

namespace Dev2.Runtime.ServiceModel
{
    public class DbSources : ExceptionManager
    {
        #region Get

        // POST: Service/dbSources/Get
        public DbSource Get(string resourceID, Guid workspaceID, Guid dataListID)
        {
            var result = new DbSource { ResourceID = Guid.Empty, ResourceType = ResourceType.DbSource, AuthenticationType = AuthenticationType.Windows };
            try
            {
                var xmlStr = Resources.ReadXml(workspaceID, ResourceType.DbSource, resourceID);
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
        public string Save(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                if (args.IndexOf("\"resourceID\":null,", StringComparison.Ordinal) > 0)
                {
                    args = args.Replace("\"resourceID\":null,", "\"resourceID\":\"" + Guid.Empty + "\",");
                }
                var databaseSourceDetails = JsonConvert.DeserializeObject<DbSource>(args);

                // Setup ports using default
                switch(databaseSourceDetails.ServerType)
                {
                    case enSourceType.SqlDatabase:
                        {
                            databaseSourceDetails.Port = 1433;
                            break;
                        }
                    default:
                        break;
                }

                ResourceCatalog.Instance.SaveResource(workspaceID, databaseSourceDetails);
                if(workspaceID != GlobalConstants.ServerWorkspaceID)
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
        public string Search(string term, Guid workspaceID, Guid dataListID)
        {
            var root = new DirectoryEntry("WinNT:");

            // This search is case-sensitive!
            term = term.ToLower();
            var results = root.Children.Cast<DirectoryEntry>()
                              .SelectMany(dom => dom.Children.Cast<DirectoryEntry>()
                                                    .Where(entry => entry.SchemaClassName == "Computer" && entry.Name.ToLower().Contains(term)))
                              .Select(entry => entry.Name)
                              .ToList();

            return JsonConvert.SerializeObject(results);
        }

        #endregion

        #region Test

        // POST: Service/DbSources/Test
        public DatabaseValidationResult Test(string args, Guid workspaceID, Guid dataListID)
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

        protected virtual DatabaseValidationResult DoDatabaseValidation(DbSource dbSourceDetails)
        {
            var result = new DatabaseValidationResult();

            switch(dbSourceDetails.ServerType)
            {
                case enSourceType.SqlDatabase:
                    result.DatabaseList = new MsSqlBroker().GetDatabasesSchema(dbSourceDetails.ConnectionString);
                    break;
                default:
                    result.IsValid = false;
                    break;
            }
            return result;
        }

        #endregion
    }
}