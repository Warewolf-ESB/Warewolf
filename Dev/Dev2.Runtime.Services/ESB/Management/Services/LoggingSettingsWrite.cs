using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

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
                throw new InvalidDataException(ErrorResource.EmptyValuesPassed);
            }

            values.TryGetValue("LoggingSettings", out StringBuilder loggingSettingsBuilder);

            if (loggingSettingsBuilder == null || loggingSettingsBuilder.Length == 0)
            {
                throw new InvalidDataException(ErrorResource.EmptyLoggingSettingsPassed);
            }

            var serializer = new Dev2JsonSerializer();

            try
            {
                var loggingSettingsTo = serializer.Deserialize<LoggingSettingsTo>(loggingSettingsBuilder);
                if(loggingSettingsTo == null)
                {
                    throw new InvalidDataException(ErrorResource.InvalidSecuritySettings);
                }

                Write(loggingSettingsTo);
            }
            catch(Exception e)
            {
                throw new InvalidDataException(ErrorResource.InvalidSecuritySettings + $" Error: {e.Message}");
            }

            var msg = new ExecuteMessage { HasError = false };
            msg.SetMessage("Success");

            return serializer.SerializeToBuilder(msg);
        }

        public static void Write(LoggingSettingsTo loggingSettingsTo)
        {
            VerifyArgument.IsNotNull("loggingSettingsTo", loggingSettingsTo);
            Dev2Logger.WriteLogSettings(loggingSettingsTo.FileLoggerLogSize.ToString(CultureInfo.InvariantCulture), loggingSettingsTo.FileLoggerLogLevel,loggingSettingsTo.EventLogLoggerLogLevel, EnvironmentVariables.ServerLogSettingsFile,"Warewolf Server");
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
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }
    }
}