using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Controller
{
    public class ProcessController
    {
        private bool _isRunning;

        public Process UtilityProcess { get; private set; }

        public string CmdLine { get; set; }

        public string Arguments { get; set; }

        public ProcessController() : this(null)
        {
        }

        public ProcessController(Process process)
        {
            UtilityProcess = process ?? new Process();
            _isRunning = false;
        }

        public void Start()
        {
            if (!_isRunning)
            {
                var startInfo = new ProcessStartInfo()
                    {
                        CreateNoWindow = true,
                        UseShellExecute = true,
                        FileName = CmdLine,
                        Arguments = Arguments
                    };

                Process.Start(startInfo);
                _isRunning = true;

            }
            else
            {
                throw new InvalidOperationException("Process already started");
            }
        }
    }
}
