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
using System.Diagnostics;
using System.IO;
using Dev2.Common;


namespace Dev2.Providers.Logs
{
    public class CustomTextWriter : TraceListener
    {
        public static string LoggingFileName => Path.Combine(StudioLogPath, "Warewolf Studio.log");

        public static string WarewolfAppPath
        {
            get
            {
                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var warewolfAppPath = Path.Combine(appDataFolder, "Warewolf");
                if(!Directory.Exists(warewolfAppPath))
                {
                    Directory.CreateDirectory(warewolfAppPath);
                }
                return warewolfAppPath;
            }
        }

        public static string StudioLogPath
        {
            get
            {
                var studioLogPath = Path.Combine(WarewolfAppPath, "Studio Logs");
                if(!Directory.Exists(studioLogPath))
                {
                    Directory.CreateDirectory(studioLogPath);
                }
                return studioLogPath;
            }
        }

        public override void Write(string message) => Dev2Logger.Info(message, GlobalConstants.WarewolfInfo);

        public override void WriteLine(string message) => Dev2Logger.Info(message, GlobalConstants.WarewolfInfo);
    }
}
