using System;
using Newtonsoft.Json;
using Dev2.Interfaces;
using Dev2.Communication;
using System.Data.SQLite;
using System.Data.Entity;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Data.Linq.Mapping;
using System.Linq.Expressions;
using System.Data.SQLite.EF6;
using System.Data.Entity.Core.Common;
using Dev2.Common;
using System.IO;
using Dev2.Common.Wrappers;

namespace Dev2.Runtime.ESB.Execution
{
    interface IDev2StateAuditLogger
    {
        void LogAuditState(AuditLog auditLog);
    }

    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        }
    }

    class Dev2StateAuditLogger : IDev2StateAuditLogger, IStateListener, IDisposable
    {
        readonly StateListener _listener;

        public Dev2StateAuditLogger(IDSFDataObject dataObject=null)
        {
            _listener = new StateListener(this, dataObject);
        }

        public void LogAdditionalDetail(object detail, string callerName)
        {
            _listener?.LogAdditionalDetail(detail, callerName);
        }

        public void LogExecuteCompleteState(IDev2Activity activity)
        {
            _listener?.LogExecuteCompleteState(activity);
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            _listener?.LogExecuteException(e, activity);
        }

        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            _listener?.LogPostExecuteState(previousActivity, nextActivity);
        }

        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
            _listener?.LogPreExecuteState(nextActivity);
        }

        public void LogStopExecutionState(IDev2Activity activity)
        {
            _listener?.LogStopExecutionState(activity);
        }
        public void Dispose()
        {

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

        public void LogAuditState(AuditLog auditLog)
        {
            var userPrinciple = Common.Utilities.ServerUser;
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
            {
                Flush(auditLog, 3);
            });
        }

        private static void Flush(AuditLog auditLog, int reTry)
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
                    Flush(auditLog, reTry);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
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
