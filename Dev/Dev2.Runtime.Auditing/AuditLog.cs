#pragma warning disable
﻿using System;
using Dev2.Interfaces;
using System.Data.Linq.Mapping;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Dev2.Communication;
using System.Linq.Expressions;
using System.Linq;

namespace Dev2.Runtime.Auditing
{
    [Table(Name = "AuditLog")]
    [DataContract(Name = "AuditLog", Namespace = "")]
    public class AuditLog
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

        [Column(Name = "ServerID", CanBeNull = true)]
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

        public static Expression<Func<AuditLog, bool>> EqualsAuditType(string searchTerm)
        {
            return p => (string.IsNullOrEmpty(searchTerm) || p.AuditType == searchTerm);
        }


        public static Expression<Func<AuditLog, bool>> IsAuditDateLessThan(DateTime dateValue)
        {
            return p => (dateValue == null || p.AuditDate <= dateValue);
        }
        public AuditLog() { }
        public AuditLog(IDSFDataObject dsfDataObject, string auditType, string detail, IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            var dev2Serializer = new Dev2JsonSerializer();
            WorkflowID = dsfDataObject.ResourceID.ToString();
            ExecutionID = dsfDataObject.ExecutionID.ToString();
            ExecutionOrigin = Convert.ToInt64(dsfDataObject.ExecutionOrigin);
            IsSubExecution = Convert.ToBoolean(dsfDataObject.IsSubExecution);
            IsRemoteWorkflow = Convert.ToBoolean(dsfDataObject.IsRemoteWorkflow());
            WorkflowName = dsfDataObject.ServiceName;
            ServerID = dsfDataObject.ServerID.ToString();
            ParentID = dsfDataObject.ParentID.ToString();
            ExecutingUser = dsfDataObject.ExecutingUser?.ToString();
            ExecutionOriginDescription = dsfDataObject.ExecutionOriginDescription;
            ExecutionToken = dev2Serializer.Serialize(ExecutionToken);
            Environment = dsfDataObject.Environment.ToJson();
            VersionNumber = dsfDataObject.VersionNumber.ToString();
            AuditDate = DateTime.Now;
            AuditType = auditType;
            AdditionalDetail = detail;
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
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
