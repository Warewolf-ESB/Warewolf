using System;
using Newtonsoft.Json;
using Dev2.Interfaces;
using Dev2.Communication;
using System.Runtime.Serialization;
using System.Data.SQLite;
using System.Data.Entity;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Data.Linq.Mapping;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;
using System.Data.SQLite.EF6;
using System.Data.Entity.Core.Common;
using Dev2.Common;
using System.IO;
using Dev2.Common.Wrappers;

namespace Dev2.Runtime.ESB.Execution
{
    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        }
    }
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
            var audits = default(IEnumerable<AuditLog>);
            var userPrinciple = Common.Utilities.ServerUser;
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
            {
                var db = GetDatabase();
                audits = db.Audits.Where(queryExpression).AsEnumerable();

            });

            return audits;
        }

        private static DatabaseContext GetDatabase()
        {
            var databaseContext = new DatabaseContext();
            return databaseContext;
        }

        public static void ClearAuditLog()
        {
            var db = GetDatabase();
            foreach (var id in db.Audits.Select(e => e.Id))
            {
                var entity = new AuditLog { Id = id };
                db.Audits.Attach(entity);
                db.Audits.Remove(entity);
            }
            db.SaveChanges();
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
            var userPrinciple = Common.Utilities.ServerUser;
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
            {
                InsertLog(auditLog, 3);
            });
        }

        private static void InsertLog(AuditLog auditLog, int reTry)
        {
            using (var database = GetDatabase())
            {
                try
                {
                    database.Audits.Add(auditLog);
                    database.SaveChanges();
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

    [Database]
    class DatabaseContext : DbContext
    {
        public DatabaseContext() : base(new SQLiteConnection
        {
            ConnectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = Path.Combine(Config.Server.AuditFilePath, "auditDB.db"),
                ForeignKeys = true
            }.ConnectionString
        }, true)
        {
            var userPrinciple = Common.Utilities.ServerUser;
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () => {
                var directoryWrapper = new DirectoryWrapper();
                directoryWrapper.CreateIfNotExists(Config.Server.AuditFilePath);
                DbConfiguration.SetConfiguration(new SQLiteConfiguration());
                this.Database.CreateIfNotExists();
                this.Database.Initialize(false);
                this.Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS \"AuditLog\" ( `Id` INTEGER PRIMARY KEY AUTOINCREMENT, `WorkflowID` TEXT, `WorkflowName` TEXT, `ExecutionID` TEXT, `AuditType` TEXT, `PreviousActivity` TEXT, `PreviousActivityType` TEXT, `PreviousActivityID` TEXT, `NextActivity` TEXT, `NextActivityType` TEXT, `NextActivityID` TEXT, `ServerID` TEXT, `ParentID` TEXT, `ClientID` TEXT, `ExecutingUser` TEXT, `ExecutionOrigin` INTEGER, `ExecutionOriginDescription` TEXT, `ExecutionToken` TEXT, `AdditionalDetail` TEXT, `IsSubExecution` INTEGER, `IsRemoteWorkflow` INTEGER, `Environment` TEXT, `AuditDate` TEXT )");
            });
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AuditLog> Audits { get; set; }
    }

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

    static class IIdentityExtensionMethods
    {
        public static string ToJson(this System.Security.Principal.IIdentity identity)
        {
            var json = new Dev2JsonSerializer();
            return json.Serialize(identity, Formatting.None);
        }
    }

    static class ExecutionTokenExtensionMethods
    {
        public static string ToJson(this Common.Interfaces.IExecutionToken executionToken)
        {
            var json = new Dev2JsonSerializer();
            return json.Serialize(executionToken, Formatting.None);
        }
    }
}
