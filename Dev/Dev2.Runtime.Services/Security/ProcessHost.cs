
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
