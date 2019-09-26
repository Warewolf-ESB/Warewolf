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
using Warewolf.OS;

namespace Dev2
{
    public class LoggingServiceMonitor : ProcessMonitor
    {
        public LoggingServiceMonitor(IChildProcessTracker childProcessTracker, IProcessFactory processFactory, IJobConfig config)
            : base(childProcessTracker, processFactory, config)
        {
        }

        protected override ProcessStartInfo GetProcessStartInfo()
        {
            return new ProcessStartInfo("WarewolfLogger.exe");
        }
    }
}
