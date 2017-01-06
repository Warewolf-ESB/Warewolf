/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.ComponentModel;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    [Serializable]
    public enum DebugItemResultType
    {
        Label,
        Variable,
        Value
    }

    public enum ActivityType
    {
        [Description("Service")] Workflow,
        [Description("Step")] Step,
        [Description("Connector")] Service
    }

    public enum ExecutionOrigin
    {
        [Description("Unknown")] Unknown,
        [Description("Workflow")] Workflow,
        [Description("Debug")] Debug,
        [Description("External")] External
    }

    [Flags]
    public enum StateType
    {
        None = 0,
        Before = 1,
        After = 2,
        Append = 4,
        Message = 8,
        Clear = 16,
        Start = 32,
        End = 64,
        All = 128,
        TestAggregate = 256,
        Duration = 512
    }
}