using System;
using System.IO;
using Newtonsoft.Json;
using Dev2.Interfaces;
using Dev2.Common;
using Dev2.Common.Wrappers;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Interfaces;
using System.IO.Compression;
using Dev2.Communication;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Security.Principal;
using Dev2.Services.Sql;
using System.Data;
using System.Text;
using System.Runtime.Serialization;
using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Runtime.ESB.Execution
{
    class Dev2StateAuditLogger : IStateListener
    {
        readonly IDSFDataObject _dsfDataObject;

        public Dev2StateAuditLogger(IDSFDataObject dsfDataObject)
        {
            _dsfDataObject = dsfDataObject;
        }
        public List<AuditLog> FilterLogAuditState(string auditType,string workflowID,string workflowName,string startDateTime,string endDateTime,string executionID)
        {
            var filePath = Path.Combine(EnvironmentVariables.AppDataPath, "Audits", "auditDB.db");
            var database = new SQLiteConnection(filePath);
            var query = database.Table<AuditLog>()
                .Where(a => (a.WorkflowID.Equals(workflowID)
                || a.WorkflowName.Equals(workflowName)
                || a.ExecutionID.Equals(executionID)
                || a.AuditType.Equals(auditType))
              );
            var result = query.ToList();
            database.Close();
            return result;
        }
        public void LogAdditionalDetail(object detail, string callerName)
        {
            var serializer = new Dev2JsonSerializer();
            var auditLog = new AuditLog(_dsfDataObject, "LogAdditionalDetail", serializer.Serialize(detail, Formatting.None), null, null);
            LogAuditState(auditLog);
        }
        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogPostExecuteState", null, previousActivity, nextActivity);
            LogAuditState(auditLog);
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogExecuteException", e.Message, activity, null);
            LogAuditState(auditLog);
        }

        public void LogExecuteCompleteState()
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogStopExecutionState", null, null, null);
            LogAuditState(auditLog);
        }

        public void LogStopExecutionState()
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogStopExecutionState", null, null, null);
            LogAuditState(auditLog);
        }

        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
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
                PreviousActivity = previousActivity.ToString();
            }
            if (nextActivity != null)
            {
                NextActivity = nextActivity.ToString();
            }
        }
    }
}
