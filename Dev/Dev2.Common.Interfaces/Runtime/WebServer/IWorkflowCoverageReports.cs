﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using Warewolf.Data;

namespace Dev2.Common.Interfaces.Runtime.WebServer
{
    public interface IWorkflowCoverageReports
    {
        IWarewolfWorkflow Resource { get; }
        bool HasTestReports { get; }
        List<IServiceTestCoverageModelTo> Reports { get; }

        (double TotalCoverage, List<IWorkflowNode> WorkflowNodes, IWorkflowNode[] CoveredNodes) GetTotalCoverage();
    }
}
