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

using Dev2.Common.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using System;
using System.IO;
using System.Windows;

namespace Dev2.Utils
{
    public static class HelperUtils
    {
       
        public static string GetStudioLogSettingsConfigFile()
        {
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var studioFolder = Path.Combine(localAppDataFolder, "Warewolf", "Studio");

            IDirectoryHelper directoryHelper = new DirectoryHelper();
            directoryHelper.CreateIfNotExists(studioFolder);
            
            var settingsConfigFile = Path.Combine(studioFolder, "Settings.config");
            return settingsConfigFile;
        }
        public static void ShowTrustRelationshipError(IPopupController popupController, SystemException exception)
        {
            if (exception.Message.Contains("The trust relationship between this workstation and the primary domain failed"))
            {
                var popup = popupController;
                popup.Header = "Error connecting to server";
                popup.Description = "This computer cannot contact the Domain Controller."
                                    + Environment.NewLine + "If it does not belong to a domain, please ensure it is removed from the domain in computer management.";
                popup.Buttons = MessageBoxButton.OK;
                popup.ImageType = MessageBoxImage.Error;
                popup.Show();
            }
        }

        public static string SanitizePath(string path, string resourceName)
        {
            var newPath = path;
            if (String.IsNullOrEmpty(newPath))
            {
                return "";
            }

            if (newPath.ToLower().StartsWith("root\\\\"))
            {
                newPath = newPath.Remove(0, 6);
            }

            if (newPath.ToLower().Equals("root"))
            {
                newPath = newPath.Remove(0, 4);
            }

            if (newPath.StartsWith("\\"))
            {
                newPath = newPath.Remove(0, 1);
            }

            newPath = String.IsNullOrEmpty(newPath) ? resourceName : newPath + "\\" + resourceName;

            return newPath.Replace("\\\\", "\\").Replace("\\\\", "\\");
        }

        public static string GetServerLogSettingsConfigFile()
        {
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var serverLogFolder = Path.Combine(localAppDataFolder, "Warewolf", "Server Log");

            IDirectoryHelper directoryHelper = new DirectoryHelper();
            directoryHelper.CreateIfNotExists(serverLogFolder);

            var serverLogFile = Path.Combine(serverLogFolder, "warewolf-Server.log");
            return serverLogFile;
        }
    }
}
