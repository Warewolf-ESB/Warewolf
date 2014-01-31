using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Checks a users permissions on the local file system
    /// </summary>
    public class SecurityRead : IEsbManagementEndpoint
    {
        readonly TimeSpan _cacheTimeout = new TimeSpan(1, 0, 0);

        public static List<WindowsGroupPermission> DefaultPermissions
        {
            get
            {
                return new List<WindowsGroupPermission>
                {
                   WindowsGroupPermission.CreateAdministrators(),
                   //new WindowsGroupPermission { IsServer = true, WindowsGroup = WindowsGroupPermission.BuiltInAdministratorsText, Permissions = Permissions.View | Permissions.Execute }
                };
            }
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if(File.Exists(ServerSecurityService.FileName))
            {
                string encryptedData;
                using(var inStream = new FileStream(ServerSecurityService.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using(var reader = new StreamReader(inStream))
                    {
                        encryptedData = reader.ReadToEnd();
                    }
                }
                try
                {
                    var decryptData = SecurityEncryption.Decrypt(encryptedData);
                    return new StringBuilder(decryptData);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {

                }
            }

            var serializer = new Dev2JsonSerializer();
            var securitySettingsTO = new SecuritySettingsTO(DefaultPermissions) { CacheTimeout = _cacheTimeout };
            return serializer.SerializeToBuilder(securitySettingsTO);
        }

        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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
            return "SecurityReadService";
        }
    }
}