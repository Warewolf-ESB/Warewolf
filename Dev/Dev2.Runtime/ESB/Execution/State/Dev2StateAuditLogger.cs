using System;
using System.IO;
using Newtonsoft.Json;
using Dev2.Interfaces;
using Dev2.Common;
using Dev2.Communication;
using System.Runtime.Serialization;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dev2.Runtime.ESB.Execution
{
    class Dev2StateAuditLogger : IStateListener
    {
        readonly IDSFDataObject _dsfDataObject;
        public static List<IAuditFilter> Filters { get; private set; } = new List<IAuditFilter>
        {
            new AllPassFilter()
        };

        public Dev2StateAuditLogger(IDSFDataObject dsfDataObject)
        {
            _dsfDataObject = dsfDataObject;
        }
        public static IEnumerable<AuditLog> Query(Expression<Func<AuditLog, bool>> queryExpression)
        {
            var database = GetDatabase();
            return database.Table<AuditLog>().Where(queryExpression).AsEnumerable();
        }

        private static SQLiteConnection GetDatabase()
        {
            var filePath = Path.Combine(EnvironmentVariables.AppDataPath, "Audits", "auditDB.db");
            var database = new SQLiteConnection(filePath);
            return database;
        }

        public static void ClearAuditLog()
        {
            var database = GetDatabase();
            database.Table<AuditLog>().Delete(item => true);
        }
        public void LogAdditionalDetail(object detail, string callerName)
        {
            var serializer = new Dev2JsonSerializer();
            var auditLog = new AuditLog(_dsfDataObject, "LogAdditionalDetail", serializer.Serialize(detail, Formatting.None), null, null);
            if (!FilterAuditLog(auditLog, detail))
            {
                return;
            }
            LogAuditState(auditLog);
        }

        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogPreExecuteState", null, null, nextActivity);
            if (!FilterAuditLog(auditLog, nextActivity))
            {
                return;
            }
            LogAuditState(auditLog);
        }
        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogPostExecuteState", null, previousActivity, nextActivity);
            if (!FilterAuditLog(auditLog, previousActivity, nextActivity))
            {
                return;
            }
            LogAuditState(auditLog);
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogExecuteException", e.Message, activity, null);
            if (!FilterAuditLog(auditLog, activity))
            {
                return;
            }
            LogAuditState(auditLog);
        }

        public void LogExecuteCompleteState(IDev2Activity activity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogExecuteCompleteState", null, activity, null);
            if (!FilterAuditLog(auditLog, activity))
            {
                return;
            }
            LogAuditState(auditLog);
        }

        public void LogStopExecutionState(IDev2Activity activity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogStopExecutionState", null, activity, null);
            if (!FilterAuditLog(auditLog, activity))
            {
                return;
            }
            LogAuditState(auditLog);
        }

        public static void LogAuditState(AuditLog auditLog)
        {
            InsertLog(auditLog, 3);
        }

        private static void InsertLog(AuditLog auditLog, int reTry)
        {

            try
            {
                var filePath = Path.Combine(EnvironmentVariables.AppDataPath, "Audits", "auditDB.db");
                var database = new SQLiteConnection(filePath);
                database.Insert(auditLog);
                database.Close();
            }
            catch (SQLiteException e)
            {
                if (reTry == 0)
                {
                    throw new Exception(e.Message);
                }
                reTry--;
                InsertLog(auditLog, reTry);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static void AddFilter(IAuditFilter filter)
        {
            Filters.Add(filter);
        }
        public static void RemoveFilter(IAuditFilter filter)
        {
            Filters.Remove(filter);
        }
        public static bool FilterAuditLog(AuditLog auditLog, IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            var ret = FilterAuditLog(auditLog, previousActivity);
            ret |= FilterAuditLog(auditLog, nextActivity);
            return ret;
        }

        public static bool FilterAuditLog(AuditLog auditLog, object detail)
        {
            foreach (var filter in Filters)
            {
                var pass = filter.FilterDetailLogEntry(auditLog, detail);
                if (pass)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool FilterAuditLog(AuditLog auditLog, IDev2Activity activity)
        {
            foreach (var filter in Filters)
            {
                var pass = filter.FilterLogEntry(auditLog, activity);
                if (pass)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool FilterAuditLog(AuditLog auditLog)
        {
            foreach (var filter in Filters)
            {
                var pass = filter.FilterLogEntry(auditLog);
                if (pass)
                {
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {

        }
    }

    [Table("AuditLog")]
    [DataContract(Name = "AuditLog", Namespace = "")]
    public class AuditLog
    {
        [Column("Id")]
        [AutoIncrement, PrimaryKey, Ignore]
        public int Id { get; set; }

        [Column("WorkflowID")]
        public string WorkflowID { get; set; }

        [Column("ExecutionID")]
        public string ExecutionID { get; set; }

        [Column("ExecutionOrigin")]
        public long ExecutionOrigin { get; set; }

        [Column("IsSubExecution")]
        public long IsSubExecution { get; set; }

        [Column("IsRemoteWorkflow")]
        public long IsRemoteWorkflow { get; set; }

        [Column("WorkflowName")]
        public string WorkflowName { get; set; }

        [Column("AuditType")]
        public string AuditType { get; set; }

        [Column("PreviousActivity")]
        public string PreviousActivity { get; set; }
        [Column("PreviousActivityType")]
        public string PreviousActivityType { get; set; }
        [Column("PreviousActivityID")]
        public string PreviousActivityId { get; set; }

        [Column("NextActivity")]
        public string NextActivity { get; set; }
        [Column("NextActivityType")]
        public string NextActivityType { get; set; }
        [Column("NextActivityID")]
        public string NextActivityId { get; set; }

        [Column("ServerID")]
        public string ServerID { get; set; }

        [Column("ParentID")]
        public string ParentID { get; set; }

        [Column("ExecutingUser")]
        public string ExecutingUser { get; set; }

        [Column("ExecutionOriginDescription")]
        public string ExecutionOriginDescription { get; set; }

        [Column("ExecutionToken")]
        public string ExecutionToken { get; set; }

        [Column("AdditionalDetail")]
        public string AdditionalDetail { get; set; }

        [Column("Environment")]
        public string Environment { get; set; }

        [Column("AuditDate")]
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

    public interface IAuditFilter
    {
        bool FilterLogEntry(AuditLog log, IDev2Activity activity);
        bool FilterDetailLogEntry(AuditLog auditLog, object detail);
        bool FilterLogEntry(AuditLog auditLog);
    }

    public class AllPassFilter : IAuditFilter
    {
        public bool FilterDetailLogEntry(AuditLog auditLog, object detail) => true;
        public bool FilterLogEntry(AuditLog log, IDev2Activity activity) => true;
        public bool FilterLogEntry(AuditLog auditLog) => true;
    }

    public class ActivityAuditFilter : IAuditFilter {
        readonly string _activityId;
        readonly string _activityType;
        readonly string _activityDisplayName;
        public ActivityAuditFilter(string activityId, string activityType, string activityDisplayName) {
            _activityId = activityId;
            _activityType = activityType;
            _activityDisplayName = activityDisplayName;
        }

        public bool FilterLogEntry(AuditLog log, IDev2Activity activity)
        {
            if (log.PreviousActivityId.Equals(_activityId)
                || log.PreviousActivityType.Equals(_activityType)
                || log.PreviousActivity.Equals(_activityDisplayName))
            {
                return true;
            }
            if (log.NextActivityId.Equals(_activityId)
                || log.NextActivityType.Equals(_activityType)
                || log.NextActivity.Equals(_activityDisplayName))
            {
                return true;
            }
            return false;
        }

        public bool FilterDetailLogEntry(AuditLog auditLog, object detail)
        {
            return FilterLogEntry(auditLog, null);
        }
        public bool FilterLogEntry(AuditLog auditLog)
        {
            return FilterLogEntry(auditLog, null);
        }
    }

    public class WorkflowAuditFilter : IAuditFilter
    {
        readonly string _workflowId;
        readonly string _workflowName;
        public WorkflowAuditFilter(string workflowId, string workflowName)
        {
            _workflowId = workflowId;
            _workflowName = workflowName;
        }

        public bool FilterLogEntry(AuditLog log, IDev2Activity activity)
        {
            if (log.WorkflowID.Equals(_workflowId)
                || log.WorkflowName.Equals(_workflowName))
            {
                return true;
            }
            return false;
        }

        public bool FilterDetailLogEntry(AuditLog auditLog, object detail)
        {
            return FilterLogEntry(auditLog, null);
        }
        public bool FilterLogEntry(AuditLog auditLog)
        {
            return FilterLogEntry(auditLog, null);
        }
    }
}
