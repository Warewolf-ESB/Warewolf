using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Newtonsoft.Json;
using ServiceStack.Common.Extensions;

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
            string permissions;

            if(values == null)
            {
                throw new InvalidDataException("Empty values passed.");
            }

            values.TryGetValue("Permissions", out permissions);

            if(string.IsNullOrEmpty(permissions))
            {
                throw new InvalidDataException("Empty permissions passed.");
            }

            try
            {
                var windowsGroupPermissions = JsonConvert.DeserializeObject<List<WindowsGroupPermission>>(permissions);

                DoFileEncryption(permissions);

                try
                {
                    var fileInfo = DenyAccessToSecurityFileToEveryone();

                    windowsGroupPermissions.Where(permission => permission.IsServer && permission.Administrator).ForEach(permission => SetFullControlToSecurityConfigFile(permission, fileInfo));
                }
                catch(IdentityNotMappedException inmex)
                {
                    ServerLogger.LogError(inmex);
                    result = "Error writing security configuration: One or more Windows Groups are invalid.";
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError(ex);
                    result = "Error writing security configuration: " + ex.Message;
                }
            }
            catch(Exception e)
            {
                throw new InvalidDataException(string.Format("The permissions passed is not a valid list of permissions. Error: {0}", e.Message));
            }

            return result;
        }

        static void DoFileEncryption(string permissions)
        {
            var byteConverter = new ASCIIEncoding();
            var encryptedData = SecurityEncryption.Encrypt(permissions);
            byte[] dataToEncrypt = byteConverter.GetBytes(encryptedData);
            File.WriteAllBytes(SecurityConfigService.FileName, dataToEncrypt);
        }

        static FileInfo DenyAccessToSecurityFileToEveryone()
        {
            var fileInfo = new FileInfo(SecurityConfigService.FileName);
            // Get a DirectorySecurity object that represents the current security settings.
            var accessControl = fileInfo.GetAccessControl();
            //remove any inherited access
            accessControl.SetAccessRuleProtection(true, false);
            //get any special user access
            var rules = accessControl.GetAccessRules(true, true, typeof(SecurityIdentifier));
            //remove any special access
            foreach(FileSystemAccessRule rule in rules)
            {
                accessControl.RemoveAccessRule(rule);
            }
            fileInfo.SetAccessControl(accessControl);
            return fileInfo;
        }

        void SetFullControlToSecurityConfigFile(WindowsGroupPermission permission, FileInfo fileInfo)
        {
            var accessRule = new FileSystemAccessRule(permission.WindowsGroup,
                                         fileSystemRights: FileSystemRights.FullControl,
                                         inheritanceFlags: InheritanceFlags.None,
                                         propagationFlags: PropagationFlags.InheritOnly,
                                         type: AccessControlType.Allow);

            // Get a DirectorySecurity object that represents the current security settings.
            var accessControl = fileInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            accessControl.AddAccessRule(accessRule);

            // Set the new access settings.
            fileInfo.SetAccessControl(accessControl);
        }

        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Permissions/><Result/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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