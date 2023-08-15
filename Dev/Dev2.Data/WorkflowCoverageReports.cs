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
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Runtime.WebServer;
using Warewolf.Data;

namespace Dev2.Data
{
    public class WorkflowCoverageReportsTO : IWorkflowCoverageReportsTO
    {
        public IWorkflowNode[] CoveredWorkflowNodes { get; set; }

        public IEnumerable<Guid> CoveredWorkflowNodesIds { get; set; }

        public IEnumerable<Guid> CoveredWorkflowNodesMockedIds { get; set; }

        public IEnumerable<Guid> CoveredWorkflowNodesNotMockedIds { get; set; }

        public bool HasTestReports { get; set; }

        public int NotCoveredNodesCount { get; set; }

        public List<IServiceTestCoverageModelTo> Reports { get; set; }

        public IWarewolfWorkflow Resource { get; set; }

        public double TotalCoverage { get; set; }

        public IEnumerable<IWorkflowNode> WorkflowNodes { get; set; }
    }

    public class WorkflowCoverageReports : IWorkflowCoverageReports
    {
        public WorkflowCoverageReports(IWarewolfWorkflow resource)
        {
            Resource = resource;
        }

        public List<IServiceTestCoverageModelTo> Reports { get; private set; } = new List<IServiceTestCoverageModelTo>();
        public bool HasTestReports => Reports.ToList().Count > 0;
        public IWarewolfWorkflow Resource { get; }
        public IEnumerable<IWorkflowNode> WorkflowNodes => CalculateWorkflowNodes();
        public IWorkflowNode[] CoveredWorkflowNodes => CalculateCoveredWorkflowNodes();
        public IEnumerable<Guid> CoveredWorkflowNodesNotMockedIds => CalculateCoveredWorkflowNodesNotMockedIds();
        public IEnumerable<Guid> CoveredWorkflowNodesMockedIds => CalculateCoveredWorkflowNodesMockedIds();
        public IEnumerable<Guid> CoveredWorkflowNodesIds => CalculateCoveredWorkflowNodesIds();
        public double TotalCoverage => GetTotalCoverage();

        //PBI: at this point we only need the count, later change this to a list of objects
        public int NotCoveredNodesCount => CalculateNotCoveredNodes();

        private int CalculateNotCoveredNodes()
        {
            return WorkflowNodes.Count() - CoveredWorkflowNodesIds.Count();
        }

        private int GetOneOnZero(int count)
        {
            return count == 0 ? 1 : count;
        }

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
                .SelectMany(oo => oo.TestNodesCovered)
                .Flatten(ooo => ooo.ChildNodes)
                .Distinct()
                .ToArray();
        }

        public IWorkflowCoverageReportsTO TryExecute()
        {
            try
            {
                return new WorkflowCoverageReportsTO
                {
                    CoveredWorkflowNodes = CoveredWorkflowNodes,
                    CoveredWorkflowNodesIds = CoveredWorkflowNodesIds,
                    CoveredWorkflowNodesMockedIds = CoveredWorkflowNodesMockedIds,
                    CoveredWorkflowNodesNotMockedIds = CoveredWorkflowNodesNotMockedIds,
                    HasTestReports = HasTestReports,
                    NotCoveredNodesCount = NotCoveredNodesCount,
                    Reports = Reports,
                    Resource = Resource,
                    TotalCoverage = TotalCoverage,
                    WorkflowNodes = WorkflowNodes
                };
            }
            catch (Exception)
            {
                Dev2Logger.Error("[Coverage] - Resource: "+Resource.ResourceName + " Failed. Details - ResourceId: " + Resource.ResourceID + " ResourcePath: "+ Resource.FilePath, GlobalConstants.WarewolfError);
                return default;
            }
        }

        private IWorkflowNode[] CalculateWorkflowNodes()
        {
            return Resource.WorkflowNodes
                .Flatten(o => o.ChildNodes)
                .Distinct()
                .ToArray();
        }

        private double GetTotalCoverage()
        {
            var accum2 = WorkflowNodes.Select(o => o.UniqueID).ToList();
            var activitiesExistingInTests = accum2.Intersect(CoveredWorkflowNodesIds).ToList();
            var total = Math.Round(activitiesExistingInTests.Count / (double)GetOneOnZero(accum2.Count), 2);
            return total;
        }
    }
}
