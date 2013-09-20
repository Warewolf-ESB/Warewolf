using System.Runtime.CompilerServices;
using Dev2.Providers.Logs;

namespace Dev2.Common
{
    /// <summary>
    /// Logger for the studio
    /// </summary>
    public static class StudioLogger
    {
        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="module"></param>
        public static void LogMessage(string message, [CallerMemberName]string module = null)
        {
            Logger.TraceInfo(message, module);
        }
    }
}
