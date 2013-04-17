using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.Security.Principal;
using System.IO;

namespace WebpartConfiguration.Test.Test_Utils
{
    class ProcessInvoker
    {
        private string _filepath;
        private string _processName;
        private System.Diagnostics.ProcessStartInfo _processInfo;
        private System.Diagnostics.Process _process;
        

        public ProcessInvoker(string pathToFile, string processName)
        {
            _filepath = pathToFile;
            _processName = processName;
        }

        public void InvokeProcess()
        {
            _processInfo = new System.Diagnostics.ProcessStartInfo(_filepath);

            int PathLenght = _filepath.Length - _processName.Length;
            _processInfo.Verb = "runas";
            _processInfo.WorkingDirectory = _filepath.Substring(0,PathLenght);
            _process = new System.Diagnostics.Process();
            _process.StartInfo = _processInfo;
            _process.Start();
        }

        public void KillWebserver()
        {
            try
            {
                _process.Kill();
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
