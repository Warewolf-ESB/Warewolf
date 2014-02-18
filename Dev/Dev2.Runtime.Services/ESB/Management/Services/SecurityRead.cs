using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Newtonsoft.Json;

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
                   WindowsGroupPermission.CreateGuests()
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
                    var currentSecuritySettingsTO = JsonConvert.DeserializeObject<SecuritySettingsTO>(decryptData);
                    var hasGuestPermission = currentSecuritySettingsTO.WindowsGroupPermissions.Any(permission => permission.IsBuiltInGuests);
                    if(!hasGuestPermission)
                    {
                        currentSecuritySettingsTO.WindowsGroupPermissions.Add(WindowsGroupPermission.CreateGuests());
                        currentSecuritySettingsTO.WindowsGroupPermissions.Sort(QuickSortForPermissions);
                        decryptData = JsonConvert.SerializeObject(currentSecuritySettingsTO);
                    }
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

        int QuickSortForPermissions(WindowsGroupPermission x, WindowsGroupPermission y)
        {
            var px = x;
            var py = y;

            if(px == null || py == null)
            {
                return 1;
            }

            // New items must be last
            //
            if(px.IsNew)
            {
                // px is greater than py
                return int.MaxValue;
            }
            if(py.IsNew)
            {
                // px is less than py
                return int.MinValue;
            }

            // BuiltInAdministrators must be first
            if(px.IsBuiltInAdministrators)
            {
                // px is less than py
                return int.MinValue;
            }
            if(py.IsBuiltInAdministrators)
            {
                // px is greater than py
                return int.MaxValue;
            }
            // IsBuiltInGuests must be second
            if(px.IsBuiltInGuests)
            {
                // px is less than py
                return int.MinValue + 1;
            }
            if(py.IsBuiltInGuests)
            {
                // px is greater than py
                return int.MaxValue - 1;
            }

            return 1;
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