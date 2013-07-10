using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Security;
using System.IO;
using Dev2.Common;

namespace Dev2 {
    public static class ExceptionHandling {
        public static void WriteEventLogEntry(
            string targetLog, 
            string source, 
            string message, 
            System.Diagnostics.EventLogEntryType eventType) {
            try {
                if (!EventLog.SourceExists(source)) {
                    try {
                        EventLog.CreateEventSource(source, targetLog);

                    }
                    catch (ArgumentException argEx) {
                        throw argEx;
                    }
                    catch (InvalidOperationException invalidOpEx) {
                        throw invalidOpEx;
                    }
                }


                try {
                    EventLog.WriteEntry(source, message, eventType);
                }
                catch {
                    if (!Directory.Exists("Log")) {
                        Directory.CreateDirectory("Log");
                    }
                    File.WriteAllText(string.Format("{0}\\{1}", "Log", DateTime.Now.ToString("yyyyMMddhhmmssfff")), message);
                }
            }
            catch (SecurityException securityEx) {
                ServerLogger.LogError(securityEx);
                throw securityEx;
            }
        }
    }
}
