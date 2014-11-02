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
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Data.Settings;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    ///     Checks a users permissions on the local file system
    /// </summary>
    public class SettingsRead : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var settings = new Settings();
            try
            {
                IEsbManagementEndpoint securityRead = CreateSecurityReadEndPoint();
                StringBuilder jsonPermissions = securityRead.Execute(values, theWorkspace);
                settings.Security = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());
            }
            catch (Exception ex)
            {
                Dev2Logger.Log.Error(ex);
                settings.HasError = true;
                settings.Error = "Error reading settings configuration : " + ex.Message;
                settings.Security = new SecuritySettingsTO(SecurityRead.DefaultPermissions);
            }

            var serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(settings);
        }

        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification =
                    new StringBuilder(
                        "<DataList><Settings ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
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
            return "SettingsReadService";
        }

        protected virtual IEsbManagementEndpoint CreateSecurityReadEndPoint()
        {
            return new SecurityRead();
        }
    }
}