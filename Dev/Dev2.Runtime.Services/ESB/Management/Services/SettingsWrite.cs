using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Data.Settings;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SettingsWrite : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if(values == null)
            {
                throw new InvalidDataException("Empty values passed.");
            }

            StringBuilder settingsJson;
            values.TryGetValue("Settings", out settingsJson);
            if(settingsJson == null || settingsJson.Length == 0)
            {
                throw new InvalidDataException("Error: Unable to parse values.");
            }

            var serializer = new Dev2JsonSerializer();
            ExecuteMessage result;
            try
            {
                var settings = serializer.Deserialize<Settings>(settingsJson.ToString());

                result = ExecuteService(theWorkspace, new SecurityWrite(), "SecuritySettings", settings.Security);
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                result = new ExecuteMessage { HasError = true };
                result.SetMessage("Error writing settings configuration.");
            }

            return serializer.SerializeToBuilder(result);
        }

        static ExecuteMessage ExecuteService(IWorkspace theWorkspace, IEsbManagementEndpoint service, string valuesKey, object valuesValue)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var values = new Dictionary<string, StringBuilder> { { valuesKey, serializer.SerializeToBuilder(valuesValue) } };
            var result = service.Execute(values, theWorkspace).ToString();
            return serializer.Deserialize<ExecuteMessage>(result);
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
            return "SettingsWriteService";
        }
    }
}