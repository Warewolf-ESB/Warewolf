/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Security.Cryptography;
using Dev2.Common;
using Newtonsoft.Json;
using Warewolf;
using Warewolf.Data;
using Warewolf.Services;

namespace Dev2.Services.Security
{
    public interface ISecuritySettings
    {
        SecuritySettingsTO ReadSettingsFile(IResourceNameProvider resourceNameProvider);
    }
    public class SecuritySettings : ISecuritySettings
    {
        public SecuritySettings()
        {

        }

        private readonly TimeSpan CacheTimeout = new TimeSpan(1, 0, 0);

        public static List<WindowsGroupPermission> DefaultPermissions => new List<WindowsGroupPermission>
        {
            WindowsGroupPermission.CreateAdministrators(),
            WindowsGroupPermission.CreateGuests()
        };

        public static INamedGuid DefaultOverrideResource => new NamedGuid();

        public static string DefaultSecretKey => "";

        public SecuritySettingsTO ReadSettingsFile(IResourceNameProvider resourceNameProvider)
        {
            var serverSecuritySettingsFile = EnvironmentVariables.ServerSecuritySettingsFile;
            if (!File.Exists(serverSecuritySettingsFile))
            {
                return new SecuritySettingsTO(DefaultPermissions, DefaultOverrideResource, DefaultSecretKey) {CacheTimeout = CacheTimeout};
            }

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
                return ProcessSettingsFile(resourceNameProvider, encryptedData);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("SecurityRead", e, GlobalConstants.WarewolfError);
                return new SecuritySettingsTO(DefaultPermissions, DefaultOverrideResource, DefaultSecretKey) {CacheTimeout = CacheTimeout};
            }
        }

        static SecuritySettingsTO ProcessSettingsFile(IResourceNameProvider resourceNameProvider, string encryptedData)
        {
            var decryptData = SecurityEncryption.Decrypt(encryptedData);
            Dev2Logger.Debug(decryptData, GlobalConstants.WarewolfDebug);

            var currentSecuritySettingsTo = JsonConvert.DeserializeObject<SecuritySettingsTO>(decryptData);
            if (currentSecuritySettingsTo.WindowsGroupPermissions.Any(a => a.ResourceID != Guid.Empty))
            {
                foreach (var perm in currentSecuritySettingsTo.WindowsGroupPermissions.Where(a => a.ResourceID != Guid.Empty))
                {
                    perm.ResourceName = resourceNameProvider.GetResourceNameById(perm.ResourceID);
                }
            }

            if (currentSecuritySettingsTo.AuthenticationOverrideWorkflow.Name.Length > 0 && currentSecuritySettingsTo.SecretKey == "")
            {
                var hmac = new HMACSHA256();
                currentSecuritySettingsTo.SecretKey = Convert.ToBase64String(hmac.Key);


            }

            var permissionGroup = currentSecuritySettingsTo.WindowsGroupPermissions;
            if (permissionGroup.Count > 0)
            {
                var adminGrp = permissionGroup[0].WindowsGroup;
                if (adminGrp == "BuiltIn\\Administrators")
                {
                    permissionGroup[0].WindowsGroup = WindowsGroupPermission.BuiltInAdministratorsText;
                }
            }

            var hasGuestPermission = permissionGroup.Any(permission => permission.IsBuiltInGuests);

            var hasAdminPermission = permissionGroup.Any(permission => permission.IsBuiltInAdministrators);
            if (!hasAdminPermission)
            {
                permissionGroup.Add(WindowsGroupPermission.CreateAdministrators());
                permissionGroup.Sort(QuickSortForPermissions);
            }

            if (!hasGuestPermission)
            {
                permissionGroup.Add(WindowsGroupPermission.CreateGuests());
                permissionGroup.Sort(QuickSortForPermissions);
            }

            return currentSecuritySettingsTo;
        }

        static int QuickSortForPermissions(WindowsGroupPermission x, WindowsGroupPermission y)
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
    }
}
