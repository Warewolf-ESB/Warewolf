/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Timers;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Subscription;
using Newtonsoft.Json;
using Warewolf.Usage;

namespace Dev2.Runtime
{
    public interface IUsageLogger : IStartTimer
    {
        void SaveOfflineUsage(string customerId, string jsonData, UsageType usageType);

    }

    public class UsageLogger : IUsageLogger
    {
        internal readonly Timer _timer;
        readonly string _persistencePath;
        readonly IDirectory _directoryWrapper;
        readonly IFile _fileWrapper;
        readonly IUsageTrackerWrapper _usageTrackerWrapper;

        public UsageLogger(double intervalMs) 
            : this(intervalMs, new UsageTrackerWrapper(), EnvironmentVariables.PersistencePath)
        {
        }
        
        public UsageLogger(double intervalMs, IUsageTrackerWrapper usageTrackerWrapper, string persistencePath)
        {
            Interval = intervalMs;
            _usageTrackerWrapper = usageTrackerWrapper;
            _directoryWrapper = new DirectoryWrapper();
            _fileWrapper = new FileWrapper();
            _persistencePath = persistencePath;
            _timer = new Timer(Interval);
            _timer.Elapsed += (sender, e) => Timer_Elapsed(this, e);
        }

        static int GetNumberOfCores()
        {
            var coreCount = 0;
            foreach(var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }

            return coreCount;
        }

        public void TrackUsage(UsageType usageType, Guid sessionId)
        {
#pragma warning disable 4014
            UploadOfflineFilesAsync();
#pragma warning restore 4014

            var subscriptionProvider = SubscriptionProvider.Instance;
            var myData = new
            {
                sessionId,
                subscriptionProvider.SubscriptionId,
                subscriptionProvider.PlanId,
                subscriptionProvider.Status,
                VersionNo = Studio.Utils.VersionInfo.FetchVersionInfo(),
                Environment.MachineName,
                Environment.ProcessorCount,
                NumberOfCores = GetNumberOfCores(),
                ProcessMemoryUsage = GC.GetTotalMemory(false) / 10000000,
                Executions = ServerStats.TotalExecutions,
                Uptime = DateTime.Now - Process.GetCurrentProcess().StartTime
            };
            var jsonData = JsonConvert.SerializeObject(myData);
            //TODO: Add whether running in container
            var customerId = subscriptionProvider.CustomerId;
            if(customerId == "")
            {
                customerId = "UnRegistered";
            }

            var returnResult = _usageTrackerWrapper.TrackEvent(customerId, usageType, jsonData);
            if(returnResult != UsageDataResult.ok)
            {
                Dev2Logger.Warn("Could not log usage: " + Enum.GetName(typeof(UsageDataResult), returnResult), GlobalConstants.UsageTracker);
                SaveOfflineUsage(customerId, jsonData, usageType);

                ServerStats.IncrementUsageServerRetry();
                if(ServerStats.UsageServerRetry > 3)
                {
                    var stopExecutions = subscriptionProvider.StopExecutions;
                    var lastUploadDate = GetLastUploadTime();
                    if(lastUploadDate.HasValue)
                    {
                        var lastUploadDays = DateTime.Now.Subtract(lastUploadDate.Value).Days;
                        if(stopExecutions && lastUploadDays > 30)
                        {
                            var data = subscriptionProvider.GetSubscriptionData();
                            data.IsLicensed = false;
                            subscriptionProvider.SaveSubscriptionData(data);
                        }
                    }

                    Dev2Logger.Warn(
                        "Could not log usage. Retries: "
                        + ServerStats.UsageServerRetry
                        + GlobalConstants.UsageServerRetriesMoreThan3,
                        GlobalConstants.UsageTracker);
                }
                else
                {
                    Dev2Logger.Warn(
                        "Could not log usage. Retry: "
                        + ServerStats.UsageServerRetry
                        + GlobalConstants.UsageServerRetriesLessThan3,
                        GlobalConstants.UsageTracker);
                }
            }
            else
            {
                ServerStats.ResetUsageServerRetry();
            }
        }

#pragma warning disable 1998
        private async Task UploadOfflineFilesAsync()
#pragma warning restore 1998
        {
            var files = _directoryWrapper.GetFiles(_persistencePath);
            foreach(var file in files)
            {
                var session = JsonConvert.DeserializeObject<SessionData>(_fileWrapper.ReadAllText(file));
                var returnResult = _usageTrackerWrapper.TrackEvent(session.CustomerId, (UsageType)session.UsageType, session.JsonData);
                if(returnResult == UsageDataResult.ok)
                {
                    _fileWrapper.Delete(file);
                }
                else
                {
                    break;
                }
            }
        }

        public void SaveOfflineUsage(string customerId, string jsonData, UsageType usageType)
        {
            if(!_directoryWrapper.Exists(_persistencePath))
            {
                _directoryWrapper.CreateDirectory(_persistencePath);
            }

            var sessionData = new SessionData
            {
                Date = DateTime.Now,
                CustomerId = customerId,
                JsonData = jsonData,
                UsageType = (int)usageType
            };
            _fileWrapper.WriteAllText(Path.Combine(_persistencePath, ServerStats.SessionId.ToString()), JsonConvert.SerializeObject(sessionData));
        }

        private DateTime? GetLastUploadTime()
        {
            var persistencePath = Path.Combine(Config.AppDataPath, "Persistence");
            var files = _directoryWrapper.GetFiles(persistencePath);

            if(files.Any())
            {
                var fileDates = files.Select(File.GetCreationTime);
                var oldestFile = fileDates.OrderByDescending(d => d).FirstOrDefault();
                return oldestFile;
            }

            return null;
        }

        private class SessionData
        {
            public DateTime Date { get; set; }
            public string CustomerId { get; set; }
            public string JsonData { get; set; }
            public int UsageType { get; set; }
        }

        static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
#if !DEBUG
                var usageLogger = (UsageLogger)sender;
                usageLogger.TrackUsage(UsageType.Usage, ServerStats.SessionId);
#endif
            }
            catch(Exception err)
            {
                Dev2Logger.Warn(err.Message, GlobalConstants.WarewolfWarn);
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
    
    public class UsageTrackerWrapper : IUsageTrackerWrapper
    {
        public UsageDataResult TrackEvent(string uniqueId, UsageType usageType, string usageInfo)
        {
            return UsageTracker.TrackEvent(uniqueId, usageType, usageInfo);
        }
    }
    
    public interface IUsageTrackerWrapper
    {
        UsageDataResult TrackEvent(string uniqueId, UsageType usageType, string usageInfo);
    }
}