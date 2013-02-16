using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Dev2.Runtime.ServiceModel
{
    public class Services : ExceptionManager
    {
        #region Get

        // POST: Service/Services/Get
        public Service Get(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                dynamic argsObj = JObject.Parse(args);

                var resourceType = (enSourceType)Resources.ParseResourceType(argsObj.resourceType.Value);
                var xmlStr = Resources.ReadXml(workspaceID, GlobalConstants.ServicesDirectory, argsObj.resourceID.Value);
                if(!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    switch(resourceType)
                    {
                        case enSourceType.SqlDatabase:
                        case enSourceType.MySqlDatabase:
                            return new DbService(xml);

                        case enSourceType.Plugin:
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }

            return DbService.Create();
        }

        #endregion

        #region DbMethods

        // POST: Service/Services/DbMethods
        public ServiceMethodList DbMethods(string args, Guid workspaceID, Guid dataListID)
        {
            var result = new ServiceMethodList();
            try
            {
                var source = JsonConvert.DeserializeObject<DbSource>(args);
                var serviceMethods = FetchMethods(source);
                result.AddRange(serviceMethods);
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

        #region Test

        // POST: Service/Services/Test
        public Recordset Test(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var service = DeserializeService(args);

                if(string.IsNullOrEmpty(service.Recordset.Name))
                {
                    service.Recordset.Name = service.Method.Name;
                }

                var addFields = service.Recordset.Fields.Count == 0;
                if (addFields)
                {
                    service.Recordset.Fields.Clear();
                }
                service.Recordset.Records.Clear();

                return FetchRecordset(service, addFields);
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new Recordset { HasErrors = true, ErrorMessage = ex.Message };
            }
        }

        #endregion

        #region Save

        // POST: Service/Services/Save
        public string Save(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var service = DeserializeService(args);

                if(service.ResourceID == Guid.Empty)
                {
                    service.ResourceID = Guid.NewGuid();
                }
                service.Save(workspaceID, dataListID);
                return service.ToString();
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message }.ToString();
            }
        }

        #endregion

        #region FetchRecordset

        public virtual Recordset FetchRecordset(Service service, bool addFields)
        {
            DbService dbService = service as DbService;
            if (dbService == null)
            {
                throw new ArgumentException(string.Format("Service of type '{0}' expected, '{1}' reveived.'", typeof(DbService), service.GetType()), "service");
            }

            //
            // Using the MsSqlBroker run the service test mode
            //
            var broker = new MsSqlBroker();
            var outputDescription = broker.TestService(dbService);

            if (outputDescription == null || outputDescription.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
            {
                throw new Exception("Error retrieving shape from service output.");
            }

            //
            // Add path data to recordset
            //
            if (addFields)
            {
                //
                // Add paths as fields
                //
                foreach (var path in outputDescription.DataSourceShapes[0].Paths)
                {
                    service.Recordset.Fields.Add(new RecordsetField { Name = path.DisplayPath, Alias = path.OutputExpression, Path = path });
                }
            }
            else
            {
                //
                // Remove fields for paths that no longer exist
                //
                var fieldsToRemove = service.Recordset.Fields.Where(r => !outputDescription.DataSourceShapes[0].Paths.Any(p => p.DisplayPath == r.Path.DisplayPath)).ToList();
                foreach(var recordsetField in fieldsToRemove)
                {
                    service.Recordset.Fields.Remove(recordsetField);
                }

                //
                // Add fields for new paths
                //
                var pathsToAdd = outputDescription.DataSourceShapes[0].Paths.Where(r => !service.Recordset.Fields.Any(f => f.Path.DisplayPath == r.DisplayPath)).ToList();
                foreach (var path in pathsToAdd)
                {
                    service.Recordset.Fields.Add(new RecordsetField { Name = path.DisplayPath, Alias = path.OutputExpression, Path = path });
                }
            }

            service.Recordset.AddRecord(fieldIndex => 
                {
                    if (fieldIndex < outputDescription.DataSourceShapes[0].Paths.Count)
                    {
                        return outputDescription.DataSourceShapes[0].Paths[fieldIndex].SampleData;
                    }

                    return "Index out of bounds";
                });

            return service.Recordset;
        }

        #endregion

        #region FetchMethods

        public virtual ServiceMethodList FetchMethods(DbSource source)
        {
            var broker = new MsSqlBroker();
            return broker.GetServiceMethods(source);
        }

        #endregion

        #region DeserializeService

        static Service DeserializeService(string args)
        {
            var service = JsonConvert.DeserializeObject<Service>(args);
            switch(service.ResourceType)
            {
                case enSourceType.SqlDatabase:
                case enSourceType.MySqlDatabase:
                    return JsonConvert.DeserializeObject<DbService>(args);

                case enSourceType.Plugin:
                    break;
            }
            return service;
        }

        #endregion
    }
}
