
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
using System.Management;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Controller
{
    public class ProcessController
    {
        private bool _isRunning;

        public Process UtilityProcess { get; private set; }

        public string CmdLine { get; set; }

        public string Arguments { get; set; }

        public bool ShowErrorDialog { get; set; }

        public bool UseShellExecute { get; set; }

        public string Verb { get; set; }

        public ProcessController()
            : this(null)
        {
        }

        public ProcessController(Process process)
        {
            UtilityProcess = process;
        }

        public void Start()
        {
            if(_isRunning)
            {
                throw new InvalidOperationException("Process already started");
            }

            if(UtilityProcess != null)
            {
                UtilityProcess.Start();
                _isRunning = true;
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = CmdLine,
                Arguments = Arguments,
                ErrorDialog = ShowErrorDialog,
                Verb = Verb,
                UseShellExecute = UseShellExecute
            };

            UtilityProcess = Process.Start(startInfo);
            _isRunning = true;
        }

        public void Kill(string processName)
        {
            var theScope = new ManagementScope("root\\cimv2");
            var theQuery = new ObjectQuery(string.Format("SELECT * FROM Win32_Process WHERE Name LIKE '{0}%'", processName));
            var theSearcher = new ManagementObjectSearcher(theScope, theQuery);
            var theCollection = theSearcher.Get();

            foreach(var o in theCollection)
            {
                var theCurObject = (ManagementObject)o;
                theCurObject.InvokeMethod("Terminate", null);
            }

            while(!UtilityProcess.HasExited)
            {
                CheckChildProcesses(UtilityProcess.Id);
                Thread.Sleep(10);
            }

            UtilityProcess.Close();
        }

        void CheckChildProcesses(int id)
        {
            var searcher = new ManagementObjectSearcher("root\\CIMV2", string.Format("SELECT * FROM Win32_Process Where ParentProcessId={0}", id));

            var managementObjectCollection = searcher.Get();
            foreach(var o in managementObjectCollection)
            {
                var queryObj = (ManagementObject)o;
                var pid = Convert.ToInt32(queryObj["ProcessId"]);
                var processById = Process.GetProcessById(pid);
                processById.Kill();
            }
        }
    }
}
