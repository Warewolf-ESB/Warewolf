using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Security;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Checks a users permissions on the local file system
    /// </summary>
    public class SecurityWrite : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            var result = "Success";
            var byteConverter = new ASCIIEncoding();
            var hostSecureConfig = new HostSecureConfig();
            var jsonData = JsonConvert.SerializeObject(new object());
            byte[] dataToEncrypt = byteConverter.GetBytes(jsonData);
            var encryptedData = hostSecureConfig.ServerKey.Encrypt(dataToEncrypt,false);
            File.WriteAllBytes("secure.config", encryptedData);
            try
            {
                var accessRule = new FileSystemAccessRule("",
                                     fileSystemRights: FileSystemRights.Modify,
                                     inheritanceFlags: InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                     propagationFlags: PropagationFlags.InheritOnly,
                                     type: AccessControlType.Allow);

                var fileInfo = new FileInfo("secure.config");

                // Get a DirectorySecurity object that represents the current security settings.
                var accessControl = fileInfo.GetAccessControl();

                // Add the FileSystemAccessRule to the security settings. 
                accessControl.AddAccessRule(accessRule);

                // Set the new access settings.
                fileInfo.SetAccessControl(accessControl);
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                result = "Error writing security configuration.";
            }

            return result;
        }

        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Security/><Result/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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
            return "SecurityWriteService";
        }
    }
}