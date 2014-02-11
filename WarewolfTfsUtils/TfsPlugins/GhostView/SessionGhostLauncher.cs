using System;
using System.Diagnostics;

namespace GhostView
{
    /// <summary>
    /// 
    /// KEY INFO : 
    /// 
    /// Used to start any "background" task in TFS
    /// Due to silly chickens working as M$ it is not possible to start a task via a bat file directly ( Even if it is creating background task ?! )
    /// Infact it is not possible to start any sort of task directly and have it live for the life-cycle of the workflow.
    /// Hence GhostView was created, a crafty solution to addressing this issue.
    /// This App creates a new background process in-which to start the desired
    /// background process, thus avoiding the silly nature of TFS workflows and background task.
    /// ;)
    /// </summary>
    class SessionGhostLauncher
    {
        static int Main(string[] args)
        {
            
            if (args.Length == 4)
            {
                Console.WriteLine("All Good, Launching");

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
