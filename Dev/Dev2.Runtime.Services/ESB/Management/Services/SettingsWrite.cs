
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
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
            var result = new ExecuteMessage { HasError = false, Message = new StringBuilder() };
            try
            {
                var settings = serializer.Deserialize<Settings>(settingsJson.ToString());
                WriteSecuritySettings(theWorkspace, settings, result);
                WriteLoggingSettings(theWorkspace, settings, result);
            }
            catch (Exception ex)
            {
                Dev2Logger.Log.Error("Error writing settings.", ex);
                result.HasError = true;
                result.Message.AppendLine("Error writing settings.");
            }
            return serializer.SerializeToBuilder(result);
        }

        static void WriteLoggingSettings(IWorkspace theWorkspace, Settings settings, ExecuteMessage result)
        {
            try
            {
                ExecuteService(theWorkspace, new LoggingSettingsWrite(), "LoggingSettings", settings.Logging);
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error("Error writing logging configuration.", ex);
                result.HasError = true;
                result.Message.AppendLine("Error writing logging configuration.");
            }
        }

        static void WriteSecuritySettings(IWorkspace theWorkspace, Settings settings, ExecuteMessage result)
        {
            try
            {
                ExecuteService(theWorkspace, new SecurityWrite(), "SecuritySettings", settings.Security);
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error("Error writing settings configuration.", ex);
                result.HasError = true;
                result.Message.AppendLine("Error writing settings configuration.");
            }
        }

        static void ExecuteService(IWorkspace theWorkspace, IEsbManagementEndpoint service, string valuesKey, object valuesValue)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var values = new Dictionary<string, StringBuilder> { { valuesKey, serializer.SerializeToBuilder(valuesValue) } };
            service.Execute(values, theWorkspace);
        }

        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><Settings ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
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
