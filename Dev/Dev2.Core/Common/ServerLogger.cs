using System;
using System.IO;

namespace Dev2.Common
{
    /// <summary>
    /// A single common logging location ;)
    /// </summary>
    public static class ServerLogger
    {
        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogMessage(string message)
        {
            try
            {
                // REMOVE
                File.AppendAllText(@"f:\foo\log.txt",
                                   string.Format("{0} :: {1}{2}", DateTime.Now, message, Environment.NewLine));
                
                File.AppendAllText(Path.Combine(EnvironmentVariables.ApplicationPath, "ServerLog.txt"),
                                   string.Format("{0} :: {1}{2}", DateTime.Now, message, Environment.NewLine));

                
            }
            catch
            {
                // We do not care, best effort 
            }
        }
    }
}
