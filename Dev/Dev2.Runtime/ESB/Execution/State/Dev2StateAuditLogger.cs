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
using Dev2.Common.Interfaces.Logging;
using System.Threading;

namespace Dev2.Runtime.ESB.Execution
{
    interface IDev2StateAuditLogger : IDisposable
    {
        IStateListener NewStateListener(IDSFDataObject dataObject);
        void Flush();
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

    class Dev2StateAuditLogger : IDev2StateAuditLogger, IWarewolfLogWriter
    {
        const int MAX_DATABASE_TRIES = 3;

        readonly IList<AuditLog> _auditLogsBuffer = new List<AuditLog>();
        public IStateListener NewStateListener(IDSFDataObject dataObject) => new StateListener(this, dataObject);
        readonly IDatabaseContextFactory _databaseContextFactory;

        // TODO: add WarewolfQueue

        public Dev2StateAuditLogger(IDatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
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
                var db = new DatabaseContext();
                audits = db.Audits.Where(queryExpression).AsEnumerable();

            });

            return audits;
        }

        public static void ClearAuditLog()
        {
            var db = new DatabaseContext();
            foreach (var id in db.Audits.Select(e => e.Id))
            {
                var entity = new AuditLog { Id = id };
                db.Audits.Attach(entity);
                db.Audits.Remove(entity);
            }
            db.SaveChanges();
        }

        public void LogAuditState(Object logEntry)
        {
            if (logEntry is AuditLog auditLog)
            {
                // TODO: this function will enqueue data into the WarewolfQueue
                _auditLogsBuffer.Add(auditLog);
                return;
            }
            throw new ArgumentException("unhandled log type: " + logEntry?.GetType().Name);
        }

        private void Flush(IAuditDatabaseContext database, int reTry)
        {
            var userPrinciple = Common.Utilities.ServerUser;
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
            {
                try
                {
                    database.SaveChanges();
                }
                catch (SQLiteException e)
                {
                    if (reTry == 0)
                    {
                        throw;
                    }
                    reTry--;
                    Flush(database, reTry);
                }
            });
        }

        public void Flush()
        {
            IAuditDatabaseContext database = null;
            int count = MAX_DATABASE_TRIES;
            do
            {
                try
                {
                    database = _databaseContextFactory.Get();
                }
                catch (Exception)
                {
                    count--;
                    if (count <= 0)
                    {
                        throw;
                    }
                }
                Thread.Sleep(100);
            } while (database is null && --count >= 0);

            using (database)
            {
                foreach (var logItem in _auditLogsBuffer)
                {
                    // TODO: this will dequeue data from the WarewolfQueue
                    database.Audits.Add(logItem);
                }

                Flush(database, MAX_DATABASE_TRIES);
            }
        }
    }

    interface IAuditDatabaseContext : IDisposable
    {
        DbSet<AuditLog> Audits { get; set; }
        int SaveChanges();
    }
    interface IDatabaseContextFactory
    {
        IAuditDatabaseContext Get();
    }
    class DatabaseContextFactory : IDatabaseContextFactory
    {
        public IAuditDatabaseContext Get()
        {
            return new DatabaseContext();
        }
    }

    [Database]
    class DatabaseContext : DbContext, IAuditDatabaseContext
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
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
            {
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

    static class ExecutionTokenExtensionMethods
    {
        public static string ToJson(this Common.Interfaces.IExecutionToken executionToken)
        {
            var json = new Dev2JsonSerializer();
            return json.Serialize(executionToken, Formatting.None);
        }
    }
}
