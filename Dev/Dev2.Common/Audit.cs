using Newtonsoft.Json;

namespace Dev2.Common
{
    public class Audit
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
        [JsonProperty("WorkflowID")]
        public string WorkflowID { get; set; }
        [JsonProperty("WorkflowName")]
        public string WorkflowName { get; set; }
        [JsonProperty("ExecutionID")]
        public string ExecutionID { get; set; }
        [JsonProperty("AuditType")]
        public string AuditType { get; set; }
        [JsonProperty("PreviousActivity")]
        public string PreviousActivity { get; set; }
        [JsonProperty("PreviousActivityType")]
        public string PreviousActivityType { get; set; }
        [JsonProperty("PreviousActivityID")]
        public string PreviousActivityID { get; set; }
        [JsonProperty("NextActivity")]
        public string NextActivity { get; set; }
        [JsonProperty("NextActivityType")]
        public string NextActivityType { get; set; }
        [JsonProperty("NextActivityID")]
        public string NextActivityID { get; set; }
        [JsonProperty("ServerID")]
        public string ServerID { get; set; }
        [JsonProperty("ParentID")]
        public string ParentID { get; set; }
        [JsonProperty("ClientID")]
        public string ClientID { get; set; }
        [JsonProperty("ExecutingUser")]
        public string ExecutingUser { get; set; }
        [JsonProperty("ExecutionOrigin")]
        public int ExecutionOrigin { get; set; }
        [JsonProperty("ExecutionOriginDescription")]
        public string ExecutionOriginDescription { get; set; }
        [JsonProperty("ExecutionToken")]
        public string ExecutionToken { get; set; }
        [JsonProperty("AdditionalDetail")]
        public string AdditionalDetail { get; set; }
        [JsonProperty("IsSubExecution")]
        public int IsSubExecution { get; set; }
        [JsonProperty("IsRemoteWorkflow")]
        public int IsRemoteWorkflow { get; set; }
        [JsonProperty("Environment")]
        public string Environment { get; set; }
        [JsonProperty("AuditDate")]
        public string AuditDate { get; set; }
    }
}
