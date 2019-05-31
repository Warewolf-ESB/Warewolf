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

using Dev2.Common.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
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

            IDirectory directory = new DirectoryWrapper();
            directory.CreateIfNotExists(studioFolder);
            
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

            IDirectory directory = new DirectoryWrapper();
            directory.CreateIfNotExists(serverLogFolder);

            var serverLogFile = Path.Combine(serverLogFolder, "warewolf-Server.log");
            return serverLogFile;
        }
    }
}
