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
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Runtime.WebServer;
using Warewolf.Data;

namespace Dev2.Data
{
    public class WorkflowCoverageReports : IWorkflowCoverageReports
    {
        public WorkflowCoverageReports(IWarewolfWorkflow resource)
        {
            Resource = resource;
        }

        public List<IServiceTestCoverageModelTo> Reports { get; private set; } = new List<IServiceTestCoverageModelTo>();
        public bool HasTestReports => Reports.ToList().Count > 0;
        public IWarewolfWorkflow Resource { get; }
        public IEnumerable<IWorkflowNode> WorkflowNodes => Resource.WorkflowNodes;
        public IWorkflowNode[] CoveredWorkflowNodes => CalculateCoveredWorkflowNodes();
        public IEnumerable<Guid> CoveredWorkflowNodesNotMockedIds => CalculateCoveredWorkflowNodesNotMockedIds();
        public IEnumerable<Guid> CoveredWorkflowNodesMockedIds => CalculateCoveredWorkflowNodesMockedIds();
        public IEnumerable<Guid> CoveredWorkflowNodesIds => CalculateCoveredWorkflowNodesIds();
        public double TotalCoverage => GetTotalCoverage();

        public void Add(IServiceTestCoverageModelTo coverage)
        {
            Reports.Add(coverage);
        }

        private IEnumerable<Guid> CalculateCoveredWorkflowNodesNotMockedIds()
        {
            return CoveredWorkflowNodes
                .Where(o => o.MockSelected is false)
                .Select(o => o.ActivityID)
                .Distinct().ToList();
        }

        private IEnumerable<Guid> CalculateCoveredWorkflowNodesMockedIds()
        {
            return CoveredWorkflowNodes
                .Where(o => o.MockSelected is true)
                .Select(o => o.ActivityID)
                .Distinct().ToList();
        }

        private IEnumerable<Guid> CalculateCoveredWorkflowNodesIds()
        {
            return CoveredWorkflowNodes
                .Select(o => o.ActivityID)
                .Distinct().ToList();
        }

        private IWorkflowNode[] CalculateCoveredWorkflowNodes()
        {
            return Reports
                .SelectMany(o => o.AllTestNodesCovered)
                .SelectMany(o => o.TestNodesCovered)
                .GroupBy(n => n.ActivityID)
                .Select(o => o.First()).ToArray();
        }

        private double GetTotalCoverage()
        {
            var accum2 = WorkflowNodes.Select(o => o.UniqueID).ToList();
            var activitiesExistingInTests = accum2.Intersect(CoveredWorkflowNodesIds).ToList();
            var total = Math.Round(activitiesExistingInTests.Count / (double)accum2.Count, 2);
            return total;
        }
    }
}
