using System.Diagnostics;

namespace Dev2.Runtime.Security
{
    internal static class ProcessHost
    {
        public static bool Invoke(string workingDir, string fileName, string args)
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
                // wait up to 10 seconds for exit ;)
                p.WaitForExit(10000);

            }
            else
            {
                invoked = false;
            }

            return invoked;
        }
    }
}