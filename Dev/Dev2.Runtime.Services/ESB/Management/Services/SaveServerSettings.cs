using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveServerSettings : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Save Resource Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue("ServerSettings", out StringBuilder resourceDefinition);

                var updatedServerSettings = serializer.Deserialize<ServerSettingsData>(resourceDefinition);

                var sourceFilePath = Config.Server.AuditFilePath;

                var auditsFilePath = updatedServerSettings.AuditFilePath;

                if (sourceFilePath != auditsFilePath)
                {
                    var source = Path.Combine(sourceFilePath, "auditDB.db");
                    IFile _file = new FileWrapper();
                    if (_file.Exists(source))
                    {
                        var destination = Path.Combine(auditsFilePath, "auditDB.db");
                        CreateIfNotExists(auditsFilePath);
                        _file.Move(source, destination);
                        Config.Server.AuditFilePath = auditsFilePath;
                        msg.Message = new StringBuilder("Moved");
                    }
                }
                else
                {
                    msg.Message = new StringBuilder();
                }
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

        static void CreateIfNotExists(string path)
        {
            var directoryWrapper = new DirectoryWrapper();
            directoryWrapper.CreateIfNotExists(path);
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ServerSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "SaveServerSettings";
    }
}
