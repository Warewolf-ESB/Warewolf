#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using System.Timers;
using Dev2.Common;
using Dev2.Common.Interfaces;

namespace Dev2.Data
{
    public class PulseLogger : IStartTimer
    {
        internal readonly Timer _timer;

        public PulseLogger(double intervalMs)
        {
            Interval = intervalMs;
            _timer = new Timer(Interval);
            _timer.Elapsed += Timer_Elapsed;       
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dev2Logger.Info(String.Format(@"
    Process Memory Usage(mb): {0}
    Number of Requests: {1} 
    Time Taken(Ms): {2}   
    Uptime: {3}",
                    GC.GetTotalMemory(false) / 10000000,
                    ServerStats.TotalRequests,
                    ServerStats.TotalTime,
                    DateTime.Now - Process.GetCurrentProcess().StartTime), "Warewolf System Data");
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
