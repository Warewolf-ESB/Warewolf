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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.ComponentModel;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.Configuration.Settings;
using log4net;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace Dev2.Common
{
    /// <summary>
    ///     A single common logging location ;)
    /// </summary>
    public static class Dev2Logger
    {
        public static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        private static LoggingSettings _loggingSettings;
        private static IDictionary<Guid, string> _workflowsToLog;

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

        public static bool EnableDebugOutput
        {
            get { return ((Logger) Log.Logger).Level <= Level.Debug; }
// ReSharper disable ValueParameterNotUsed
            set
// ReSharper restore ValueParameterNotUsed
            {
                if (value)
                    UpdateLoggingConfig("Debug");
            }
        }

        public static bool EnableErrorOutput
        {
            get { return ((Logger) Log.Logger).Level <= Level.Error; }
// ReSharper disable ValueParameterNotUsed
            set
// ReSharper restore ValueParameterNotUsed
            {
                if (value)
                    UpdateLoggingConfig("Error");
            }
        }

        public static bool EnableInfoOutput
        {
            get { return ((Logger) Log.Logger).Level <= Level.Info; }
// ReSharper disable ValueParameterNotUsed
            set
// ReSharper restore ValueParameterNotUsed
            {
                if (value)
                    UpdateLoggingConfig("INFO");
            }
        }

        public static bool EnableTraceOutput
        {
            get { return ((Logger) Log.Logger).Level <= Level.Info; }
// ReSharper disable ValueParameterNotUsed
            set
// ReSharper restore ValueParameterNotUsed
            {
                if (value)
                    UpdateLoggingConfig("DEBUG");
            }
        }

        public static void SetLog(ILog log)
        {
            Log = log;
        }


        private static void UpdateLoggingConfig(string Level)
        {
            ILoggerRepository repository = LogManager.GetAllRepositories().First();
            repository.Threshold = repository.LevelMap[Level];
            var hier = (Hierarchy) repository;
            ILogger[] loggers = hier.GetCurrentLoggers();
            foreach (ILogger logger in loggers)
            {
                ((Logger) logger).Level = hier.LevelMap[Level];
            }


            //Configure the root logger.
            var h = (Hierarchy) LogManager.GetRepository();
            Logger rootLogger = h.Root;
            rootLogger.Level = h.LevelMap[Level];
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

                string dirPath = GetDirectoryPath(LoggingSettings);
                bool dirExists = Directory.Exists(dirPath);
                if (!dirExists)
                {
                    Directory.CreateDirectory(dirPath);
                }

                _workflowsToLog = new Dictionary<Guid, string>();
                foreach (IWorkflowDescriptor wf in LoggingSettings.Workflows)
                {
                    _workflowsToLog.Add(Guid.Parse(wf.ResourceID), wf.ResourceName);
                }
            });
        }


        ///// <summary>
        ///// Only continue with logging if workflow selected in logsettings or allworkflows selected
        ///// </summary>
        ///// <param name="iDebugState">THe debug state</param>
        ///// <returns></returns>
        ///// <author>Jurie.smit</author>
        ///// <date>2013/05/21</date>
        [ExcludeFromCodeCoverage] // wf debug logging
        public static bool ShouldLog(IDebugState iDebugState)
        {
            var debugState = iDebugState as DebugState;
            if (debugState == null)
            {
                return false;
            }

            return ShouldLog(debugState.OriginatingResourceID);
        }

        [ExcludeFromCodeCoverage] // wf debug logging
        public static bool ShouldLog(Guid resourceID)
        {
            //Unnecessary to continue if logging is turned off
            if (!EnableLogOutput)
            {
                return false;
            }

            //only log if included in the settings
            bool shouldlog = LoggingSettings.LogAll ||
                             _workflowsToLog.ContainsKey(resourceID);
            return shouldlog;
        }

        [ExcludeFromCodeCoverage] // wf debug logging
        public static string GetDirectoryPath(LoggingSettings loggingSettings)
        {
            string dirPath = loggingSettings.LogFileDirectory;

            if (string.IsNullOrWhiteSpace(dirPath))
            {
                dirPath = GetDefaultLogDirectoryPath();
            }
            return dirPath;
        }

        [ExcludeFromCodeCoverage] // wf debug logging
        public static string GetDefaultLogDirectoryPath()
        {
            string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (rootDir != null)
            {
                return Path.Combine(rootDir, "Logs");
            }

            return string.Empty;
        }
    }
}