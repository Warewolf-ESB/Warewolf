using System;
using Dev2.Interfaces;
using System.Runtime.Serialization;
using System.Data.Linq.Mapping;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Execution
{
    [Table(Name = "AuditLog")]
    [DataContract(Name = "AuditLog", Namespace = "")]
    public class AuditLog
    {
        [JsonProperty]
        [Column(Name = "Id", IsDbGenerated = true, DbType = "Integer", IsPrimaryKey = true)]
        [Key]
        public int Id { get; set; }

        [JsonProperty]
        [Column(Name = "WorkflowID", CanBeNull = true)]
        public string WorkflowID { get; set; }

        [JsonProperty]
        [Column(Name = "ExecutionID", CanBeNull = true)]
        public string ExecutionID { get; set; }

        [JsonProperty]
        [Column(Name = "ExecutionOrigin", CanBeNull = true)]
        public long ExecutionOrigin { get; set; }

        [JsonProperty]
        [Column(Name = "IsSubExecution", CanBeNull = true)]
        public long IsSubExecution { get; set; }

        [JsonProperty]
        [Column(Name = "IsRemoteWorkflow", CanBeNull = true)]
        public long IsRemoteWorkflow { get; set; }

        [JsonProperty]
        [Column(Name = "WorkflowName", CanBeNull = true)]
        public string WorkflowName { get; set; }

        [JsonProperty]
        [Column(Name = "AuditType", CanBeNull = true)]
        public string AuditType { get; set; }

        [JsonProperty]
        [Column(Name = "PreviousActivity", CanBeNull = true)]
        public string PreviousActivity { get; set; }

        [JsonProperty]
        [Column(Name = "PreviousActivityType", CanBeNull = true)]
        public string PreviousActivityType { get; set; }

        [JsonProperty]
        [Column(Name = "PreviousActivityID", CanBeNull = true)]
        public string PreviousActivityId { get; set; }

        [JsonProperty]
        [Column(Name = "NextActivity", CanBeNull = true)]
        public string NextActivity { get; set; }

        [JsonProperty]
        [Column(Name = "NextActivityType", CanBeNull = true)]
        public string NextActivityType { get; set; }

        [JsonProperty]
        [Column(Name = "NextActivityID", CanBeNull = true)]
        public string NextActivityId { get; set; }

        [JsonProperty]
        [Column(Name = "ServerID", CanBeNull = true)]
        public string ServerID { get; set; }

        [JsonProperty]
        [Column(Name = "ParentID", CanBeNull = true)]
        public string ParentID { get; set; }

        [JsonProperty]
        [Column(Name = "ExecutingUser", CanBeNull = true)]
        public string ExecutingUser { get; set; }

        [JsonProperty]
        [Column(Name = "ExecutionOriginDescription", CanBeNull = true)]
        public string ExecutionOriginDescription { get; set; }

        [JsonProperty]
        [Column(Name = "ExecutionToken", CanBeNull = true)]
        public string ExecutionToken { get; set; }

        [JsonProperty]
        [Column(Name = "AdditionalDetail", CanBeNull = true)]
        public string AdditionalDetail { get; set; }

        [JsonProperty]
        [Column(Name = "Environment", CanBeNull = true)]
        public string Environment { get; set; }

        [JsonProperty]
        [Column(Name = "AuditDate", CanBeNull = true)]
        public string AuditDate { get; set; }

        public AuditLog() { }
        public AuditLog(IDSFDataObject dsfDataObject, string auditType, string detail, IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            WorkflowID = dsfDataObject.ResourceID.ToString();
            ExecutionID = dsfDataObject.ExecutionID.ToString();
            ExecutionOrigin = Convert.ToInt64(dsfDataObject.ExecutionOrigin);
            IsSubExecution = Convert.ToInt64(dsfDataObject.IsSubExecution);
            IsRemoteWorkflow = Convert.ToInt64(dsfDataObject.IsRemoteWorkflow());
            WorkflowName = dsfDataObject.ServiceName;
            ServerID = dsfDataObject.ServerID.ToString();
            ParentID = dsfDataObject.ParentID.ToString();
            ExecutingUser = dsfDataObject.ExecutingUser?.ToString();
            ExecutionOriginDescription = dsfDataObject.ExecutionOriginDescription;
            ExecutionToken = dsfDataObject.ExecutionToken.ToJson();
            Environment = dsfDataObject.Environment.ToJson();
            AuditDate = DateTime.Now.ToString();
            AuditType = auditType;
            AdditionalDetail = detail;
            if (previousActivity != null)
            {
                PreviousActivity = previousActivity.GetDisplayName();
                PreviousActivityType = previousActivity.GetType().ToString();
                PreviousActivityId = previousActivity.ActivityId.ToString();
            }
            if (nextActivity != null)
            {
                NextActivity = nextActivity.GetDisplayName();
                NextActivityType = nextActivity.GetType().ToString();
                NextActivityId = nextActivity.ActivityId.ToString();
            }
        }
    }
}
