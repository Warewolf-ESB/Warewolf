using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostView
{
    /// <summary>
    /// Used to start an RDP session for coded UI testing
    /// Provided a maximized screen yet invisible like a ghost ;)
    /// </summary>
    class GhostViewer
    {
        static int Main(string[] args)
        {
            var rdpFile = @"C:\Users\travis.frisinger\Desktop\tfsbld.rdp";

            if (args.Length == 1)
            {
                return StartSession(args[0]);
            }

            return StartSession(rdpFile);

        }


        private static int StartSession(string rdpFile)
        {
            string executable = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\mstsc.exe");

            Process rdcProcess = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = true;
            startInfo.FileName = executable;
            //startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.Arguments = rdpFile;

            if (executable != null)
            {
                rdcProcess = Process.Start(startInfo);

                return rdcProcess.Id;
            }

            return -1;
        }
    }
}
