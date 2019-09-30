/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;

namespace Warewolf.Logging
{
    public enum LogLevel
    {
        [Description("None: No logging")]
        None,
        [Description("Fatal: Only log events that are fatal")]
        Fatal,
        [Description("Error: Log fatal and error events")]
        Error,
        [Description("Warn: Log error, fatal and warning events")]
        Warn,
        [Description("Info: Log system info including pulse data, fatal, error and warning events")]
        Info,
        [Description("Debug: Log all system activity including executions. Also logs fatal, error, warning and info events")]
        Debug,
        [Description("Trace: Log detailed system information. Includes events from all the levels above")]
        Trace
    }
}
