/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Common.Serializers;
using Dev2.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Warewolf.Interfaces.Auditing;

namespace Warewolf.Auditing
{
    [Table(Name = "Audit")]
    [DataContract(Name = "Audit", Namespace = "")]
    public class Audit : IAudit
    {
        [Column(Name = "Id", IsDbGenerated = true, DbType = "Integer", IsPrimaryKey = true)]
        [Key]
        [JsonProperty("Id")]
        [DataMember]
        public int Id { get; set; }

        [Column(Name = "WorkflowID", CanBeNull = true)]
        [JsonProperty("WorkflowID")]
        [DataMember]
        public string WorkflowID { get; set; }

        [Column(Name = "ExecutionID", CanBeNull = true)]
        [JsonProperty("ExecutionID")]
        [DataMember]
        public string ExecutionID { get; set; }
        
        [Column(Name = "CustomTransactionID", CanBeNull = true)]
        [JsonProperty("CustomTransactionID")]
        [DataMember]
        public string CustomTransactionID { get; set; }
        
        [Column(Name = "ExecutionOrigin", CanBeNull = true)]
        [JsonProperty("ExecutionOrigin")]
        [DataMember]
        public long ExecutionOrigin { get; set; }

        [Column(Name = "IsSubExecution", CanBeNull = true)]
        [JsonProperty("IsSubExecution")]
        [DataMember]
        public bool IsSubExecution { get; set; }

        [Column(Name = "IsRemoteWorkflow", CanBeNull = true)]
        [JsonProperty("IsRemoteWorkflow")]
        [DataMember]
        public bool IsRemoteWorkflow { get; set; }

        [Column(Name = "WorkflowName", CanBeNull = true)]
        [JsonProperty("WorkflowName")]
        [DataMember]
        public string WorkflowName { get; set; }

        [Column(Name = "AuditType", CanBeNull = true)]
        [JsonProperty("AuditType")]
        [DataMember]
        public string AuditType { get; set; }

        [Column(Name = "PreviousActivity", CanBeNull = true)]
        [JsonProperty("PreviousActivity")]
        [DataMember]
        public string PreviousActivity { get; set; }

        [Column(Name = "PreviousActivityType", CanBeNull = true)]
        [JsonProperty("PreviousActivityType")]
        [DataMember]
        public string PreviousActivityType { get; set; }

        [Column(Name = "PreviousActivityID", CanBeNull = true)]
        [JsonProperty("PreviousActivityID")]
        [DataMember]
        public string PreviousActivityId { get; set; }

        [Column(Name = "NextActivity", CanBeNull = true)]
        [JsonProperty("NextActivity")]
        [DataMember]
        public string NextActivity { get; set; }
        [Column(Name = "NextActivityType", CanBeNull = true)]
        [JsonProperty("NextActivityType")]
        [DataMember]
        public string NextActivityType { get; set; }
        [Column(Name = "NextActivityID", CanBeNull = true)]
        [JsonProperty("NextActivityID")]
        [DataMember]
        public string NextActivityId { get; set; }

        [JsonProperty("ServerID")]
        [DataMember]
        public string ServerID { get; set; }

        [Column(Name = "ParentID", CanBeNull = true)]
        [JsonProperty("ParentID")]
        [DataMember]
        public string ParentID { get; set; }

        [Column(Name = "ExecutingUser", CanBeNull = true)]
        [JsonProperty("ExecutingUser")]
        [DataMember]
        public string ExecutingUser { get; set; }

        [Column(Name = "ExecutionOriginDescription", CanBeNull = true)]
        [JsonProperty("ExecutionOriginDescription")]
        [DataMember]
        public string ExecutionOriginDescription { get; set; }

        [Column(Name = "ExecutionToken", CanBeNull = true)]
        [JsonProperty("ExecutionToken")]
        [DataMember]
        public string ExecutionToken { get; set; }

        [Column(Name = "AdditionalDetail", CanBeNull = true)]
        [JsonProperty("AdditionalDetail")]
        [DataMember]
        public string AdditionalDetail { get; set; }

        [Column(Name = "Environment", CanBeNull = true)]
        [JsonProperty("Environment")]
        [DataMember]
        public string Environment { get; set; }

        [Column(Name = "VersionNumber", CanBeNull = true)]
        [JsonProperty("VersionNumber")]
        [DataMember]
        public string VersionNumber { get; set; }

        [Column(Name = "AuditDate", DbType = "string", CanBeNull = true)]
        [JsonProperty("AuditDate")]
        [DataMember]
        public DateTime AuditDate { get; set; }

        [Column(Name = "Url", DbType = "string", CanBeNull = true)]
        [JsonProperty("Url")]
        [DataMember]
        public string Url { get; set; }
        [Column(Name = "User", CanBeNull = true)]
        [JsonProperty("User")]
        [DataMember]
        public string User { get; set; }
        [Column(Name = "Status", CanBeNull = true)]
        [JsonProperty("Status")]
        [DataMember]
        public string Status { get; set; }
        [Column(Name = "ExecutionTime", CanBeNull = true)]
        [JsonProperty("ExecutionTime")]
        [DataMember]
        public string ExecutionTime { get; set; }
        [Column(Name = "Exception", CanBeNull = true)]
        [JsonProperty("Exception")]
        [DataMember]
        public Exception Exception { get; set; }

        [JsonProperty("StartDateTime")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime StartDateTime { get; set; }

        [JsonProperty("CompletedDateTime")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime CompletedDateTime { get; set; }
        public static Expression<Func<Audit, bool>> EqualsAuditType(string searchTerm)
        {
            return p => (string.IsNullOrEmpty(searchTerm) || p.AuditType == searchTerm);
        }


        public static Expression<Func<Audit, bool>> IsAuditDateLessThan(DateTime dateValue)
        {
            return p => (dateValue == null || p.AuditDate <= dateValue);
        }
        public Audit() { }
        public Audit(IDSFDataObject dsfDataObject, string auditType, string detail, IDev2Activity previousActivity, IDev2Activity nextActivity)
            : this(dsfDataObject, auditType, detail, previousActivity, nextActivity, null)
        {

        }

        public Audit(IDSFDataObject dsfDataObject, string auditType, string detail, IDev2Activity previousActivity, IDev2Activity nextActivity, Exception exception)
        {
            var dev2Serializer = new Dev2JsonSerializer();
            WorkflowID = dsfDataObject.ResourceID.ToString();
            ExecutionID = dsfDataObject.ExecutionID.ToString();
            CustomTransactionID = dsfDataObject.CustomTransactionID;
            ExecutionOrigin = Convert.ToInt64(dsfDataObject.ExecutionOrigin);
            IsSubExecution = Convert.ToBoolean(dsfDataObject.IsSubExecution);
            IsRemoteWorkflow = Convert.ToBoolean(dsfDataObject.IsRemoteWorkflow());
            WorkflowName = dsfDataObject.ServiceName;
            ServerID = dsfDataObject.ServerID.ToString();
            ParentID = dsfDataObject.ParentID.ToString();
            ExecutingUser = dsfDataObject.ExecutingUser?.ToString();
            User = dsfDataObject.ExecutingUser?.ToString();
            ExecutionOriginDescription = dsfDataObject.ExecutionOriginDescription;
            ExecutionToken = dev2Serializer.Serialize(ExecutionToken);
            Environment = dsfDataObject.Environment.ToJson();
            VersionNumber = dsfDataObject.VersionNumber.ToString();
            AuditDate = DateTime.Now;
            StartDateTime = DateTime.Now;
            CompletedDateTime = DateTime.Now;
            Url = dsfDataObject.WebUrl;
            AuditType = auditType;
            AdditionalDetail = detail;
            Exception = exception;

            if (previousActivity != null)
            {
                PreviousActivity = previousActivity.GetDisplayName();
                PreviousActivityType = previousActivity.GetType().ToString();
                PreviousActivityId = previousActivity.UniqueID;
            }
            if (nextActivity != null)
            {
                NextActivity = nextActivity.GetDisplayName();
                NextActivityType = nextActivity.GetType().ToString();
                NextActivityId = nextActivity.UniqueID;
            }
        }
    }
}
