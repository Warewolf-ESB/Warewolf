using System;
using System.Diagnostics;
using System.Security;
using System.Threading;
using log4net.Appender;
using log4net.Core;
using log4net.Util;
using SecurityContext = log4net.Core.SecurityContext;

namespace log4net.Appender
{
    /// <summary>
    /// EventLogAppender sourced from the log4net repo but repackaged to be usable in netstandard2.0 projects
    /// </summary>
    public class EventLogAppender : AppenderSkeleton
    {
        /// <summary>
        ///     The maximum size supported by default.
        /// </summary>
        /// <remarks>
        ///     http://msdn.microsoft.com/en-us/library/xzwc042w(v=vs.100).aspx
        ///     The 32766 documented max size is two bytes shy of 32K (I'm assuming 32766
        ///     may leave space for a two byte null terminator of #0#0). The 32766 max
        ///     length is what the .NET 4.0 source code checks for, but this is WRONG!
        ///     Strings with a length > 31839 on Windows Vista or higher can CORRUPT
        ///     the event log! See: System.Diagnostics.EventLogInternal.InternalWriteEvent()
        ///     for the use of the 32766 max size.
        /// </remarks>
        private const int MaxEventlogMessageSizeDefault = 32766;

        /// <summary>
        ///     The maximum size supported by a windows operating system that is vista
        ///     or newer.
        /// </summary>
        /// <remarks>
        ///     See ReportEvent API:
        ///     http://msdn.microsoft.com/en-us/library/aa363679(VS.85).aspx
        ///     ReportEvent's lpStrings parameter:
        ///     "A pointer to a buffer containing an array of
        ///     null-terminated strings that are merged into the message before Event Viewer
        ///     displays the string to the user. This parameter must be a valid pointer
        ///     (or NULL), even if wNumStrings is zero. Each string is limited to 31,839 characters."
        ///     Going beyond the size of 31839 will (at some point) corrupt the event log on Windows
        ///     Vista or higher! It may succeed for a while...but you will eventually run into the
        ///     error: "System.ComponentModel.Win32Exception : A device attached to the system is
        ///     not functioning", and the event log will then be corrupt (I was able to corrupt
        ///     an event log using a length of 31877 on Windows 7).
        ///     The max size for Windows Vista or higher is documented here:
        ///     http://msdn.microsoft.com/en-us/library/xzwc042w(v=vs.100).aspx.
        ///     Going over this size may succeed a few times but the buffer will overrun and
        ///     eventually corrupt the log (based on testing).
        ///     The maxEventMsgSize size is based on the max buffer size of the lpStrings parameter of the ReportEvent API.
        ///     The documented max size for EventLog.WriteEntry for Windows Vista and higher is 31839, but I'm leaving room for a
        ///     terminator of #0#0, as we cannot see the source of ReportEvent (though we could use an API monitor to examine the
        ///     buffer, given enough time).
        /// </remarks>
        private const int MaxEventlogMessageSizeVistaOrNewer = 31839 - 2;

        /// <summary>
        ///     The fully qualified type of the EventLogAppender class.
        /// </summary>
        /// <remarks>
        ///     Used by the internal logger to record the Type of the
        ///     log message.
        /// </remarks>
        private static readonly Type DeclaringType = typeof(EventLogAppender);

        /// <summary>
        ///     The maximum size that the operating system supports for
        ///     a event log message.
        /// </summary>
        /// <remarks>
        ///     Used to determine the maximum string length that can be written
        ///     to the operating system event log and eventually truncate a string
        ///     that exceeds the limits.
        /// </remarks>
        private static readonly int MaxEventlogMessageSize = GetMaxEventLogMessageSize();

        /// <summary>
        ///     Mapping from level object to EventLogEntryType
        /// </summary>
        private readonly LevelMapping _levelMapping = new LevelMapping();

        /// <summary>
        ///     The name of the machine which holds the event log. This is
        ///     currently only allowed to be '.' i.e. the current machine.
        /// </summary>
        private readonly string _machineName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventLogAppender" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Default constructor.
        ///     </para>
        /// </remarks>
        public EventLogAppender()
        {
            ApplicationName = Thread.GetDomain().FriendlyName;
            LogName = "Application"; // Defaults to application log
            _machineName = "."; // Only log on the local machine
        }

        /// <summary>
        ///     The name of the log where messages will be stored.
        /// </summary>
        /// <value>
        ///     The string name of the log where messages will be stored.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         This is the name of the log as it appears in the Event Viewer
        ///         tree. The default value is to log into the <c>Application</c>
        ///         log, this is where most applications write their events. However
        ///         if you need a separate log for your application (or applications)
        ///         then you should set the <see cref="LogName" /> appropriately.
        ///     </para>
        ///     <para>
        ///         This should not be used to distinguish your event log messages
        ///         from those of other applications, the <see cref="ApplicationName" />
        ///         property should be used to distinguish events. This property should be
        ///         used to group together events into a single log.
        ///     </para>
        /// </remarks>
        public string LogName { get; set; }

        /// <summary>
        ///     Property used to set the Application name.  This appears in the
        ///     event logs when logging.
        /// </summary>
        /// <value>
        ///     The string used to distinguish events from different sources.
        /// </value>
        /// <remarks>
        ///     Sets the event log source property.
        /// </remarks>
        public string ApplicationName { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="SecurityContext" /> used to write to the EventLog.
        /// </summary>
        /// <value>
        ///     The <see cref="SecurityContext" /> used to write to the EventLog.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         The system security context used to write to the EventLog.
        ///     </para>
        ///     <para>
        ///         Unless a <see cref="SecurityContext" /> specified here for this appender
        ///         the <see cref="SecurityContextProvider.DefaultProvider" /> is queried for the
        ///         security context to use. The default behavior is to use the security context
        ///         of the current thread.
        ///     </para>
        /// </remarks>
        public SecurityContext SecurityContext { get; set; }

        /// <summary>
        ///     Gets or sets the <c>EventId</c> to use unless one is explicitly specified via the <c>LoggingEvent</c>'s properties.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The <c>EventID</c> of the event log entry will normally be
        ///         set using the <c>EventID</c> property (<see cref="LoggingEvent.Properties" />)
        ///         on the <see cref="LoggingEvent" />.
        ///         This property provides the fallback value which defaults to 0.
        ///     </para>
        /// </remarks>
        public int EventId { get; set; } = 0;

        /// <summary>
        ///     Gets or sets the <c>Category</c> to use unless one is explicitly specified via the <c>LoggingEvent</c>'s
        ///     properties.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The <c>Category</c> of the event log entry will normally be
        ///         set using the <c>Category</c> property (<see cref="LoggingEvent.Properties" />)
        ///         on the <see cref="LoggingEvent" />.
        ///         This property provides the fallback value which defaults to 0.
        ///     </para>
        /// </remarks>
        public short Category { get; set; } = 0;

        /// <summary>
        ///     This appender requires a <see cref="Layout" /> to be set.
        /// </summary>
        /// <value>
        ///     <c>true</c>
        /// </value>
        protected override bool RequiresLayout => true;

        /// <summary>
        ///     Add a mapping of level to <see cref="EventLogEntryType" /> - done by the config file
        /// </summary>
        /// <param name="mapping">The mapping to add</param>
        /// <remarks>
        ///     <para>
        ///         Add a <see cref="Level2EventLogEntryType" /> mapping to this appender.
        ///         Each mapping defines the event log entry type for a level.
        ///     </para>
        /// </remarks>
        public void AddMapping(Level2EventLogEntryType mapping)
        {
            _levelMapping.Add(mapping);
        }

        /// <summary>
        ///     Initialize the appender based on the options set
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is part of the <see cref="IOptionHandler" /> delayed object
        ///         activation scheme. The <see cref="ActivateOptions" /> method must
        ///         be called on this object after the configuration properties have
        ///         been set. Until <see cref="ActivateOptions" /> is called this
        ///         object is in an undefined state and must not be used.
        ///     </para>
        ///     <para>
        ///         If any of the configuration properties are modified then
        ///         <see cref="ActivateOptions" /> must be called again.
        ///     </para>
        /// </remarks>
        public override void ActivateOptions()
        {
            try
            {
                base.ActivateOptions();

                if (SecurityContext == null)
                    SecurityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);

                bool sourceAlreadyExists;
                string currentLogName = null;

                using (SecurityContext.Impersonate(this))
                {
                    sourceAlreadyExists = EventLog.SourceExists(ApplicationName);
                    if (sourceAlreadyExists)
                        currentLogName = EventLog.LogNameFromSourceName(ApplicationName, _machineName);
                }

                if (sourceAlreadyExists && currentLogName != LogName)
                    LogLog.Debug(DeclaringType,
                        "Changing event source [" + ApplicationName + "] from log [" + currentLogName + "] to log [" +
                        LogName + "]");
                else if (!sourceAlreadyExists)
                    LogLog.Debug(DeclaringType,
                        "Creating event source Source [" + ApplicationName + "] in log " + LogName + "]");

                string registeredLogName = null;

                using (SecurityContext.Impersonate(this))
                {
                    if (sourceAlreadyExists && currentLogName != LogName)
                    {
                        //
                        // Re-register this to the current application if the user has changed
                        // the application / logfile association
                        //
                        EventLog.DeleteEventSource(ApplicationName, _machineName);
                        CreateEventSource(ApplicationName, LogName, _machineName);

                        registeredLogName = EventLog.LogNameFromSourceName(ApplicationName, _machineName);
                    }
                    else if (!sourceAlreadyExists)
                    {
                        CreateEventSource(ApplicationName, LogName, _machineName);

                        registeredLogName = EventLog.LogNameFromSourceName(ApplicationName, _machineName);
                    }
                }

                _levelMapping.ActivateOptions();

                LogLog.Debug(DeclaringType,
                    "Source [" + ApplicationName + "] is registered to log [" + registeredLogName + "]");
            }
            catch (SecurityException ex)
            {
                ErrorHandler.Error(
                    "Caught a SecurityException trying to access the EventLog.  Most likely the event source "
                    + ApplicationName
                    + " doesn't exist and must be created by a local administrator.  Will disable EventLogAppender."
                    + "  See http://logging.apache.org/log4net/release/faq.html#trouble-EventLog",
                    ex);
                Threshold = Level.Off;
            }
        }

        /// <summary>
        ///     Create an event log source
        /// </summary>
        private static void CreateEventSource(string source, string logName, string machineName)
        {
            var eventSourceCreationData = new EventSourceCreationData(source, logName) { MachineName = machineName };
            EventLog.CreateEventSource(eventSourceCreationData);
        }

        /// <summary>
        ///     This method is called by the <see cref="M:AppenderSkeleton.DoAppend(LoggingEvent)" />
        ///     method.
        /// </summary>
        /// <param name="loggingEvent">the event to log</param>
        /// <remarks>
        ///     <para>
        ///         Writes the event to the system event log using the
        ///         <see cref="ApplicationName" />.
        ///     </para>
        ///     <para>
        ///         If the event has an <c>EventID</c> property (see <see cref="LoggingEvent.Properties" />)
        ///         set then this integer will be used as the event log event id.
        ///     </para>
        ///     <para>
        ///         There is a limit of 32K characters for an event log message
        ///     </para>
        /// </remarks>
        protected override void Append(LoggingEvent loggingEvent)
        {
            //
            // Write the resulting string to the event log system
            //
            var eventId = EventId;

            // Look for the EventID property
            var eventIdPropertyObj = loggingEvent.LookupProperty("EventID");
            if (eventIdPropertyObj != null)
            {
                if (eventIdPropertyObj is int i)
                {
                    eventId = i;
                }
                else
                {
                    if (!(eventIdPropertyObj is string eventIdPropertyString))
                        eventIdPropertyString = eventIdPropertyObj.ToString();
                    if (!string.IsNullOrEmpty(eventIdPropertyString))
                    {
                        // Read the string property into a number
                        if (SystemInfo.TryParse(eventIdPropertyString, out int intVal))
                            eventId = intVal;
                        else
                            ErrorHandler.Error("Unable to parse event ID property [" + eventIdPropertyString + "].");
                    }
                }
            }

            var category = Category;
            // Look for the Category property
            var categoryPropertyObj = loggingEvent.LookupProperty("Category");
            if (categoryPropertyObj != null)
            {
                if (categoryPropertyObj is short s)
                {
                    category = s;
                }
                else
                {
                    if (!(categoryPropertyObj is string categoryPropertyString))
                        categoryPropertyString = categoryPropertyObj.ToString();
                    if (!string.IsNullOrEmpty(categoryPropertyString))
                    {
                        // Read the string property into a number
                        if (SystemInfo.TryParse(categoryPropertyString, out short shortVal))
                            category = shortVal;
                        else
                            ErrorHandler.Error("Unable to parse event category property [" + categoryPropertyString +
                                               "].");
                    }
                }
            }

            // Write to the event log
            try
            {
                var eventTxt = RenderLoggingEvent(loggingEvent);

                // There is a limit of about 32K characters for an event log message
                if (eventTxt.Length > MaxEventlogMessageSize) eventTxt = eventTxt.Substring(0, MaxEventlogMessageSize);

                var entryType = GetEntryType(loggingEvent.Level);

                using (SecurityContext.Impersonate(this))
                {
                    EventLog.WriteEntry(ApplicationName, eventTxt, entryType, eventId, category);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Error(
                    "Unable to write to event log [" + LogName + "] using source [" + ApplicationName + "]", ex);
            }
        }

        /// <summary>
        ///     Get the equivalent <see cref="EventLogEntryType" /> for a <see cref="Level" /> <paramref name="level" />
        /// </summary>
        /// <param name="level">the Level to convert to an EventLogEntryType</param>
        /// <returns>The equivalent <see cref="EventLogEntryType" /> for a <see cref="Level" /> <paramref name="level" /></returns>
        /// <remarks>
        ///     Because there are fewer applicable <see cref="EventLogEntryType" />
        ///     values to use in logging levels than there are in the
        ///     <see cref="Level" /> this is a one way mapping. There is
        ///     a loss of information during the conversion.
        /// </remarks>
        protected virtual EventLogEntryType GetEntryType(Level level)
        {
            // see if there is a specified lookup.
            if (_levelMapping.Lookup(level) is Level2EventLogEntryType entryType) return entryType.EventLogEntryType;

            // Use default behavior

            if (level >= Level.Error)
                return EventLogEntryType.Error;
            if (level == Level.Warn) return EventLogEntryType.Warning;

            // Default setting
            return EventLogEntryType.Information;
        }

        /// <summary>
        ///     This method determines the maximum event log message size allowed for
        ///     the current environment.
        /// </summary>
        /// <returns></returns>
        private static int GetMaxEventLogMessageSize()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6)
                return MaxEventlogMessageSizeVistaOrNewer;
            return MaxEventlogMessageSizeDefault;
        }

        /// <summary>
        ///     A class to act as a mapping between the level that a logging call is made at and
        ///     the color it should be displayed as.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Defines the mapping between a level and its event log entry type.
        ///     </para>
        /// </remarks>
        public class Level2EventLogEntryType : LevelMappingEntry
        {
            /// <summary>
            ///     The <see cref="EventLogEntryType" /> for this entry
            /// </summary>
            /// <remarks>
            ///     <para>
            ///         Required property.
            ///         The <see cref="EventLogEntryType" /> for this entry
            ///     </para>
            /// </remarks>
            public EventLogEntryType EventLogEntryType { get; set; }
        }
    }
}
