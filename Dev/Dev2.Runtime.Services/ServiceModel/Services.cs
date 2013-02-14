using Dev2.DynamicServices;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
                var xmlStr = Resources.ReadXml(workspaceID, resourceType, argsObj.resourceID.Value);
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

        #region Methods

        // POST: Service/Services/Methods
        public ServiceMethodList Methods(string args, Guid workspaceID, Guid dataListID)
        {
            var result = new ServiceMethodList();
            try
            {
                var service = DeserializeService(args) as DbService;
                var serviceMethods = FetchMethods(service);
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
                if(addFields)
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
            if(addFields)
            {
                // TODO: Implement real stuff
                for(var j = 0; j < 30; j++)
                {
                    var colName = "Column" + (j + 1);
                    service.Recordset.Fields.Add(new RecordsetField { Name = colName, Alias = colName });
                }
            }

            // TODO: Implement real stuff
            var random = new Random();
            for(var i = 0; i < 15; i++)
            {
                service.Recordset.AddRecord(fieldIndex => random.GenerateString(30, string.Empty, true));
            }

            return service.Recordset;
        }

        #endregion

        #region FetchDatabaseBroker

        public virtual ServiceMethodList FetchMethods(DbService service)
        {
            var broker = new MsSqlBroker();
            return broker.GetServiceMethods(service);
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
