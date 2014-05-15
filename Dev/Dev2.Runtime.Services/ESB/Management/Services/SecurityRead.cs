using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
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
                    var permissionGroup = currentSecuritySettingsTO.WindowsGroupPermissions;

                    // We need to change BuiltIn\Administrators to -> Warewolf Administrators ;)
                    if(permissionGroup.Count > 0)
                    {
                        var adminGrp = permissionGroup[0].WindowsGroup;
                        if(adminGrp == "BuiltIn\\Administrators")
                        {
                            permissionGroup[0].WindowsGroup = WindowsGroupPermission.BuiltInAdministratorsText;
                            decryptData = JsonConvert.SerializeObject(currentSecuritySettingsTO);
                        }
                    }

                    var hasGuestPermission = permissionGroup.Any(permission => permission.IsBuiltInGuests);
                    if(!hasGuestPermission)
                    {
                        permissionGroup.Add(WindowsGroupPermission.CreateGuests());
                        permissionGroup.Sort(QuickSortForPermissions);
                        decryptData = JsonConvert.SerializeObject(currentSecuritySettingsTO);
                    }
                    return new StringBuilder(decryptData);
                }
                catch(Exception e)
                {
                    ServerLogger.LogError("SecurityRead", e);
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