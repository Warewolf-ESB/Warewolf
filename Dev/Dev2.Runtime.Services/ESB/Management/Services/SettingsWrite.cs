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
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Checks a users permissions on the local file system
    /// </summary>
    public class SettingsWrite : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage {HasError = false};
            msg.SetMessage("Success");
            if(values == null)
            {
                throw new InvalidDataException("Empty values passed.");
            }
            StringBuilder settingsJson;
            values.TryGetValue("Settings", out settingsJson);
            if(settingsJson ==null || settingsJson.Length == 0)
            {
                throw new InvalidDataException("Error: Unable to parse values.");
            }
           
            try
            {
                var settings = JsonConvert.DeserializeObject<Settings>(settingsJson.ToString());
                var errors = ExecuteService(new SecurityWrite(), "Permissions", settings.Security, theWorkspace);

                if (errors.Length > 0)
                {
                    msg.HasError = true;
                    msg.SetMessage(errors.ToString());
                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                msg.HasError = true;
                msg.SetMessage("Error writing settings configuration.");
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(msg);
        }

        StringBuilder ExecuteService(IEsbManagementEndpoint service, string key, object value, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer= new Dev2JsonSerializer();
            var values = new Dictionary<string, StringBuilder> { { key, serializer.SerializeToBuilder(value)} };
            var result = service.Execute(values, theWorkspace).ToString();
            var msg = serializer.Deserialize<ExecuteMessage>(result);

            if (msg.HasError)
            {
                return msg.Message;
            }

            return new StringBuilder();
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