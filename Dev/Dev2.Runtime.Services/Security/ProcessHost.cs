#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics;

namespace Dev2.Runtime.Security
{
    public static class ProcessHost
    {
        /// <summary>
        /// Invokes Process.Start to start/run a process (application or batch file)
        /// </summary>
        /// <param name="workingDir">s</param>
        /// <param name="fileName"></param>
        /// <param name="args"></param>
        /// <param name="useShellExecute"></param>
        /// <returns></returns>
#if NETFRAMEWORK
        public static bool Invoke(string workingDir, string fileName, string args)
#else
        public static bool Invoke(string workingDir, string fileName, string args, bool useShellExecute = false)
#endif
        {
            var invoked = true;

            var p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = fileName,
#if NETFRAMEWORK
                    Verb = "runas"
#else
                    Verb = "runas",
                    UseShellExecute = true
#endif
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
