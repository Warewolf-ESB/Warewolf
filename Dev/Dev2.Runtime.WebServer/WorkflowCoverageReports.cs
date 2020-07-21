/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Data;

namespace Dev2.Runtime.WebServer
{
    internal class WorkflowCoverageReports
    {
        public WorkflowCoverageReports(IWarewolfWorkflow resource)
        {
            Resource = resource;
        }

        public List<IServiceTestCoverageModelTo> Reports { get; } = new List<IServiceTestCoverageModelTo>();
        public bool HasTestReports => Reports.Count > 0;
        public IWarewolfWorkflow Resource { get; }

        internal void Add(IServiceTestCoverageModelTo coverage)
        {
            Reports.Add(coverage);
        }

        public (double TotalCoverage, List<IWorkflowNode> AllWorkflowNodes, IWorkflowNode[] CoveredNodes) GetTotalCoverage()
        {
            var coveredNodes = Reports
                .SelectMany(o => o.AllTestNodesCovered)
                .SelectMany(o => o.TestNodesCovered)
                .GroupBy(n => n.ActivityID)
                .Select(o => o.First()).ToArray();

            var accum = coveredNodes
                .Where(o => o.MockSelected is false)
                .Select(o => o.ActivityID)
                .Distinct().ToList();
            var allWorkflowNodes = Resource.WorkflowNodes;
            var accum2 = allWorkflowNodes.Select(o => o.UniqueID).ToList();
            var activitiesExistingInTests = accum2.Intersect(accum).ToList();
            var total = Math.Round(activitiesExistingInTests.Count / (double)accum2.Count, 2);
            return (total, allWorkflowNodes, coveredNodes);
        }
    }
}
