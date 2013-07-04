using System;
using System.Diagnostics;

namespace GhostView
{
    /// <summary>
    /// Used to start an RDP session for coded UI testing
    /// Provided a maximized screen yet invisible like a ghost ;)
    /// </summary>
    class SessionGhostLauncher
    {
        static int Main(string[] args)
        {
            
            if (args.Length == 4)
            {
                string appPath = args[0];
                string svr = args[1];
                string user = args[2];
                string pass = args[3];

                ProcessStartInfo processStartInfo = new ProcessStartInfo();

                processStartInfo.FileName = appPath;
                //processStartInfo.CreateNoWindow = true;
                processStartInfo.Arguments = svr + " " + user + " " + pass;
                //processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process gView = new Process();
                gView.StartInfo = processStartInfo;

                gView.Start();

                return gView.Id;
            }
            else
            {
                Console.WriteLine("Arg Cnt [ " + args.Length + " ]");
            }

            return -1;
        }

    }
}
