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
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Data.Settings;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SettingsWrite : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if(values == null)
            {
                throw new InvalidDataException(ErrorResource.EmptyValuesPassed);
            }

            values.TryGetValue("Settings", out StringBuilder settingsJson);
            if (settingsJson == null || settingsJson.Length == 0)
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
                WritePerfCounterSettings(theWorkspace, settings, result);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ErrorResource.ErrorWritingSettings, ex, GlobalConstants.WarewolfError);
                result.HasError = true;
                result.Message.AppendLine(ErrorResource.ErrorWritingSettings);
            }
            return serializer.SerializeToBuilder(result);
        }

        static void WriteLoggingSettings(IWorkspace theWorkspace, Settings settings, ExecuteMessage result)
        {
            try
            {
                if(settings.Logging != null)
                {
                    var executionResult = ExecuteService(theWorkspace, new LoggingSettingsWrite(), "LoggingSettings", settings.Logging);
                    result.Message.AppendLine(executionResult);
                }
            }
            catch(Exception ex)
            {
                Dev2Logger.Error(ErrorResource.ErrorWritingLoggingConfiguration, ex, GlobalConstants.WarewolfError);
                result.HasError = true;
                result.Message.AppendLine(ErrorResource.ErrorWritingLoggingConfiguration);
            }
        }

        static void WritePerfCounterSettings(IWorkspace theWorkspace, Settings settings, ExecuteMessage result)
        {
            try
            {
                if (settings.Logging != null)
                {
                    var executionResult = ExecuteService(theWorkspace, new SavePerformanceCounters(), "PerformanceCounterTo", settings.PerfCounters);
                    result.Message.AppendLine(executionResult);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ErrorResource.ErrorWritingLoggingConfiguration, ex, GlobalConstants.WarewolfError);
                result.HasError = true;
                result.Message.AppendLine(ErrorResource.ErrorWritingLoggingConfiguration);
            }
        }

        static void WriteSecuritySettings(IWorkspace theWorkspace, Settings settings, ExecuteMessage result)
        {
            try
            {
                if(settings.Security != null)
                {
                    var executionResult = ExecuteService(theWorkspace, new SecurityWrite(), "SecuritySettings", settings.Security);
                    result.Message.AppendLine(executionResult);
                }
            }
            catch(Exception ex)
            {
                Dev2Logger.Error(ErrorResource.ErrorWritingSettingsConfiguration, ex, GlobalConstants.WarewolfError);
                result.HasError = true;
                result.Message.AppendLine(ErrorResource.ErrorWritingSettingsConfiguration);
            }
        }

        static string ExecuteService(IWorkspace theWorkspace, IEsbManagementEndpoint service, string valuesKey, object valuesValue)
        {
            var serializer = new Dev2JsonSerializer();
            var values = new Dictionary<string, StringBuilder> { { valuesKey, serializer.SerializeToBuilder(valuesValue) } };
            return service.Execute(values, theWorkspace).ToString();
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Settings ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "SettingsWriteService";
    }
}
