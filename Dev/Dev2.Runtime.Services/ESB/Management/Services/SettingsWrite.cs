using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Data.Settings;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Checks a users permissions on the local file system
    /// </summary>
    public class SettingsWrite : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            var result = "Success";
            if(values == null)
            {
                throw new InvalidDataException("Empty values passed.");
            }
            string settingsJson;
            values.TryGetValue("Settings", out settingsJson);
            if(string.IsNullOrEmpty(settingsJson))
            {
                throw new InvalidDataException("Error: Unable to parse values.");
            }
            var errors = new StringBuilder();
            try
            {
                var settings = JsonConvert.DeserializeObject<Settings>(settingsJson);
                ExecuteService(new SecurityWrite(), "Permissions", settings.Security, theWorkspace, errors);
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                errors.AppendLine("Error writing settings configuration.");
            }

            if(errors.Length > 0)
            {
                result = errors.ToString();
            }
            return result;
        }

        static void ExecuteService(IEsbManagementEndpoint service, string key, object value, IWorkspace theWorkspace, StringBuilder errors)
        {
            var values = new Dictionary<string, string> { { key, JsonConvert.SerializeObject(value) } };
            var result = service.Execute(values, theWorkspace);
            if(result.ToLowerInvariant() != "success")
            {
                errors.AppendLine(result);
            }
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
            return "SettingsWriteService";
        }
    }
}