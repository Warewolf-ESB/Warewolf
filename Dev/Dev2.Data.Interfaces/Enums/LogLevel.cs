#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;

namespace Dev2.Data.Interfaces.Enums
{
    public enum LogLevel
    {
        // ReSharper disable InconsistentNaming
        [Description("None: No logging")]
        OFF,
        [Description("Fatal: Only log events that are fatal")]
        FATAL,
        [Description("Error: Log fatal and error events")]
        ERROR,
        [Description("Warn: Log error, fatal and warning events")]
        WARN,
        [Description("Info: Log system info including pulse data, fatal, error and warning events")]
        INFO,
        [Description("Debug: Log all system activity including executions. Also logs fatal, error, warning and info events")]
        DEBUG,
        [Description("Trace: Log detailed system information. Includes events from all the levels above")]
        TRACE
    }
}