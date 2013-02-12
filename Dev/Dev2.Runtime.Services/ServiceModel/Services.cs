using System;
using System.Xml.Linq;
using Dev2.DynamicServices;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
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
                //TODO
                //1. Hydrate source string into a source data model
                //2. Get a list of actions for that source
                //3. Create instances for ServiceAction for each action
                //4. Return the JSON representation of the service actions

                var service = JsonConvert.DeserializeObject<Service>(args);
                switch(service.ResourceType)
                {
                    case enSourceType.SqlDatabase:
                    case enSourceType.MySqlDatabase:
                        service = JsonConvert.DeserializeObject<DbService>(args);
                        break;
                    case enSourceType.Plugin:
                        break;
                }

                var random = new Random();
                for(var i = 0; i < 50; i++)
                {
                    var method = new ServiceMethod { Name = string.Format("dbo.Pr_GetCake_{0:00}", i) };
                    for(var j = 0; j < 10; j++)
                    {
                        var varLength = j % 4 == 0 ? 30 : 15;
                        method.Parameters.Add(new MethodParameter { Name = random.GenerateString(varLength, "@") });
                    }
                    result.Add(method);
                }
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
            var recordset = new Recordset();
            try
            {
                var service = JsonConvert.DeserializeObject<Service>(args);
                switch(service.ResourceType)
                {
                    case enSourceType.SqlDatabase:
                    case enSourceType.MySqlDatabase:
                        service = JsonConvert.DeserializeObject<DbService>(args);
                        break;
                    case enSourceType.Plugin:
                        break;
                }

                // TODO: Implement real stuff
                const int ColCount = 30;
                if(string.IsNullOrEmpty(service.MethodRecordset.Name))
                {
                    recordset.Name = service.MethodName;
                }

                if(service.MethodRecordset.Fields.Count == 0)
                {
                    for(var j = 0; j < ColCount; j++)
                    {
                        recordset.Fields.Add("Column" + (j + 1));
                    }
                }
                var random = new Random();
                for(var i = 0; i < 15; i++)
                {
                    recordset.Records.Add(recordset, fieldIndex => random.GenerateString(30, string.Empty, true));
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                recordset.HasErrors = true;
                recordset.ErrorMessage = ex.Message;
            }

            return recordset;
        }

        #endregion

        #region Save

        // POST: Service/Services/Save
        public string Save(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var service = JsonConvert.DeserializeObject<Service>(args);
                switch(service.ResourceType)
                {
                    case enSourceType.SqlDatabase:
                    case enSourceType.MySqlDatabase:
                        service = JsonConvert.DeserializeObject<DbService>(args);
                        break;
                    case enSourceType.Plugin:
                        break;
                }

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


    }
}
