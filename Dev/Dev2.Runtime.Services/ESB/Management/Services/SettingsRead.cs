using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Data.Settings;
using Dev2.DynamicServices;
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
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            var settings = new Settings();
            try
            {
                var securityRead = new SecurityRead();
                var jsonPermissions = securityRead.Execute(values, theWorkspace);
                settings.Security = JsonConvert.DeserializeObject<List<WindowsGroupPermission>>(jsonPermissions);

            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                settings.HasError = true;
                settings.Error = "Error reading settings configuration : " + ex.Message;
            }

            return settings.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Settings/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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