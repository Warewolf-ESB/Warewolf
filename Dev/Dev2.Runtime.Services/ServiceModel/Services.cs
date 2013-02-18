using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

                var resourceType = (ResourceType)Resources.ParseResourceType(argsObj.resourceType.Value);
                var xmlStr = Resources.ReadXml(workspaceID, resourceType, argsObj.resourceID.Value);
                if(!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    switch(resourceType)
                    {
                        case ResourceType.DbService:
                            return new DbService(xml);

                        case ResourceType.Plugin:
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

        #region DbTest

        // POST: Service/Services/DbTest
        public Recordset DbTest(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var service = JsonConvert.DeserializeObject<DbService>(args);

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
                if(workspaceID != GlobalConstants.ServerWorkspaceID)
                {
                    service.Save(GlobalConstants.ServerWorkspaceID, dataListID);
                }
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

        public virtual Recordset FetchRecordset(DbService dbService, bool addFields)
        {
            if(dbService == null)
            {
                throw new ArgumentNullException("dbService");
            }

            var broker = new MsSqlBroker();
            var outputDescription = broker.TestService(dbService);

            if(outputDescription == null || outputDescription.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
            {
                throw new Exception("Error retrieving shape from service output.");
            }

            // Clear out the Recordset.Fields list because the sequence and
            // number of fields may have changed since the last invocation.
            //
            // Create a copy of the Recordset.Fields list before clearing it
            // so that we don't lose the user-defined aliases.
            //
            var rsFields = new List<RecordsetField>(dbService.Recordset.Fields);
            dbService.Recordset.Fields.Clear();

            for(var i = 0; i < outputDescription.DataSourceShapes[0].Paths.Count; i++)
            {
                var path = outputDescription.DataSourceShapes[0].Paths[i];
                if(string.IsNullOrEmpty(path.SampleData))
                {
                    continue;
                }

                // Remove bogus names
                var name = path.DisplayPath.Replace("NewDataSet", "").Replace(".Table.", "");

                #region Remove recordset name if present

                var idx = name.IndexOf("()", StringComparison.InvariantCultureIgnoreCase);
                if(idx >= 0)
                {
                    name = name.Remove(0, idx + 2);
                }

                #endregion

                var field = new RecordsetField { Name = name, Alias = string.IsNullOrEmpty(path.OutputExpression) ? name : path.OutputExpression, Path = path };

                RecordsetField rsField;
                if(!addFields && (rsField = rsFields.FirstOrDefault(f => f.Path.ActualPath == path.ActualPath)) != null)
                {
                    field.Alias = rsField.Alias;
                }
                dbService.Recordset.Fields.Add(field);

                var data = path.SampleData.Split(',');
                for(var recordIndex = 0; recordIndex < data.Length; recordIndex++)
                {
                    dbService.Recordset.SetValue(recordIndex, i, data[recordIndex]);
                }
            }

            return dbService.Recordset;
        }

        #endregion

        #region FetchMethods

        public virtual ServiceMethodList FetchMethods(DbSource dbSource)
        {
            var broker = new MsSqlBroker();
            return broker.GetServiceMethods(dbSource);
        }

        #endregion

        #region DeserializeService

        static Service DeserializeService(string args)
        {
            var service = JsonConvert.DeserializeObject<Service>(args);
            switch(service.ResourceType)
            {
                case ResourceType.DbService:
                    return JsonConvert.DeserializeObject<DbService>(args);

                case ResourceType.Plugin:
                    break;
            }
            return service;
        }

        #endregion
    }
}
