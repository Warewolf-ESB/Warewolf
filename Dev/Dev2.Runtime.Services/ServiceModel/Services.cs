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
            var result = new Service { ResourceID = Guid.Empty, ResourceType = enSourceType.SqlDatabase };
            try
            {
                dynamic argsObj = JObject.Parse(args);

                var resourceType = Resources.ParseResourceType(argsObj.resourceType.Value);
                var xmlStr = Resources.ReadXml(workspaceID, resourceType, argsObj.resourceID.Value);
                if(!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    result = new Service(xml);
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
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
                for(var i = 0; i < 50; i++)
                {
                    var method = new ServiceMethod { Name = string.Format("dbo.Pr_GetCake_{0:00}", i) };
                    for(var j = 0; j < 10; j++)
                    {
                        method.Parameters.Add(new MethodParameter { Name = string.Format("Param_{0:00}", j) });
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

        #region Save

        // POST: Service/Services/Save
        public string Save(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var service = JsonConvert.DeserializeObject<Service>(args);
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
