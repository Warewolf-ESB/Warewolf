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
        [DataMember]
        public int Id { get; set; }

        [JsonProperty]
        [Column(Name = "WorkflowID", CanBeNull = true)]
        [DataMember]
        public string WorkflowID { get; set; }

        [JsonProperty]
        [Column(Name = "ExecutionID", CanBeNull = true)]
        [DataMember]
        public string ExecutionID { get; set; }

        [JsonProperty]
        [Column(Name = "ExecutionOrigin", CanBeNull = true)]
        [DataMember]
        public long ExecutionOrigin { get; set; }

        [JsonProperty]
        [Column(Name = "IsSubExecution", CanBeNull = true)]
        [DataMember]
        public long IsSubExecution { get; set; }

        [JsonProperty]
        [Column(Name = "IsRemoteWorkflow", CanBeNull = true)]
        [DataMember]
        public long IsRemoteWorkflow { get; set; }

        [JsonProperty]
        [DataMember]
        [Column(Name = "WorkflowName", CanBeNull = true)]
        public string WorkflowName { get; set; }

        [JsonProperty]
        [Column(Name = "AuditType", CanBeNull = true)]
        [DataMember]
        public string AuditType { get; set; }

        [JsonProperty]
        [Column(Name = "PreviousActivity", CanBeNull = true)]
        [DataMember]
        public string PreviousActivity { get; set; }

        [JsonProperty]
        [Column(Name = "PreviousActivityType", CanBeNull = true)]
        [DataMember]
        public string PreviousActivityType { get; set; }

        [JsonProperty]
        [Column(Name = "PreviousActivityID", CanBeNull = true)]
        [DataMember]
        public string PreviousActivityId { get; set; }

        [JsonProperty]
        [Column(Name = "NextActivity", CanBeNull = true)]
        [DataMember]
        public string NextActivity { get; set; }

        [JsonProperty]
        [Column(Name = "NextActivityType", CanBeNull = true)]
        [DataMember]
        public string NextActivityType { get; set; }

        [JsonProperty]
        [Column(Name = "NextActivityID", CanBeNull = true)]
        [DataMember]
        public string NextActivityId { get; set; }

        [JsonProperty]
        [Column(Name = "ServerID", CanBeNull = true)]
        [DataMember]
        public string ServerID { get; set; }

        [JsonProperty]
        [Column(Name = "ParentID", CanBeNull = true)]
        [DataMember]
        public string ParentID { get; set; }

        [JsonProperty]
        [Column(Name = "ExecutingUser", CanBeNull = true)]
        [DataMember]
        public string ExecutingUser { get; set; }

        [JsonProperty]
        [Column(Name = "ExecutionOriginDescription", CanBeNull = true)]
        [DataMember]
        public string ExecutionOriginDescription { get; set; }

        [JsonProperty]
        [Column(Name = "ExecutionToken", CanBeNull = true)]
        [DataMember]
        public string ExecutionToken { get; set; }

        [JsonProperty]
        [Column(Name = "AdditionalDetail", CanBeNull = true)]
        [DataMember]
        public string AdditionalDetail { get; set; }

        [JsonProperty]
        [Column(Name = "Environment", CanBeNull = true)]
        [DataMember]
        public string Environment { get; set; }

        [JsonProperty]
        [Column(Name = "AuditDate", CanBeNull = true)]
        [DataMember]
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


        public override bool Equals(object obj)
        {
            if (obj is AuditLog auditLog)
            {
                return Equals(auditLog);
            }
            return false;
        }
        public bool Equals(AuditLog other) {
            var eq = true;
            eq &= Id == other.Id;
            eq &= WorkflowID == other.WorkflowID;
            eq &= ExecutionID == other.ExecutionID;
            eq &= ExecutionOrigin == other.ExecutionOrigin;
            eq &= IsSubExecution == other.IsSubExecution;
            eq &= IsRemoteWorkflow == other.IsRemoteWorkflow;
            eq &= WorkflowName == other.WorkflowName;
            eq &= AuditType == other.AuditType;
            eq &= PreviousActivity == other.PreviousActivity;
            eq &= PreviousActivityType == other.PreviousActivityType;
            eq &= PreviousActivityId == other.PreviousActivityId;
            eq &= NextActivity == other.NextActivity;
            eq &= NextActivityType == other.NextActivityType;
            eq &= NextActivityId == other.NextActivityId;
            eq &= ServerID == other.ServerID;
            eq &= ParentID == other.ParentID;
            eq &= ExecutingUser == other.ExecutingUser;
            eq &= ExecutionOriginDescription == other.ExecutionOriginDescription;
            eq &= ExecutionToken == other.ExecutionToken;
            eq &= AdditionalDetail == other.AdditionalDetail;
            eq &= Environment == other.Environment;
            eq &= AuditDate == other.AuditDate;
            return eq;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 1561563491;

                hashCode = (hashCode * 157) + Id.GetHashCode();
                hashCode = (hashCode * 157) + WorkflowID.GetHashCode();
                hashCode = (hashCode * 157) + ExecutionID.GetHashCode();
                hashCode = (hashCode * 157) + ExecutionOrigin.GetHashCode();
                hashCode = (hashCode * 157) + IsSubExecution.GetHashCode();
                hashCode = (hashCode * 157) + IsRemoteWorkflow.GetHashCode();
                hashCode = (hashCode * 157) + WorkflowName.GetHashCode();
                hashCode = (hashCode * 157) + AuditType.GetHashCode();
                hashCode = (hashCode * 157) + PreviousActivity.GetHashCode();
                hashCode = (hashCode * 157) + PreviousActivityType.GetHashCode();
                hashCode = (hashCode * 157) + PreviousActivityId.GetHashCode();
                hashCode = (hashCode * 157) + NextActivity.GetHashCode();
                hashCode = (hashCode * 157) + NextActivityType.GetHashCode();
                hashCode = (hashCode * 157) + NextActivityId.GetHashCode();
                hashCode = (hashCode * 157) + ServerID.GetHashCode();
                hashCode = (hashCode * 157) + ParentID.GetHashCode();
                hashCode = (hashCode * 157) + ExecutingUser.GetHashCode();
                hashCode = (hashCode * 157) + ExecutionOriginDescription.GetHashCode();
                hashCode = (hashCode * 157) + ExecutionToken.GetHashCode();
                hashCode = (hashCode * 157) + AdditionalDetail.GetHashCode();
                hashCode = (hashCode * 157) + Environment.GetHashCode();
                hashCode = (hashCode * 157) + AuditDate.GetHashCode();

                return hashCode;
            }
        }
    }
}
