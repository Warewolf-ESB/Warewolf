using System;
using Dev2.Interfaces;
using System.Runtime.Serialization;
using System.Data.Linq.Mapping;
using System.ComponentModel.DataAnnotations;

namespace Dev2.Runtime.ESB.Execution
{
    [Table(Name = "AuditLog")]
    [DataContract(Name = "AuditLog", Namespace = "")]
    public class AuditLog
    {
        [Column(Name = "Id", IsDbGenerated = true, DbType = "Integer", IsPrimaryKey = true)]
        [Key]
        public int Id { get; set; }

        [Column(Name = "WorkflowID", CanBeNull = true)]
        public string WorkflowID { get; set; }

        [Column(Name = "ExecutionID", CanBeNull = true)]
        public string ExecutionID { get; set; }

        [Column(Name = "ExecutionOrigin", CanBeNull = true)]
        public long ExecutionOrigin { get; set; }

        [Column(Name = "IsSubExecution", CanBeNull = true)]
        public long IsSubExecution { get; set; }

        [Column(Name = "IsRemoteWorkflow", CanBeNull = true)]
        public long IsRemoteWorkflow { get; set; }

        [Column(Name = "WorkflowName", CanBeNull = true)]
        public string WorkflowName { get; set; }

        [Column(Name = "AuditType", CanBeNull = true)]
        public string AuditType { get; set; }

        [Column(Name = "PreviousActivity", CanBeNull = true)]
        public string PreviousActivity { get; set; }
        [Column(Name = "PreviousActivityType", CanBeNull = true)]
        public string PreviousActivityType { get; set; }
        [Column(Name = "PreviousActivityID", CanBeNull = true)]
        public string PreviousActivityId { get; set; }

        [Column(Name = "NextActivity", CanBeNull = true)]
        public string NextActivity { get; set; }
        [Column(Name = "NextActivityType", CanBeNull = true)]
        public string NextActivityType { get; set; }
        [Column(Name = "NextActivityID", CanBeNull = true)]
        public string NextActivityId { get; set; }

        [Column(Name = "ServerID", CanBeNull = true)]
        public string ServerID { get; set; }

        [Column(Name = "ParentID", CanBeNull = true)]
        public string ParentID { get; set; }

        [Column(Name = "ExecutingUser", CanBeNull = true)]
        public string ExecutingUser { get; set; }

        [Column(Name = "ExecutionOriginDescription", CanBeNull = true)]
        public string ExecutionOriginDescription { get; set; }

        [Column(Name = "ExecutionToken", CanBeNull = true)]
        public string ExecutionToken { get; set; }

        [Column(Name = "AdditionalDetail", CanBeNull = true)]
        public string AdditionalDetail { get; set; }

        [Column(Name = "Environment", CanBeNull = true)]
        public string Environment { get; set; }

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
