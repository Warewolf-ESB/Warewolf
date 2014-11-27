
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Diagnostics;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2TaskSettings : ITaskSettings
    {
        private readonly TaskSettings _nativeInstance;

        public Dev2TaskSettings(TaskSettings nativeInstance)
        {
            _nativeInstance = nativeInstance;
        }

        public ProcessPriorityClass Priority
        {
            get { return Instance.Priority; }
            set { Instance.Priority = value; }
        }

        public bool AllowDemandStart
        {
            get { return Instance.AllowDemandStart; }
            set { Instance.AllowDemandStart = value; }
        }

        public bool AllowHardTerminate
        {
            get { return Instance.AllowHardTerminate; }
            set { Instance.AllowHardTerminate = value; }
        }

        public TimeSpan DeleteExpiredTaskAfter
        {
            get { return Instance.DeleteExpiredTaskAfter; }
            set { Instance.DeleteExpiredTaskAfter = value; }
        }

        public bool DisallowStartOnRemoteAppSession
        {
            get { return Instance.DisallowStartIfOnBatteries; }
            set { Instance.DisallowStartIfOnBatteries = value; }
        }

        public bool Enabled
        {
            get { return Instance.Enabled; }
            set { Instance.Enabled = value; }
        }

        public TimeSpan ExecutionTimeLimit
        {
            get { return Instance.ExecutionTimeLimit; }
            set { Instance.ExecutionTimeLimit = value; }
        }

        public bool Hidden
        {
            get { return Instance.Hidden; }
            set { Instance.Hidden = value; }
        }


        public int RestartCount
        {
            get { return Instance.RestartCount; }
            set { Instance.RestartCount = value; }
        }

        public TimeSpan RestartInterval
        {
            get { return Instance.RestartInterval; }
            set { Instance.RestartInterval = value; }
        }

        public bool StartWhenAvailable
        {
            get { return Instance.StartWhenAvailable; }
            set { Instance.StartWhenAvailable = value; }
        }

        public bool WakeToRun
        {
            get { return Instance.WakeToRun; }
            set { Instance.WakeToRun = value; }
        }

        public TaskInstancesPolicy MultipleInstances
        {
            get { return Instance.MultipleInstances; }
            set { Instance.MultipleInstances = value; }
        }


        public void Dispose()
        {
            Instance.Dispose();
        }


        public TaskSettings Instance
        {
            get { return _nativeInstance; }
        }
    }
}
