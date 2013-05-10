using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using Dev2.Activities;

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

        public ProcessController() : this(null)
        {
        }

        public ProcessController(Process process)
        {
            UtilityProcess = process;
        }

        public void Start()
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("Process already started");
            }

            if (UtilityProcess != null)
            {
                UtilityProcess.Start();
                _isRunning = true;
                return;
            }

            var startInfo = new ProcessStartInfo()
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

        public void Kill()
        {
            while (!UtilityProcess.HasExited)
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
            foreach (ManagementObject queryObj in managementObjectCollection)
            {
                var pid = Convert.ToInt32(queryObj["ProcessId"]);
                var processById = Process.GetProcessById(pid);
                processById.Kill();
            }
        }
    }
}
