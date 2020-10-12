/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Text;
using Warewolf.Configuration;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SavePersistenceSettings: IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Save Persistence Settings Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue(Warewolf.Service.SavePersistenceSettings.PersistenceSettings, out StringBuilder settings);

                var updatedPersistenceSettings = serializer.Deserialize<PersistenceSettingsData>(settings);
                Config.Persistence.PersistenceDataSource = updatedPersistenceSettings.PersistenceDataSource;
                Config.Persistence.EncryptDataSource = updatedPersistenceSettings.EncryptDataSource;
                Config.Persistence.Enable = updatedPersistenceSettings.Enable;
                Config.Persistence.PrepareSchemaIfNecessary = updatedPersistenceSettings.PrepareSchemaIfNecessary;
                Config.Persistence.PersistenceScheduler = updatedPersistenceSettings.PersistenceScheduler;
                Config.Persistence.DashboardEndpoint = updatedPersistenceSettings.DashboardEndpoint;
                Config.Persistence.DashboardName = updatedPersistenceSettings.DashboardName;
                Config.Persistence.DashboardPort = updatedPersistenceSettings.DashboardPort;
                Config.Persistence.ServerName = updatedPersistenceSettings.ServerName;

                msg.Message = new StringBuilder();
                msg.HasError = false;
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ServerSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;
        public string HandlesType() => nameof(Warewolf.Service.SavePersistenceSettings);
    }
}