/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



using System;
using System.Collections.Generic;
using Warewolf.Data;

namespace Dev2.Common.Interfaces.Runtime.WebServer
{
    public interface IWorkflowCoverageReportsTO
    {
        IWorkflowNode[] CoveredWorkflowNodes { get; }
        IEnumerable<Guid> CoveredWorkflowNodesIds { get; }
        IEnumerable<Guid> CoveredWorkflowNodesMockedIds { get; }
        IEnumerable<Guid> CoveredWorkflowNodesNotMockedIds { get; }
        bool HasTestReports { get; }
        int NotCoveredNodesCount { get; }
        List<IServiceTestCoverageModelTo> Reports { get; }
        IWarewolfWorkflow Resource { get; }
        double TotalCoverage { get; }
        IEnumerable<IWorkflowNode> WorkflowNodes { get; }
    }
}
