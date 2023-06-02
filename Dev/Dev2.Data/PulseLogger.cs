#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using sun.reflect.generics.tree;
using Warewolf.Execution;
using Warewolf.Interfaces.Auditing;
using Warewolf.Streams;


namespace Dev2.Data
{
    public class PulseLogger : IStartTimer
    {
        internal readonly Timer _timer;
        private readonly IExecutionLogPublisher _logger;
        private string auditFilePath = null;
        private long maxLogFileSize = LegacySettings.DefaultAuditLogMaxSize;
        private static string backupDirectory = Path.Combine(Config.AppDataPath, "Audits", "BackUp");

        public PulseLogger(double intervalMs, IExecutionLogPublisher executionLogPublisher)
        {
            NativeMethods.RegisterNotification();
            Interval = intervalMs;
            _timer = new Timer(Interval);
            _timer.Elapsed += Timer_Elapsed;
            _logger = executionLogPublisher;
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dev2Logger.Info(String.Format(@"
    Process Memory Usage(mb): {0}
    Number of Requests: {1} 
    Time Taken(Ms): {2}   
    Uptime: {3}
    Total Physical Memory: {4}
    Total Available Physical Memory: {5}
    Load Memory: {6}",
                    GC.GetTotalMemory(false) / 10000000,
                    ServerStats.TotalRequests,
                    ServerStats.TotalTime,
                    DateTime.Now - Process.GetCurrentProcess().StartTime,
                    MemoryPressureTracker(MemoryStatus.TotalPhys),
                    MemoryPressureTracker(MemoryStatus.AvaliablePhys),
                    MemoryPressureTracker(MemoryStatus.LoadMemory)),"Warewolf System Data");

                AuditDBTracker();
            }                
            catch (Exception err)
            {
                Dev2Logger.Warn(err.Message, "Warewolf Warn");
            }
        }

        public IStartTimer Start()
        {
            try
            {
                _timer.Start();
                return this;
            }
            catch(Exception)
            {

                return null;
            }
        }
        
        public void Dispose()
        {
            _timer.Dispose();
            NativeMethods.ReleaseResources();
        }

        public double Interval { get; private set; }

        public enum MemoryStatus
        {
            TotalPhys,
            AvaliablePhys,
            LoadMemory
        }

        public StringBuilder GetConvertedBytes(ulong statusValue, StringBuilder stringBuilder)
        {
            ByteConstants byteConstants = new ByteConstants();

            if ((long)statusValue / byteConstants.OneGbValue < 1)
            {
                
                return stringBuilder.Append(Utilities.ByteConvertor((long)statusValue, byteConstants.OneMbValue) + "MB");
            }
            else
            {
                return stringBuilder.Append(Utilities.ByteConvertor((long)statusValue, byteConstants.OneGbValue) + "GB");
            }   
        }

        public string MemoryPressureTracker(MemoryStatus memoryStatus)
        {
            NativeMethods.MEMORYSTATUSEX status = new NativeMethods.MEMORYSTATUSEX();
            status.dwLength = (uint)Marshal.SizeOf(status);
            Boolean ret = NativeMethods.GlobalMemoryStatusEx(ref status);
            
            StringBuilder stringBuilder = new StringBuilder();
            var memoryPressureMessage = string.Empty;

            switch (memoryStatus)
            {
                case MemoryStatus.TotalPhys:
                    GetConvertedBytes(status.ulTotalPhys, stringBuilder);
                    break;
                case MemoryStatus.AvaliablePhys:
                    GetConvertedBytes(status.ulAvailPhys, stringBuilder);
                    break;
                case MemoryStatus.LoadMemory:
                    if (((int)status.dwMemoryLoad) >= 90)
                    {
                        memoryPressureMessage = "Load memory of " + status.dwMemoryLoad + "% Used. High Memory Pressure detected.";
                        Dev2Logger.Warn(memoryPressureMessage, "Warewolf Warn");
                        _logger.Warn(memoryPressureMessage);
                    }

                    if(NativeMethods.IsLowMemoryDetected())
                    {
                        var lowMemoryMessage = "Low memory of " + status.dwMemoryLoad + "% signaled by Operating System at " + DateTime.Now.ToString();
                        Dev2Logger.Warn(lowMemoryMessage, "Warewolf Warn");
                        _logger.Warn(lowMemoryMessage);
                    }
                    stringBuilder.Append((status.dwMemoryLoad) + "% Used. " + memoryPressureMessage);
                    break;
            }
                        
            return stringBuilder.ToString();
        }

        public void AuditDBTracker()
        {
            try
            {
                var auditDBMessage = string.Empty;

                CheckIfBackupDirectoryExist(backupDirectory);
                AuditDBFileProperties();

                long fileSizeInBytes = new FileInfo(auditFilePath).Length;
                long fileSizeInMB = fileSizeInBytes / (1024 * 1024); // Convert bytes to megabytes

                if (fileSizeInMB >= maxLogFileSize)
                {
                    string backupFileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                    string backupFilePath = Path.Combine(backupDirectory, backupFileName);

                    //Create backup file
                    File.Copy(auditFilePath, backupFilePath);
                    auditDBMessage = "Backup created successfully.";

                    //Create new file
                    CreateNewDBFile(auditFilePath);
                    auditDBMessage = auditDBMessage + " New AuditDB file created.";

                    Dev2Logger.Info(auditDBMessage, "Warewolf Info");
                    _logger.Info(auditDBMessage);

                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Warn(ex.Message, "Warewolf Warn");
                _logger.Warn(ex.Message);
            }
        }

        public void CheckIfBackupDirectoryExist(string backupDirectory)
        {
            try
            {
                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Warn(ex.Message, "Warewolf Warn");
                _logger.Warn(ex.Message);
            }
        }

        public void AuditDBFileProperties()
        {
            LegacySettings legacySettings = new LegacySettings();

            auditFilePath = legacySettings.AuditFilePath;
            auditFilePath = Path.Combine(auditFilePath, "AuditDB.db");
            maxLogFileSize = legacySettings.AuditLogMaxSize;
        }

        public void CreateNewDBFile(string sourceFilePath)
        {
            var destinationFilePath = sourceFilePath.Replace(".db", "_New.db");

            try
            {
                CreateBlankDatabase(sourceFilePath, destinationFilePath);

                //Remove old file
                File.Delete(sourceFilePath);
                File.Move(destinationFilePath, destinationFilePath.Replace("_New.db", ".db"));
            }
            catch (Exception ex)
            {
                Dev2Logger.Warn(ex.Message, "Warewolf Warn");
                _logger.Warn(ex.Message);
            }
        }

        void CreateBlankDatabase(string sourceFilePath, string destinationFilePath)
        {
            using (var sourceConnection = new SQLiteConnection($"Data Source={sourceFilePath};"))
            {
                sourceConnection.Open();

                // Get the schema (table structure) of the source database
                string schemaSql = "SELECT sql FROM sqlite_master WHERE type='table'";
                using (var schemaCommand = new SQLiteCommand(schemaSql, sourceConnection))
                using (var schemaReader = schemaCommand.ExecuteReader())
                {
                    SQLiteConnection.CreateFile(destinationFilePath);

                    using (var destinationConnection = new SQLiteConnection($"Data Source={destinationFilePath}"))
                    {
                        destinationConnection.Open();

                        while (schemaReader.Read())
                        {
                            string createTableSql = schemaReader.GetString(0);
                            if (!createTableSql.StartsWith("CREATE TABLE sqlite_", StringComparison.OrdinalIgnoreCase))
                            {
                                using (var createTableCommand = new SQLiteCommand(createTableSql, destinationConnection))
                                {
                                    createTableCommand.ExecuteNonQuery();
                                }
                            }
                        }

                        destinationConnection.Close();
                    }
                }

                sourceConnection.Close();
            }
        }
    }

    public class PulseTracker : IStartTimer
    {
        readonly Timer _timer;

        public PulseTracker(double intervalMs)
        {
            Interval = intervalMs;
            _timer = new Timer(Interval);
            _timer.Elapsed += TimerElapsed;       
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (WorkflowExecutionWatcher.HasAWorkflowBeenExecuted)
                {                   
                    WorkflowExecutionWatcher.HasAWorkflowBeenExecuted = false;
                }
            }
            catch (Exception err)
            {
                Dev2Logger.Warn(err.Message, "Warewolf Warn");
            }
        }

    
        public IStartTimer Start()
        {
            try
            {
                _timer.Start();
                return this;
            }
            catch(Exception)
            {
                return null;
            }
        }
     
        public void Dispose()
        {
            _timer.Dispose();
        }

        public double Interval { get; private set; }
    }
}
