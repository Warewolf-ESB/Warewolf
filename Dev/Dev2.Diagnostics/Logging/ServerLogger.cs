using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Dev2.Common.Common;
using Dev2.Diagnostics;
using Dev2.Instrumentation;
using Dev2.Runtime.Configuration.Settings;

namespace Dev2.Common
{
    /// <summary>
    /// A single common logging location ;)
    /// </summary>
    public static class ServerLogger
    {

        #region private fields

        static readonly object _lock = new object();

        static LoggingSettings _loggingSettings;
        static IDictionary<Guid, string> _workflowsToLog;
        static IDictionary<Guid, string> _currentExecutionLogs;
        static IDictionary<Guid, StreamWriter> _currentLogStreams;
        static IDictionary<Guid, int> _nestedLevels;
        static IDictionary<Guid, GuidTree> _lastNested;
        static IDictionary<Guid, DateTime> _startTimes;

        static IDictionary<Guid, DateTime> StartTimes
        {
            get
            {
                return _startTimes ??
                       (_startTimes = new Dictionary<Guid, DateTime>());
            }
        }
        static IDictionary<Guid, string> CurrentExecutionLogs
        {
            get
            {
                return _currentExecutionLogs ??
                       (_currentExecutionLogs = new Dictionary<Guid, string>());
            }
        }

        static IDictionary<Guid, StreamWriter> CurrentLogStreams
        {
            get
            {
                return _currentLogStreams ??
                       (_currentLogStreams = new Dictionary<Guid, StreamWriter>());
            }
        }

        static IDictionary<Guid, int> NestedLevels
        {
            get
            {
                return _nestedLevels ??
                       (_nestedLevels = new Dictionary<Guid, int>());
            }
        }

        static IDictionary<Guid, GuidTree> LastNested
        {
            get
            {
                return _lastNested ??
                       (_lastNested = new Dictionary<Guid, GuidTree>());
            }
        }

        #endregion private fields

        #region public properties

        /// <summary>
        /// Gets or sets a value indicating whether [enable debug output].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable debug output]; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableDebugOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable trace output].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable trace output]; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableTraceOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable error output].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable error output]; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableErrorOutput { get; set; }

        public static bool EnableLogOutput { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable info output].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable info output]; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableInfoOutput { get; set; }

        public static void LogCallStack(string className, [CallerMemberName] string methodName = null)
        {
            if(EnableInfoOutput)
            {
                var stackTrace = new System.Diagnostics.StackTrace(1, true); // skip the call to ServerLogger.LogCallStack
                InternalLogMessage(string.Format("{0}.{1}{2}{3}", className, methodName, Environment.NewLine, stackTrace), "CALLSTACK");
            }
        }

        public static void LogDebugStart(string className, [CallerMemberName] string methodName = null)
        {
            LogDebug(string.Format("{0}.{1}.START", className, methodName));
        }

        public static void LogDebugEnd(string className, [CallerMemberName] string methodName = null)
        {
            LogDebug(string.Format("{0}.{1}.END", className, methodName));
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogMessage(string message)
        {
            if(EnableInfoOutput)
            {
                InternalLogMessage(message, "INFO");
            }

        }

        /// <summary>
        /// Logs the debug.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogDebug(string message)
        {
            if(EnableDebugOutput)
            {
                InternalLogMessage(message, "DEBUG");
            }
        }

        /// <summary>
        /// Logs the trace.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogTrace(string message)
        {
            if(EnableTraceOutput)
            {
                InternalLogMessage(message, "TRACE");
            }
        }

        public static LoggingSettings LoggingSettings
        {
            get { return _loggingSettings; }
            set
            {
                if(_loggingSettings == value)
                {
                    return;
                }

                _loggingSettings = value;
                UpdateSettings(_loggingSettings);
            }
        }

        public static string WebserverUri { get; set; }

        #endregion public properties

        #region public methods


        public static void UpdateSettings(LoggingSettings loggingSettings)
        {
            Task.Run(() =>
            {
                LoggingSettings = loggingSettings;

                EnableLogOutput = LoggingSettings.IsLoggingEnabled;

                //Unnecessary to continue if logging is turned off
                if(!EnableLogOutput)
                {
                    return;
                }

                var dirPath = GetDirectoryPath(LoggingSettings);
                var dirExists = Directory.Exists(dirPath);
                if(!dirExists)
                {
                    Directory.CreateDirectory(dirPath);
                }

                _workflowsToLog = new Dictionary<Guid, string>();
                foreach(var wf in LoggingSettings.Workflows)
                {
                    _workflowsToLog.Add(Guid.Parse(wf.ResourceID), wf.ResourceName);
                }

            });
        }

        #endregion public methods

        #region private methods

        static void InternalLogMessage(string message, string typeOf)
        {
            Task.Run(() =>
            {
                try
                {

                    lock(_lock)
                    {
                        File.AppendAllText(Path.Combine(EnvironmentVariables.ApplicationPath, "ServerLog.txt"),
                                                string.Format("{0} :: {1} -> {2}{3}", DateTime.Now, typeOf, message,
                                                                Environment.NewLine));
                    }

                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // We do not care, best effort 
                }
            });
        }

        #endregion private methods

        #region LogDebug

        public static void LogDebug(IDebugState idebugState)
        {
            // Not stream safe?!

            //Task.Run(() =>
            //{
            //    if(!ShouldLog(idebugState))
            //        return;

            //    var debugState = (DebugState)idebugState;

            //    string workflowName = GetWorkflowName(debugState);

            //    if(String.IsNullOrWhiteSpace(workflowName))
            //    {
            //        throw new NoNullAllowedException("Only workflows with valid names can be logged");
            //    }

            //    switch(debugState.StateType)
            //    {
            //        case StateType.Start:
            //            Initialize(debugState, workflowName);
            //            break;

            //        case StateType.Before:
            //            Serialize(debugState, workflowName, StateType.Before);
            //            break;

            //        case StateType.End:
            //            Finalize(debugState, workflowName);
            //            break;

            //        case StateType.After:
            //            Serialize(debugState, workflowName, StateType.After);
            //            break;

            //        default:
            //            Serialize(debugState, workflowName);
            //            break;
            //    }
            //});

        }

        #region Serialization

        /// <summary>
        /// Serializes the specified debug state. Will affect the nested levels
        /// Used when you know the state, 
        /// usually with a workflow, or scoped activity (ie, nested)
        /// </summary>
        /// <param name="debugState">State of the debug.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="state">The state.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        static void Serialize(DebugState debugState, string workflowName, StateType state)
        {
            //Check nesting levels - if zero this is not applicable
            if(LoggingSettings.NestedLevelCount > 0)
            {
                switch(state)
                {
                    case StateType.Before:
                        if(CheckAndAdjustLevel(debugState, +1))
                        {
                            return;
                        }

                        NestThis(debugState);
                        break;

                    case StateType.After:
                        if(CheckAndAdjustLevel(debugState, -1))
                        {
                            return;
                        }

                        UnNestThis(debugState);
                        break;
                }
            }

            //Get appropriate stream
            var logPath = GetLogPath(workflowName, debugState);
            var writer = GetLogStream(logPath, debugState);

            SerializeToXML(debugState, writer, new[] { typeof(DebugItem) });
        }

        static void UnNestThis(DebugState debugState)
        {
            GuidTree lastNested = LastNested[debugState.OriginalInstanceID];
            if(lastNested != null)
            {
                LastNested[debugState.OriginalInstanceID] = lastNested.Parent;
            }
        }

        static void NestThis(DebugState debugState)
        {
            GuidTree previousNested;
            LastNested.TryGetValue(debugState.OriginalInstanceID, out previousNested);
            var lastNested = new GuidTree(debugState.ID, previousNested)
                {
                    Name = debugState.DisplayName,
                    LogChildren = ((NestedLevels[debugState.OriginalInstanceID] == 0) ||
                                   (NestedLevels[debugState.OriginalInstanceID] <
                                    LoggingSettings.NestedLevelCount))
                };

            LastNested[debugState.OriginalInstanceID] = lastNested;
        }

        /// <summary>
        /// Serializes the specified debug state without affecting nesting levels
        /// </summary>
        /// <param name="debugState">State of the debug.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        static void Serialize(DebugState debugState, string workflowName)
        {
            //Do not log if lastnested is not the parent
            GuidTree lastNested;
            LastNested.TryGetValue(debugState.OriginalInstanceID, out lastNested);

            //Only log if lastNested should log children
            if(lastNested != null && !lastNested.LogChildren)
            {
                return;
            }

            //Get appropriate stream
            var logPath = GetLogPath(workflowName, debugState);
            var writer = GetLogStream(logPath, debugState);

            SerializeToXML(debugState, writer, new[] { typeof(DebugItem) });
        }

        /// <summary>
        /// Serializes a specific instance of a class to a streamwriter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toSerialize">To serialize.</param>
        /// <param name="streamWriter">The streamwriter to serialize to.</param>
        /// <param name="extraTypes">The extra types that is included in the hierarchy.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        public static void SerializeToXML<T>(T toSerialize, StreamWriter streamWriter, Type[] extraTypes)
            where T : class
        {
            var serializer = new XmlSerializer(toSerialize.GetType(), null, extraTypes, null, string.Empty);

            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                CloseOutput = false,
                NewLineHandling = NewLineHandling.Entitize,
                Indent = true
            };

            using(var writer = XmlWriter.Create(streamWriter, settings))
            {
                serializer.Serialize(writer, toSerialize);
                writer.WriteRaw(Environment.NewLine);
            }
        }

        #endregion serialization

        #region Initialization

        static void Initialize(DebugState debugState, string workflowName)
        {
            if(!debugState.IsFirstStep())
            {
                return;
            }

            var logPath = InitLogPath(workflowName, debugState);
            InitLogStream(logPath, debugState);
            NestedLevels.Add(debugState.OriginalInstanceID, 0);
            NestThis(debugState);
            StartTimes.Add(debugState.OriginalInstanceID, debugState.StartTime);

            var writer = GetLogStream(logPath, debugState);
            SerializeToXML(debugState, writer, new[] { typeof(DebugItem) });
        }

        static string InitLogPath(string workflowName, IDebugState debugState)
        {
            var dirPath = GetDirectoryPath(LoggingSettings);
            var dateTime = DateTime.Now;
            var logPath = Path.Combine(dirPath, string.Format("{0}-{1:yyyyMMdd hh-mm-ss-fff}.wwlfl",
                workflowName, dateTime));

            //now cache this file for reuse with any other executions of this instance id
            CurrentExecutionLogs.Add(debugState.OriginalInstanceID, logPath);
            return logPath;
        }

        static void InitLogStream(string logPath, IDebugState debugState)
        {
            var filestream = new FileStream(logPath, FileMode.Append);
            var writer = new StreamWriter(filestream);
            writer.WriteLineAsync("<Workflow>");
            CurrentLogStreams.Add(debugState.OriginalInstanceID, writer);
        }

        #endregion

        #region Finalization

        static void Finalize(DebugState debugState, string workflowName)
        {
            //Remove the stored information if it is final step
            if(debugState.IsFinalStep())
            {
                DateTime startTime;
                var exists = StartTimes.TryGetValue(debugState.OriginalInstanceID, out startTime);
                if(exists)
                {
                    debugState.StartTime = StartTimes[debugState.OriginalInstanceID];
                    var logPath = GetLogPath(workflowName, debugState);
                    var writer = GetLogStream(logPath, debugState);

                    SerializeToXML(debugState, writer, new[] { typeof(DebugItem) });
                }

                RunPostWorkflow(debugState.OriginalInstanceID, debugState.OriginatingResourceID);
                Remove(debugState);
            }
        }

        static void RunPostWorkflow(Guid originaInstanceID, Guid originatingResourceID)
        {
            if(!LoggingSettings.RunPostWorkflow)
            {
                return;
            }

            if(LoggingSettings.PostWorkflow == null)
            {
                return;
            }

            //Dont run postworkflow if it is the originating resource (would cause recursive loop)
            if(LoggingSettings.PostWorkflow.ResourceID == originatingResourceID.ToString())
            {
                return;
            }

            string input = string.Empty;
            if(!string.IsNullOrWhiteSpace(LoggingSettings.ServiceInput))
            {
                input += LoggingSettings.ServiceInput;
                input += "=";
                input += _currentExecutionLogs[originaInstanceID];
            }

            string postData = String.Format("{0}/{1}/{2}?{3}",
                WebserverUri, "services", LoggingSettings.PostWorkflow.ResourceName, input);

            var request = WebRequest.Create(postData);

            request.Method = "POST";
        }

        static void Remove(IDebugState debugState)
        {
            var writer = CurrentLogStreams[debugState.OriginalInstanceID];
            writer.WriteLine("</Workflow>");
            writer.Flush();
            writer.Close();
            writer.Dispose();
            CurrentLogStreams.Remove(debugState.OriginalInstanceID);
            StartTimes.Remove(debugState.OriginalInstanceID);
            CurrentExecutionLogs.Remove(debugState.OriginalInstanceID);
            NestedLevels.Remove(debugState.OriginalInstanceID);
            LastNested.Remove(debugState.OriginalInstanceID);
        }

        #endregion

        #region Public Helpers

        public static bool CheckAndAdjustLevel(DebugState debugState, int i)
        {
            var currentLevel = NestedLevels[debugState.OriginalInstanceID];

            //check - if i is positive, it is before and we need to take equal into account
            // if i is negative it is being decrease and equal shouldnt be logged
            var isAboveLimit = (i >= 0)
                                   ? currentLevel >= LoggingSettings.NestedLevelCount
                                   : currentLevel > LoggingSettings.NestedLevelCount;

            //adjust
            currentLevel = currentLevel + i;
            NestedLevels[debugState.OriginalInstanceID] = currentLevel;

            return isAboveLimit;
        }

        /// <summary>
        /// Only continue with logging if workflow selected in logsettings or allworkflows selected
        /// </summary>
        /// <param name="iDebugState">THe debug state</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        public static bool ShouldLog(IDebugState iDebugState)
        {
            var debugState = iDebugState as DebugState;
            if(debugState == null)
            {
                return false;
            }

            return ShouldLog(debugState.OriginatingResourceID);
        }

        public static bool ShouldLog(Guid resourceID)
        {
            //Unnecessary to continue if logging is turned off
            if(!EnableLogOutput)
            {
                return false;
            }

            //only log if included in the settings
            bool shouldlog = LoggingSettings.LogAll ||
                _workflowsToLog.ContainsKey(resourceID);
            return shouldlog;
        }

        public static string GetWorkflowName(IDebugState debugState)
        {
            string name;
            _workflowsToLog.TryGetValue(debugState.OriginatingResourceID, out name);
            if(string.IsNullOrWhiteSpace(name))
            {
                _workflowsToLog[debugState.OriginatingResourceID] = debugState.DisplayName;
                return debugState.DisplayName;
            }
            return name;
        }


        public static StreamWriter GetLogStream(string logPath, IDebugState debugState)
        {
            StreamWriter currentStream;
            CurrentLogStreams.TryGetValue(debugState.OriginalInstanceID, out currentStream);
            if(currentStream != null)
            {
                return currentStream;
            }

            throw new Exception("Logstream not found, check initialization or early disposal.");
        }

        public static string GetLogPath(string workflowName, IDebugState debugState)
        {
            string currentPath;
            CurrentExecutionLogs.TryGetValue(debugState.OriginalInstanceID, out currentPath);
            if(!String.IsNullOrWhiteSpace(currentPath))
            {
                return currentPath;
            }

            throw new Exception("Logpath not found, check initialization or early disposal.");
        }

        public static string GetDirectoryPath(LoggingSettings loggingSettings)
        {
            var dirPath = loggingSettings.LogFileDirectory;

            if(string.IsNullOrWhiteSpace(dirPath))
            {
                dirPath = GetDefaultLogDirectoryPath();
            }
            return dirPath;
        }

        public static string GetDefaultLogDirectoryPath()
        {
            var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if(rootDir != null)
            {
                return Path.Combine(rootDir, "Logs");
            }

            return string.Empty;
        }

        #endregion

        #endregion LogDebug

        public static void LogError(string className, Exception ex, [CallerMemberName] string methodName = null)
        {
            LogError(className, methodName, ex);
        }

        public static void LogError(this object obj, Exception ex, [CallerMemberName] string methodName = null)
        {
            LogError(obj, methodName, ex);
        }

        public static void LogError(this object obj, string message, [CallerMemberName] string methodName = null)
        {
            LogError(obj, methodName, new Exception(message));
        }

        public static void LogTrace(this object obj, string message = null, [CallerMemberName] string methodName = null)
        {
            LogTrace(CreateMessage(obj == null ? string.Empty : obj.GetType().Name, methodName, message));
        }

        static void LogError(object obj, string methodName, Exception e)
        {
            LogError(obj == null ? "UnknownClass" : obj.GetType().Name, methodName, e);
        }

        static void LogError(string className, string methodName, Exception e)
        {
            Tracker.TrackException(className, methodName, e);

            if(EnableErrorOutput)
            {
                var errors = new StringBuilder();
                errors.AppendLine(CreateMessage(className, methodName, e.Message));
                errors.AppendLine(e.GetAllMessages());
                InternalLogMessage(errors.ToString(), "ERROR");
            }

        }

        static string CreateMessage(string className, string methodName, string message)
        {
            return string.Format("{0}.{1} : {2}", className, methodName, message);
        }
    }

    internal class GuidTree
    {
        public Guid ID { get; set; }
        public GuidTree Parent { get; set; }
        public bool LogChildren { get; set; }
        public string Name { get; set; }

        public GuidTree(Guid id, GuidTree parent)
        {
            ID = id;
            Parent = parent;
        }
    }
}
