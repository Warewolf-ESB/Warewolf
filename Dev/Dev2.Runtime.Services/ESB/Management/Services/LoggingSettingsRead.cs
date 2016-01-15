using System;
using System.Collections.Generic;
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
    public class LoggingSettingsRead : IEsbManagementEndpoint
    {


        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                var logLevel = Dev2Logger.GetLogLevel();
                var logMaxSize= Dev2Logger.GetLogMaxSize();
                var loggingSettings = new LoggingSettingsTo {LogLevel = logLevel,LogSize = logMaxSize};
                var serializer = new Dev2JsonSerializer();
                var serializeToBuilder = serializer.SerializeToBuilder(loggingSettings);
                return serializeToBuilder;
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error("LoggingSettingsRead", e);
            }
            return null;
        }


        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
                                 {
                                     Name = HandlesType(),
                                     DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
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
            return "LoggingSettingsReadService";
        }
    }
}