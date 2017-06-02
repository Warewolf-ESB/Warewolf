using Dev2.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dev2.Web2.Models.ExecutionLogging
{
    public class ExecutionLoggingRequestViewModel: LogEntry
    {
        public string Server { get; set; }
        public string Port { get; set; }
        public string Protocol { get; set; }
        public string SortBy { get; set; }
    }
}