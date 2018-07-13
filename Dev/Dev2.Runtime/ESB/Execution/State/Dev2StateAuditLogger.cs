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

        public Dev2StateAuditLogger(IDSFDataObject dsfDataObject)
        {
            _dsfDataObject = dsfDataObject;
        }
        public static IEnumerable<AuditLog> Query(Expression<Func<AuditLog, bool>> queryExpression)
        {
            SQLiteConnection database = GetDatabase();
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
            if (AuditFilter.FilterAuditLog(detail))
            {
                return;
            }
            var serializer = new Dev2JsonSerializer();
            var auditLog = new AuditLog(_dsfDataObject, "LogAdditionalDetail", serializer.Serialize(detail, Formatting.None), null, null);
            LogAuditState(auditLog);
        }
        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            if (AuditFilter.FilterAuditLog(previousActivity, nextActivity))
            {
                return;
            }
            var auditLog = new AuditLog(_dsfDataObject, "LogPostExecuteState", null, previousActivity, nextActivity);
            LogAuditState(auditLog);
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            if (AuditFilter.FilterAuditLog(activity))
            {
                return;
            }
            var auditLog = new AuditLog(_dsfDataObject, "LogExecuteException", e.Message, activity, null);
            LogAuditState(auditLog);
        }

        public void LogExecuteCompleteState()
        {
            if (AuditFilter.FilterAuditLog("LogStopExecutionState"))
            {
                return;
            }
            var auditLog = new AuditLog(_dsfDataObject, "LogStopExecutionState", null, null, null);
            LogAuditState(auditLog);
        }

        public void LogStopExecutionState()
        {
            if (AuditFilter.FilterAuditLog("LogStopExecutionState"))
            {
                return;
            }
            var auditLog = new AuditLog(_dsfDataObject, "LogStopExecutionState", null, null, null);
            LogAuditState(auditLog);
        }

        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
            if (AuditFilter.FilterAuditLog(nextActivity))
            {
                return;
            }
            var auditLog = new AuditLog(_dsfDataObject, "LogPreExecuteState", null, null, nextActivity);
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

        public List<AuditLog> ReturnLogAudit(AuditLog auditLog)
        {
            try
            {
                var filePath = Path.Combine(EnvironmentVariables.AppDataPath, "Audits", "auditDB.db");
                var conn = new SQLiteConnection(filePath);
                var query = conn.Table<AuditLog>().Where(v => v.WorkflowID.Equals(auditLog.WorkflowID));
                var result = query.ToList();
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
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

        [Column("NextActivity")]
        public string NextActivity { get; set; }

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
            ExecutingUser = dsfDataObject.ExecutingUser.ToString();
            ExecutionOriginDescription = dsfDataObject.ExecutionOriginDescription;
            ExecutionToken = dsfDataObject.ExecutionToken.ToJson();
            Environment = dsfDataObject.Environment.ToJson();
            AuditDate = DateTime.Now.ToString();
            AuditType = auditType;
            AdditionalDetail = detail;
            if (previousActivity != null)
            {
                PreviousActivity = previousActivity.GetDisplayName();
            }
            if (nextActivity != null)
            {
                NextActivity = nextActivity.GetDisplayName();
            }           
        }
    }

    public class AuditFilter
    {
        public string AuditType { get; set; }
        public AuditFilter() { }
        public AuditFilter(AuditLog log)
        {
            AuditType = log.AuditType;
        }
        public static void AddFilter(AuditFilter filter)
        {
            var filters = new List<AuditFilter>();
            filters.Add(filter);
        }
        public static void RemoveFilter(AuditFilter filter)
        {
            var filters = new List<AuditFilter>();
            filters.Remove(filter);
        }
        public static bool FilterAuditLog(IDev2Activity activity)
        {
            var filters = AuditFilter.ReturnFilters();
            foreach (var filter in filters)
            {
                if (filter.AuditType == activity.GetDisplayName())
                {
                    return false;
                }
            }
            return true;
        }
        public static List<AuditFilter> ReturnFilters()
        {
            var filters = new List<AuditFilter>();
            var filter = new AuditFilter
            {
                AuditType = "LogPreExecuteState"
            };
            filters.Add(filter);

            return filters;
        }

        public static bool FilterAuditLog(IDev2Activity previousActivity, IDev2Activity nextActivity) {
            var filters = AuditFilter.ReturnFilters();
            foreach (var filter in filters)
            {
                if (filter.AuditType == previousActivity.GetDisplayName())
                {
                    return false;
                }
            }
            return true;
        }

        public static bool FilterAuditLog(object detail)
        {
            var filters = AuditFilter.ReturnFilters();
            foreach (var filter in filters)
            {
                if (filter.AuditType == detail.ToString())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
