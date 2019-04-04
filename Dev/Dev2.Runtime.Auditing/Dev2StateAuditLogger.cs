#pragma warning disable
﻿using System;
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
using Dev2.Common.Interfaces.Container;
using LinqKit;

namespace Dev2.Runtime.Auditing
{
    public interface IDev2StateAuditLogger : IDisposable
    {
        IStateListener NewStateListener(IDSFDataObject dataObject);
        
        void Flush();
    }

    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            SQLiteLog.Enabled = false;
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        }
    }

    public class Dev2StateAuditLogger : IDev2StateAuditLogger, IWarewolfLogWriter
    {
        const int MAX_DATABASE_TRIES = 10;
        const int DATABASE_RETRY_DELAY = 100;

        public IStateListener NewStateListener(IDSFDataObject dataObject) => new StateListener(this, dataObject);
        readonly IDatabaseContextFactory _databaseContextFactory;

        readonly IWarewolfQueue _warewolfQueue;

        public Dev2StateAuditLogger(IDatabaseContextFactory databaseContextFactory, IWarewolfQueue warewolfQueue)
        {
            _databaseContextFactory = databaseContextFactory;
            _warewolfQueue = warewolfQueue;
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

        public static IQueryable<AuditLog> PredicateQuery(Expression<Func<AuditLog, bool>> queryExpression)
        {
            var audits = default(IQueryable<AuditLog>);
            var userPrinciple = Common.Utilities.ServerUser;
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
            {
                var db = new DatabaseContext();
                audits = db.Audits.AsExpandable().Where(queryExpression).AsQueryable();

            });

            return audits;
        }

        public static void ClearAuditLog()
        {
            var db = new DatabaseContext();
            db.Database.ExecuteSqlCommand("DELETE FROM AuditLog");

            db.SaveChanges();
        }

        public void LogAuditState(Object logEntry)
        {
            if (logEntry is AuditLog auditLog)
            {
                using (var session = _warewolfQueue.OpenSession())
                {
                    session.Enqueue(auditLog);
                    session.Flush();
                }
                return;
            }
            throw new ArgumentException("unhandled log type: " + logEntry?.GetType().Name);
        }

        private void Flush(IAuditDatabaseContext database, int reTry)
        {
            
            try
            {
                database.SaveChanges();
            }
            catch (Exception e)
            {
                if (reTry == 0)
                {
                    throw;
                }
                reTry--;
                Thread.Sleep(DATABASE_RETRY_DELAY);
                Flush(database, reTry);
            }
        }

        public void Flush()
        {
            bool AddLogsToDatabase(IAuditDatabaseContext databaseContext)
            {
                var _hadLogs = false;
                using (var session = _warewolfQueue.OpenSession())
                {
                    for (int insertCount = 0; insertCount <= 250; insertCount++)
                    {
                        var auditLog = session.Dequeue<AuditLog>();
                        if (auditLog is null)
                        {
                            break;
                        }
                        databaseContext.Audits.Add(auditLog);
                        _hadLogs = true;
                    }
                }
                return _hadLogs;
            }
            void TryGetDatabaseContext(ref IAuditDatabaseContext databaseContext)
            {
                int count = MAX_DATABASE_TRIES;
                do
                {
                    try
                    {
                        databaseContext = _databaseContextFactory.Get();
                    }
                    catch (Exception)
                    {
                        count--;
                        if (count <= 0)
                        {
                            throw;
                        }
                    }
                    Thread.Sleep(DATABASE_RETRY_DELAY);
                } while (databaseContext is null && --count >= 0);
            }

            if (_warewolfQueue.IsEmpty())
            {
                return;
            }

            IAuditDatabaseContext database = null;
            var hadLogs = false;
            do
            {
                TryGetDatabaseContext(ref database);

                using (database)
                {
                    using (var transaction = database.Database.BeginTransaction())
                    {
                        try
                        {
                            try
                            {
                                hadLogs = AddLogsToDatabase(database);
                            }
                            finally
                            {
                                Flush(database, MAX_DATABASE_TRIES);
                            }
                            transaction.Commit();
                        } catch
                        {
                            transaction.Rollback();
                        }
                    }
                }
            } while (hadLogs);
        }
    }

    public interface IAuditDatabaseContext : IDisposable
    {
        DbSet<AuditLog> Audits { get; set; }
        int SaveChanges();
        Database Database { get; }
    }
    public interface IDatabaseContextFactory
    {
        IAuditDatabaseContext Get();
    }
    public class DatabaseContextFactory : IDatabaseContextFactory
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
                this.Configuration.AutoDetectChangesEnabled = false;
                this.Configuration.ValidateOnSaveEnabled = false;
                var directoryWrapper = new DirectoryWrapper();
                directoryWrapper.CreateIfNotExists(Config.Server.AuditFilePath);
                DbConfiguration.SetConfiguration(new SQLiteConfiguration());
                this.Database.CreateIfNotExists();
                this.Database.Initialize(false);
                this.Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS \"AuditLog\" ( `Id` INTEGER PRIMARY KEY AUTOINCREMENT, `WorkflowID` TEXT, `WorkflowName` TEXT, `ExecutionID` TEXT, `VersionNumber` TEXT, `AuditType` TEXT, `PreviousActivity` TEXT, `PreviousActivityType` TEXT, `PreviousActivityID` TEXT, `NextActivity` TEXT, `NextActivityType` TEXT, `NextActivityID` TEXT, `ServerID` TEXT, `ParentID` TEXT, `ClientID` TEXT, `ExecutingUser` TEXT, `ExecutionOrigin` INTEGER, `ExecutionOriginDescription` TEXT, `ExecutionToken` TEXT, `AdditionalDetail` TEXT, `IsSubExecution` INTEGER, `IsRemoteWorkflow` INTEGER, `Environment` TEXT, `AuditDate` TEXT )");
                this.Configuration.AutoDetectChangesEnabled = false;
                this.Configuration.ValidateOnSaveEnabled = false;
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
