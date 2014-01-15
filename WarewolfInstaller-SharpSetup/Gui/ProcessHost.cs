using System.Diagnostics;

namespace Gui
{
    public static class ProcessHost
    {
        public static bool Invoke(string workingDir, string fileName, string args, bool waitForExit = true)
        {
            var invoked = true;

            var p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = fileName
                }
            };

            if(!string.IsNullOrEmpty(workingDir))
            {
                p.StartInfo.WorkingDirectory = workingDir;
            }
            if(!string.IsNullOrEmpty(args))
            {
                p.StartInfo.Arguments = args;
            }

            if(p.Start())
            {
                if(waitForExit)
                {
                    // wait up to 10 seconds for exit ;)
                    p.WaitForExit(10000);
                }
            }
            else
            {
                invoked = false;
            }

            return invoked;
        }
    }
}
