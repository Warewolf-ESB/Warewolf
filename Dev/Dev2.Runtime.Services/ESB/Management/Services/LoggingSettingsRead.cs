#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class LoggingSettingsRead : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                var fileLogLevel = Dev2Logger.GetFileLogLevel();
                var eventLogLevel = Dev2Logger.GetEventLogLevel();
                var logMaxSize= Dev2Logger.GetLogMaxSize();

                var loggingSettings = new LoggingSettingsTo
                {
                    FileLoggerLogLevel = fileLogLevel,
                    EventLogLoggerLogLevel = eventLogLevel,
                    FileLoggerLogSize = logMaxSize
                };

                var serializer = new Dev2JsonSerializer();
                var serializeToBuilder = serializer.SerializeToBuilder(loggingSettings);
                return serializeToBuilder;
            }
            catch (Exception e)
            {
                Dev2Logger.Error("LoggingSettingsRead", e, GlobalConstants.WarewolfError);
            }
            return null;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "LoggingSettingsReadService";
    }
}