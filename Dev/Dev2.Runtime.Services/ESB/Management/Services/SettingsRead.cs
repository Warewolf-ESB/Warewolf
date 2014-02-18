using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Data.Settings;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Checks a users permissions on the local file system
    /// </summary>
    public class SettingsRead : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var settings = new Settings();
            try
            {
                var securityRead = CreateSecurityReadEndPoint();
                var jsonPermissions = securityRead.Execute(values, theWorkspace);
                settings.Security = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                settings.HasError = true;
                settings.Error = "Error reading settings configuration : " + ex.Message;
                settings.Security = new SecuritySettingsTO(SecurityRead.DefaultPermissions);
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(settings);
        }

        protected virtual IEsbManagementEndpoint CreateSecurityReadEndPoint()
        {
            return new SecurityRead();
        }

        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Settings ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
            };

            var serviceAction = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            dynamicService.Actions.Add(serviceAction);

            return dynamicService;
        }

        public string HandlesType()
        {
            return "SettingsReadService";
        }
    }
}