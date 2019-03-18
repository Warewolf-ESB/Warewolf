#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Newtonsoft.Json;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class SecurityRead : DefaultEsbManagementEndpoint
    {
        readonly TimeSpan _cacheTimeout = new TimeSpan(1, 0, 0);
        IResourceCatalog _resourceCatalog;

        public static List<WindowsGroupPermission> DefaultPermissions => new List<WindowsGroupPermission>
        {
            WindowsGroupPermission.CreateAdministrators(),
            WindowsGroupPermission.CreateGuests()
        };

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2Logger.Debug("Start Security Read", GlobalConstants.WarewolfDebug);
            var serverSecuritySettingsFile = EnvironmentVariables.ServerSecuritySettingsFile;
            if(File.Exists(serverSecuritySettingsFile))
            {
                string encryptedData;
                using (var inStream = new FileStream(serverSecuritySettingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using(var reader = new StreamReader(inStream))
                    {
                        encryptedData = reader.ReadToEnd();
                    }
                }
                Dev2Logger.Debug("Security Data Read", GlobalConstants.WarewolfDebug);
                try
                {
                    return Execute(encryptedData);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error("SecurityRead", e, GlobalConstants.WarewolfError);
                }
            }

            var serializer = new Dev2JsonSerializer();
            var securitySettingsTo = new SecuritySettingsTO(DefaultPermissions) { CacheTimeout = _cacheTimeout };
            return serializer.SerializeToBuilder(securitySettingsTo);
        }

        StringBuilder Execute(string encryptedData)
        {
            var decryptData = SecurityEncryption.Decrypt(encryptedData);
            Dev2Logger.Debug(decryptData, GlobalConstants.WarewolfDebug);
            var currentSecuritySettingsTo = JsonConvert.DeserializeObject<SecuritySettingsTO>(decryptData);
            if (currentSecuritySettingsTo.WindowsGroupPermissions.Any(a => a.ResourceID != Guid.Empty))
            {
                foreach (var perm in currentSecuritySettingsTo.WindowsGroupPermissions.Where(a => a.ResourceID != Guid.Empty))
                {
                    perm.ResourceName = Catalog.GetResourcePath(GlobalConstants.ServerWorkspaceID, perm.ResourceID);
                }
            }
            decryptData = JsonConvert.SerializeObject(currentSecuritySettingsTo);
            var permissionGroup = currentSecuritySettingsTo.WindowsGroupPermissions;

            // We need to change BuiltIn\Administrators to -> Warewolf Administrators ;)
            if (permissionGroup.Count > 0)
            {
                var adminGrp = permissionGroup[0].WindowsGroup;
                if (adminGrp == "BuiltIn\\Administrators")
                {
                    permissionGroup[0].WindowsGroup = WindowsGroupPermission.BuiltInAdministratorsText;
                    decryptData = JsonConvert.SerializeObject(currentSecuritySettingsTo);
                }
            }

            var hasGuestPermission = permissionGroup.Any(permission => permission.IsBuiltInGuests);
            var hasAdminPermission = permissionGroup.Any(permission => permission.IsBuiltInAdministrators);
            if (!hasAdminPermission)
            {
                permissionGroup.Add(WindowsGroupPermission.CreateAdministrators());
                permissionGroup.Sort(QuickSortForPermissions);
                decryptData = JsonConvert.SerializeObject(currentSecuritySettingsTo);
            }
            if (!hasGuestPermission)
            {
                permissionGroup.Add(WindowsGroupPermission.CreateGuests());
                permissionGroup.Sort(QuickSortForPermissions);
                decryptData = JsonConvert.SerializeObject(currentSecuritySettingsTo);
            }
            return new StringBuilder(decryptData);
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

        public IResourceCatalog Catalog
        {
            get => _resourceCatalog ?? ResourceCatalog.Instance;
            set => _resourceCatalog = value;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "SecurityReadService";
    }
}
