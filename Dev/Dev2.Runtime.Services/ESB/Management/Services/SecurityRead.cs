#pragma warning disable
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
using Warewolf;
using Warewolf.Data;


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

        public static INamedGuid DefaultOverrideResouce => new NamedGuid();

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2Logger.Debug("Start Security Read", GlobalConstants.WarewolfDebug);
            var serverSecuritySettingsFile = EnvironmentVariables.ServerSecuritySettingsFile;
            if (File.Exists(serverSecuritySettingsFile))
            {
                string encryptedData;
                using (var inStream = new FileStream(serverSecuritySettingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(inStream))
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
            var securitySettingsTo = new SecuritySettingsTO(DefaultPermissions, DefaultOverrideResouce) {CacheTimeout = _cacheTimeout};
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

            if (px == null || py == null)
            {
                return 1;
            }

            // New items must be last
            //
            if (px.IsNew)
            {
                // px is greater than py
                return int.MaxValue;
            }

            if (py.IsNew)
            {
                // px is less than py
                return int.MinValue;
            }

            // BuiltInAdministrators must be first
            if (px.IsBuiltInAdministrators)
            {
                // px is less than py
                return int.MinValue;
            }

            if (py.IsBuiltInAdministrators)
            {
                // px is greater than py
                return int.MaxValue;
            }

            // IsBuiltInGuests must be second
            if (px.IsBuiltInGuests)
            {
                // px is less than py
                return int.MinValue + 1;
            }

            if (py.IsBuiltInGuests)
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