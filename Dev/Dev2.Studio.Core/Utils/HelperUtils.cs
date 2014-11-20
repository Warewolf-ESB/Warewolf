
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Windows;
using Dev2.Common.Interfaces.Studio.Controller;

namespace Dev2.Utils
{
    public static class HelperUtils
    {
        public static string GetStudioLogSettingsConfigFile()
        {
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var settingsConfigFile = Path.Combine(localAppDataFolder, "Warewolf", "Studio", "Settings.config");
            return settingsConfigFile;
        }
        public static void ShowTrustRelationshipError(SystemException exception)
        {
            if(exception.Message.Contains("The trust relationship between this workstation and the primary domain failed"))
            {
                                var popup = CustomContainer.Get<IPopupController>();
                                popup.Header = "Error connecting to server";
                                popup.Description = "This computer cannot contact the Domain Controller."
                                                    + Environment.NewLine + "If it does not belong to a domain, please ensure it is removed from the domain in computer management.";
                                popup.Buttons = MessageBoxButton.OK;
                                popup.ImageType = MessageBoxImage.Error;
                                popup.Show();
            }
        }

        public static string SanitizePath(string path, string resourceName = "")
        {
            if (String.IsNullOrEmpty(path))
            {
                return "";
            }

            if (path.ToLower().StartsWith("root\\\\"))
            {
                path = path.Remove(0, 6);
            }

            if (path.ToLower().Equals("root"))
            {
                path = path.Remove(0, 4);
            }

            if (path.StartsWith("\\"))
            {
                path = path.Remove(0, 1);
            }

            path = String.IsNullOrEmpty(path) ? resourceName : path + "\\" + resourceName;

            return path.Replace("\\\\", "\\")
                .Replace("\\\\", "\\");
        }
    }
}
