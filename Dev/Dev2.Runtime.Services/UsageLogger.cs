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
using Dev2.Runtime.Subscription;
using Newtonsoft.Json;
using Warewolf.Usage;

namespace Dev2.Runtime
{
    public class UsageLogger : IStartTimer
    {
        internal readonly Timer _timer;
        static string _persistencePath = Path.Combine(Config.UserDataPath, "Persistence");

        public UsageLogger(double intervalMs)
        {
            Interval = intervalMs;
            _timer = new Timer(Interval);
            _timer.Elapsed += Timer_Elapsed;
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

        static void TrackUsage(UsageType usageType)
        {
            UploadOfflineFiles();

            var subscriptionProvider = SubscriptionProvider.Instance;
            var myData = new
            {
                ServerStats.SessionId,
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

            var returnResult = UsageTracker.TrackEvent(customerId, usageType, jsonData);

            if(returnResult != UsageDataResult.ok)
            {
                SaveOfflineUsage(customerId, jsonData, usageType);

                ServerStats.IncrementUsageServerRetry();
                if(ServerStats.UsageServerRetry > 3)
                {
                    //TODO: If the machine has not connected to the internet for 30 days, stop executions.
                    var stopExecutions = subscriptionProvider.StopExecutions;
                    var lastUploadDate = GetLastUploadTime();
                    if(lastUploadDate.HasValue)
                    {
                        var lastUploadDays = DateTime.Now.Subtract(lastUploadDate.Value).Days;
                        if(stopExecutions && lastUploadDays > 30)
                        {
                            //subscriptionProvider.IsLicensed = false;
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

        private static void UploadOfflineFiles()
        {
            Task.Run(
                () =>
                {
                    var files = Directory.GetFiles(_persistencePath);
                    foreach(var file in files)
                    {
                        var session = JsonConvert.DeserializeObject<SessionData>(File.ReadAllText(file));
                        var returnResult = UsageTracker.TrackEvent(session.CustomerId, (UsageType)session.UsageType, session.JsonData);
                        if(returnResult == UsageDataResult.ok)
                        {
                            File.Delete(file);
                        }
                        else
                        {
                            break;
                        }
                    }
                });
        }

        public static void SaveOfflineUsage(string customerId, string jsonData, UsageType usageType)
        {
            if(!Directory.Exists(_persistencePath)) Directory.CreateDirectory(_persistencePath);
            var sessionData = new SessionData
            {
                Date = DateTime.Now,
                CustomerId = customerId,
                JsonData = jsonData,
                UsageType = (int)usageType
            };
            File.WriteAllText(Path.Combine(_persistencePath, ServerStats.SessionId.ToString()), JsonConvert.SerializeObject(sessionData));
        }

        private static DateTime? GetLastUploadTime()
        {
            var persistencePath = Path.Combine(Config.AppDataPath, "Persistence");
            var files = Directory.GetFiles(persistencePath);

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
                TrackUsage(UsageType.Usage);
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
}