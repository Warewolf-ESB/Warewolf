using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Checks a users permissions on the local file system
    /// </summary>
    public class LoggingSettingsWrite : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if(values == null)
            {
                throw new InvalidDataException("Empty values passed.");
            }

            StringBuilder loggingSettingsBuilder;
            values.TryGetValue("LoggingSettings", out loggingSettingsBuilder);

            if(loggingSettingsBuilder == null || loggingSettingsBuilder.Length == 0)
            {
                throw new InvalidDataException("Empty Logging Settings passed.");
            }

            var serializer = new Dev2JsonSerializer();

            try
            {
                var loggingSettingsTo = serializer.Deserialize<LoggingSettingsTo>(loggingSettingsBuilder);
                if(loggingSettingsTo == null)
                {
                    throw new InvalidDataException("The security settings are not valid.");
                }

                Write(loggingSettingsTo);
            }
            catch(Exception e)
            {
                throw new InvalidDataException(string.Format("The security settings are not valid. Error: {0}", e.Message));
            }

            var msg = new ExecuteMessage { HasError = false };
            msg.SetMessage("Success");

            return serializer.SerializeToBuilder(msg);
        }

        public static void Write(LoggingSettingsTo loggingSettingsTo)
        {
            VerifyArgument.IsNotNull("loggingSettingsTo", loggingSettingsTo);
            Dev2Logger.WriteLogSettings(loggingSettingsTo.LogSize.ToString(CultureInfo.InvariantCulture), loggingSettingsTo.LogLevel, "Settings.config");
        }

        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><LoggingSettings ColumnIODirection=\"Input\"></LoggingSettings><Result/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
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
            return "LoggingSettingsWriteService";
        }
    }
}