using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dev2.Studio.UI.Tests
{
    public sealed class ProcessManager
    {
        private Process _thisProcess;
        private int _processIdentifier;
        private string _processFilePath;
        private string _processFolder;

        public ProcessManager(string processName)
        {
            Initialize(processName);
        }


        #region Initialization

        private void Initialize(string processName)
        {
            Process process = new Process();
            List<Process> processToRemove = new List<Process>();
            Process processWeReallyWant;
            var processToRetrieve = System.Diagnostics.Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName.StartsWith(processName));
            if (processToRetrieve == null || processToRetrieve.Count() == 0)
            {
                throw new Exception("Error - unable to find running process " + processName);
            }
            else if (processToRetrieve.Count() > 1)
            {
                foreach (Process proc in processToRetrieve)
                {
                    if (proc.ProcessName.Contains("vshost"))
                    {
                        processToRemove.Add(proc);
                    }
                }
                processToRemove.ForEach(c => processToRetrieve = processToRetrieve.Where(d => d.Id != c.Id));
                
                if (processToRetrieve.Count() > 1)
                {
                    throw new Exception(string.Format("Error - more than 1 process of {0} is running", processName));
                }
                processWeReallyWant = processToRetrieve.FirstOrDefault();
            }
            else
            {
                processWeReallyWant = processToRetrieve.First();
                
            }

            string wMIQuery = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processWeReallyWant.Id.ToString();

            using (var searcher = new ManagementObjectSearcher(wMIQuery))
            {
                using (var result = searcher.Get())
                {
                    ManagementObject processInfo = result.Cast<ManagementObject>().FirstOrDefault();

                    if (processInfo != null)
                    {
                        _thisProcess = processWeReallyWant;
                        _processFilePath = (string)processInfo["ExecutablePath"];

                        // Sashen.Naidoo: A Note to user, if you are running an instance of your process through Debugger,
                        //                The process manager will restart the process again using the executable
                        //                Which means you will not be debugging anymore when running any test using this class
                        //                This could either be done or a popup will be displayed prompting the user to restart 
                        //                their debug session then continue running tests

                        if (_processFilePath.Contains("vshost")) 
                            _processFilePath = _processFilePath.Substring(0, _processFilePath.IndexOf("vshost") - 1) 
                                                        + _processFilePath.Substring(_processFilePath.IndexOf("vshost") 
                                                                                        + "vshost".Length, _processFilePath.Length 
                                                                                        - (_processFilePath.IndexOf("vshost") + "vshost".Length));
                        Int32.TryParse(processInfo["ProcessId"].ToString(), out _processIdentifier);
                        _processFolder = _processFilePath.Substring(0, _processFilePath.LastIndexOf(@"\") + 1);
                    }
                }
            }
        }

        #endregion Initialization

        public void CloseProcess()
        {
            if (!_thisProcess.HasExited)
            {
                _thisProcess.Close();
                Thread.Sleep(3000);
            }
        }

        public bool IsProcessRunning()
        {
            return (!_thisProcess.HasExited);
        }

        public void StartProcess()
        {
            if (_thisProcess == null || _thisProcess.HasExited)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = _processFilePath,
                    UseShellExecute = true,
                    WorkingDirectory = _processFolder,
                    Verb = "runas"
                };

                Process proc = new Process();
                proc.StartInfo = startInfo;
                proc.Start();
                // Give it 10 seconds to start
                Thread.Sleep(10000);
                _thisProcess = proc;
            }
        }
    }
}
