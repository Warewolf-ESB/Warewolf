/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
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
    public class SaveAuditingSettings : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Save Auditing Settings Service", GlobalConstants.WarewolfInfo);
                values.TryGetValue(Warewolf.Service.SaveAuditingSettings.LegacySettings, out StringBuilder legacySettings);
                values.TryGetValue(Warewolf.Service.SaveAuditingSettings.AuditingSettings, out StringBuilder auditSettings);
                values.TryGetValue(Warewolf.Service.SaveAuditingSettings.SinkType, out StringBuilder sinkType);

                switch (sinkType.ToString())
                {
                    case nameof(AuditingSettingsData):
                        var updatedAuditingSettings = serializer.Deserialize<AuditingSettingsData>(auditSettings);
                        Config.Auditing.LoggingDataSource = updatedAuditingSettings.LoggingDataSource;
                        Config.Auditing.EncryptDataSource = updatedAuditingSettings.EncryptDataSource;
                        Config.Auditing.IncludeEnvironmentVariable = updatedAuditingSettings.IncludeEnvironmentVariable;
                        Config.Auditing.Save();

                        Config.Server.Sink = nameof(AuditingSettingsData);
                        Config.Server.IncludeEnvironmentVariable = updatedAuditingSettings.IncludeEnvironmentVariable;
                        Config.Server.Save();
                        break;

                    case nameof(LegacySettingsData):
                        var updatedLegacySettings = serializer.Deserialize<LegacySettingsData>(legacySettings);
                        Config.Legacy.AuditFilePath = updatedLegacySettings.AuditFilePath;
                        Config.Legacy.IncludeEnvironmentVariable = updatedLegacySettings.IncludeEnvironmentVariable;                       
                        Config.Legacy.AuditLogMaxSize = updatedLegacySettings.AuditLogMaxSize;
                        Config.Legacy.Save();

                        Config.Server.Sink = nameof(LegacySettingsData);
                        Config.Server.IncludeEnvironmentVariable = updatedLegacySettings.IncludeEnvironmentVariable;                         
                        Config.Server.Save();
                        break;

                    default:
                        var err = $"SinkType: { sinkType } unknown";
                        msg.HasError = true;
                        msg.Message = new StringBuilder(err);
                        Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                        break;
                }
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ServerSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => nameof(Warewolf.Service.SaveAuditingSettings);
    }
}