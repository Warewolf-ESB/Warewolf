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
using System.Globalization;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class LoggingSettingsWrite : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
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

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><LoggingSettings ColumnIODirection=\"Input\"></LoggingSettings><Result/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "LoggingSettingsWriteService";
    }
}