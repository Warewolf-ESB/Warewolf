/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Dev2.Runtime.Configuration.Settings;

namespace Dev2.Diagnostics.Logging
{
    static public class WorkflowLogger
    {
        static LoggingSettings _loggingSettings;
        static IDictionary<Guid, string> _workflowsToLog;
        public static bool EnableLogOutput { get; private set; }
        public static LoggingSettings LoggingSettings
        {
            get { return _loggingSettings; }
            set
            {
                if (_loggingSettings == value)
                {
                    return;
                }

                _loggingSettings = value;
                UpdateSettings(_loggingSettings);
            }
        }

        public static void UpdateSettings(LoggingSettings loggingSettings)
        {
            Task.Run(() =>
            {
                LoggingSettings = loggingSettings;

                EnableLogOutput = LoggingSettings.IsLoggingEnabled;

                //Unnecessary to continue if logging is turned off
                if (!EnableLogOutput)
                {
                    return;
                }

                var dirPath = GetDirectoryPath(LoggingSettings);
                var dirExists = Directory.Exists(dirPath);
                if (!dirExists)
                {
                    Directory.CreateDirectory(dirPath);
                }

                _workflowsToLog = new Dictionary<Guid, string>();
                foreach (var wf in LoggingSettings.Workflows)
                {
                    _workflowsToLog.Add(Guid.Parse(wf.ResourceID), wf.ResourceName);
                }

            });
        }

        public static bool ShouldLog(Guid resourceID)
        {
            if (!EnableLogOutput)
            {
                return false;
            }
            var shouldlog = LoggingSettings.LogAll || _workflowsToLog.ContainsKey(resourceID);
            return shouldlog;
        }

        public static string GetDirectoryPath(LoggingSettings loggingSettings)
        {
            var dirPath = loggingSettings.LogFileDirectory;

            if (String.IsNullOrWhiteSpace(dirPath))
            {
                dirPath = GetDefaultLogDirectoryPath();
            }
            return dirPath;
        }
        public static string GetDefaultLogDirectoryPath()
        {
            var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (rootDir != null)
            {
                return Path.Combine(rootDir, "Logs");
            }

            return String.Empty;
        }
    }
}