/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.Data.Settings;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SettingsRead : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            var settings = new Settings();
            try
            {
                var securityRead = CreateSecurityReadEndPoint();
                var loggingSettingsRead = CreateLoggingSettingsReadEndPoint();
                var jsonPermissions = securityRead.Execute(values, theWorkspace);
                var jsonLoggingSettings = loggingSettingsRead.Execute(values, theWorkspace);
                var permissionsRead = CreatePerfCounterReadEndPoint();
                var perfsettings = permissionsRead.Execute(values, theWorkspace);

                settings.Security = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());
                settings.Logging = JsonConvert.DeserializeObject<LoggingSettingsTo>(jsonLoggingSettings.ToString());
                settings.PerfCounters = serializer.Deserialize<IPerformanceCounterTo>(perfsettings.ToString());
            }
            catch(Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                settings.HasError = true;
                settings.Error = ErrorResource.ErrorReadingSettingsConfiguration + ex.Message;
                settings.Security = new SecuritySettingsTO(SecurityRead.DefaultPermissions);
            }

          
            return serializer.SerializeToBuilder(settings);
        }

        protected virtual IEsbManagementEndpoint CreatePerfCounterReadEndPoint()
        {
            return new FetchPerformanceCounters();
        }

        protected virtual IEsbManagementEndpoint CreateSecurityReadEndPoint()
        {
            return new SecurityRead();
        }
        
        protected virtual IEsbManagementEndpoint CreateLoggingSettingsReadEndPoint()
        {
            return new LoggingSettingsRead();
        }

        public override DynamicService CreateServiceEntry()
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

        public override string HandlesType()
        {
            return "SettingsReadService";
        }
    }
}
