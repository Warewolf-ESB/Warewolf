using Newtonsoft.Json;
using System;

namespace Dev2.Common
{
    public class LogEntry
    {
        [JsonProperty("StartDateTime")]
        public DateTime StartDateTime { get; set; }
        [JsonProperty("Status")]
        public string Status { get; set; }
        [JsonProperty("Url")]
        public string Url { get; set; }
        [JsonProperty("Result")]
        public string Result { get; set; }
        [JsonProperty("User")]
        public string User { get; set; }
        [JsonProperty("CompletedDateTime")]
        public DateTime CompletedDateTime { get; set; }
        [JsonProperty("ExecutionTime")]
        public string ExecutionTime { get; set; }
        [JsonProperty("ExecutionId")]
        public string ExecutionId { get; set; }
    }
}