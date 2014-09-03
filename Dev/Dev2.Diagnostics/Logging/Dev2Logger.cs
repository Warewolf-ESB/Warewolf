using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.Configuration.Settings;
using log4net.Core;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Common
{
    /// <summary>
    /// A single common logging location ;)
    /// </summary>
    public static class Dev2Logger
    {
        public static log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
       

        public  static void SetLog(log4net.ILog log)
        {
            Log = log;
        }
        

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
        public static bool EnableDebugOutput
        {
            get
            {
               return ((log4net.Repository.Hierarchy.Logger)Log.Logger).Level <= Level.Debug;
            }
// ReSharper disable ValueParameterNotUsed
            set
// ReSharper restore ValueParameterNotUsed
            {
                UpdateLoggingConfig("Debug");
            }
        }
        public static bool EnableErrorOutput
        {
            get
            {
                return ((log4net.Repository.Hierarchy.Logger)Log.Logger).Level <= Level.Error;
            }
// ReSharper disable ValueParameterNotUsed
            set
// ReSharper restore ValueParameterNotUsed
            {
                UpdateLoggingConfig("Error");
            }
        }
        public static bool EnableInfoOutput
        {
            get
            {
                return ((log4net.Repository.Hierarchy.Logger)Log.Logger).Level <= Level.Info;
            }
// ReSharper disable ValueParameterNotUsed
            set
// ReSharper restore ValueParameterNotUsed
            {
                UpdateLoggingConfig("INFO");
            }
        }
        public static bool EnableTraceOutput
        {
            get
            {
                return ((log4net.Repository.Hierarchy.Logger)Log.Logger).Level <= Level.Info;
            }
// ReSharper disable ValueParameterNotUsed
            set
// ReSharper restore ValueParameterNotUsed
            {
                UpdateLoggingConfig("DEBUG");
            }
        }


        private static void UpdateLoggingConfig(string Level)
        {

            log4net.Repository.ILoggerRepository repository = log4net.LogManager.GetAllRepositories().First();
            repository.Threshold = repository.LevelMap[Level];
            log4net.Repository.Hierarchy.Hierarchy hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
            var loggers = hier.GetCurrentLoggers();
            foreach (var logger in loggers)
            {
                ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap[Level];
            }
            

            //Configure the root logger.
            log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
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
            var dirPath = loggingSettings.LogFileDirectory;

            if (string.IsNullOrWhiteSpace(dirPath))
            {
                dirPath = GetDefaultLogDirectoryPath();
            }
            return dirPath;
        }
        [ExcludeFromCodeCoverage] // wf debug logging
        public static string GetDefaultLogDirectoryPath()
        {
            var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (rootDir != null)
            {
                return Path.Combine(rootDir, "Logs");
            }

            return string.Empty;
        }

    }


}
