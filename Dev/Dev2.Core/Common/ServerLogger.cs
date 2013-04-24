using System;
using System.IO;

namespace Dev2.Common
{
    /// <summary>
    /// A single common logging location ;)
    /// </summary>
    public static class ServerLogger
    {

        // Configured via ServerLifeCycleManager ;)

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

        /// <summary>
        /// Gets or sets a value indicating whether [enable info output].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable info output]; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableInfoOutput { get; set; }

        
        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogMessage(string message)
        {
            if (EnableInfoOutput)
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
            if (EnableDebugOutput)
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
            if (EnableTraceOutput)
            {
                InternalLogMessage(message, "TRACE");
            }
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogError(string message)
        {
            if (EnableErrorOutput)
            {
                InternalLogMessage(message, "ERROR");
            }
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void InternalLogMessage(string message, string typeOf)
        {
            try
            {
                // REMOVE
                //File.AppendAllText(@"f:\foo\log.txt",
                //                   string.Format("{0} :: {1}{2}", DateTime.Now, message, Environment.NewLine));

                File.AppendAllText(Path.Combine(EnvironmentVariables.ApplicationPath, "ServerLog.txt"),
                                   string.Format("{0} :: {1} -> {2}{3}", DateTime.Now, typeOf, message, Environment.NewLine));


            }
            catch
            {
                // We do not care, best effort 
            }
        }

    }
}
