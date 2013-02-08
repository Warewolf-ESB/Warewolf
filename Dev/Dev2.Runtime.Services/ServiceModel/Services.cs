using System;
using System.Collections.Generic;
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

        public Service Get(string args, Guid workspaceID, Guid dataListID)
        {
            dynamic argsObj = JObject.Parse(args);
            enSourceType resourceType = ParseResourceType(argsObj);

            var result = new Service { ResourceID = Guid.Empty, ResourceType = resourceType };
            try
            {
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

        #region Sources

        public string Sources(string args, Guid workspaceID, Guid dataListID)
        {
            dynamic argsObj = JObject.Parse(args);

            try
            {
                var resources = Resources.Read(workspaceID, ParseResourceType(argsObj));
                return JsonConvert.SerializeObject(resources);
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message }.ToString();
            }
        }

        #endregion

        #region Actions

        public string Actions(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                //TODO
                //1. Hydrate source string into a source data model
                //2. Get a list of actions for that source
                //3. Create instances for ServiceAction for each action
                //4. Return the JSON representation of the service actions

                var service = JsonConvert.DeserializeObject<Service>(args);
                var actions = new List<ServiceActionWrapper>
                {
                    new ServiceActionWrapper { Name = "Action1" },
                    new ServiceActionWrapper { Name = "Action2" },
                    new ServiceActionWrapper { Name = "Action3" }
                };

                return JsonConvert.SerializeObject(actions);
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message }.ToString();
            }
        }

        #endregion

        #region Save

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

        #region ParseResourceType

        static enSourceType ParseResourceType(dynamic jsonObj)
        {
            enSourceType resourceType;
            if(!Enum.TryParse(jsonObj.resourceType.Value, out resourceType))
            {
                resourceType = enSourceType.SqlDatabase;
            }
            return resourceType;
        }

        #endregion

    }
}
