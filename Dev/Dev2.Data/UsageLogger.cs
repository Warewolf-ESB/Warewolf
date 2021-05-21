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
using System.Management;
using System.Timers;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Newtonsoft.Json;
using Warewolf.Usage;

namespace Dev2.Data
{
    public class UsageLogger : IStartTimer
    {
        internal readonly Timer _timer;

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

        void TrackUsage(UsageType usageType)
        {
            var myData = new
            {
                ServerStats.SessionId,
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
            //TODO: Licensing: Get customer ID from the licensing
            var customerId = "Unknown";
            var returnResult = UsageTracker.TrackEvent(customerId, usageType, jsonData);
            if(returnResult != UsageDataResult.ok)
            {
                ServerStats.IncrementUsageServerRetry();
                if(ServerStats.UsageServerRetry > 3)
                {
                    //TODO: Licensing: switch warewolf to readonly mode. Only Warn until Licensing is implemented
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

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
#if ! (DEBUG)
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