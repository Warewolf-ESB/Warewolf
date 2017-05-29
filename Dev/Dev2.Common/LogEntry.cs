using System;

namespace Dev2.Common
{
    public class LogEntry
    {
        public DateTime StartDateTime { get; set; }
        public string Status { get; set; }
        public string Url { get; set; }
        public string Result { get; set; }
        public string User { get; set; }
        public DateTime CompletedDateTime { get; set; }
        public string ExecutionTime { get; set; }
        public string ExecutionId { get; set; }
    }
}