/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Runtime.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Data;

namespace Dev2.Data
{
    public class WarewolfWorkflowReports
    {
        private readonly IEnumerable<IWarewolfWorkflow> _workflows;
        private List<WorkflowCoverageReports> _workflowCoverageReports;
        private List<WorkflowTestResults> _workflowTestResults;
        private readonly string _reportName;

        public WarewolfWorkflowReports(IEnumerable<IWarewolfWorkflow> coverageReportResources, string reportName)
        {
            _workflows = coverageReportResources;
            _reportName = reportName;
        }

        public IEnumerable<IWarewolfWorkflow> Workflows => _workflows;

        public int TotalWorkflowNodesCount { get; private set; }

        public int TotalWorkflowNodesCoveredCount { get; private set; }

        public double TotalWorkflowNodesCoveredPercentage { get; private set; }


        public (IEnumerable<WorkflowTestResults> TestResults, IEnumerable<WorkflowCoverageReports> WorkflowCoverageReports) Calculte(ITestCoverageCatalog testCoverageCatalog, ITestCatalog testCatalog)
        {
            _workflowTestResults = new List<WorkflowTestResults>();
            _workflowCoverageReports = new List<WorkflowCoverageReports>();

            foreach (var coverageResource in _workflows)
            {
                if (coverageResource is null)
                {
                    continue;
                }

                SetWarewolfTestResults(testCatalog, coverageResource.ResourceID);
                SetWarewolfCoverageReports(testCoverageCatalog, coverageResource);

            }


            TotalWorkflowNodesCount = GetTotalWorkflowsNodesCount();
            TotalWorkflowNodesCoveredCount = GetTotalWorkflowNodesCoveredCount();
            TotalWorkflowNodesCoveredPercentage = GetTotalWorkflowNodesCoveredPercentage();

            return (_workflowTestResults, _workflowCoverageReports);

        }

        private void SetWarewolfCoverageReports(ITestCoverageCatalog testCoverageCatalog, IWarewolfWorkflow coverageResource)
        {
            var resourceId = coverageResource.ResourceID;
            var coverageReports = new WorkflowCoverageReports(coverageResource);
            if (!string.IsNullOrEmpty(_reportName) && _reportName != "*")
            {
                var report = testCoverageCatalog.Fetch(resourceId);
                var tempcoverageReport = report?.Find(oo => oo.ReportName?.ToUpper() == _reportName.ToUpper());
                if (tempcoverageReport != null)
                {
                    coverageReports.Add(tempcoverageReport);
                }
            }
            else
            {
                testCoverageCatalog.Fetch(resourceId)
                 ?.ForEach(o => coverageReports.Add(o));
            }

            _workflowCoverageReports.Add(coverageReports);
        }

        private void SetWarewolfTestResults(ITestCatalog testCatalog, Guid coverageResourceId)
        {
            var workflowTestResults = new WorkflowTestResults();

            testCatalog.Fetch(coverageResourceId)
                ?.ForEach(o => workflowTestResults.Add(o));

            _workflowTestResults.Add(workflowTestResults);
        }

        private double GetTotalWorkflowNodesCoveredPercentage()
        {
            var calc = (double)TotalWorkflowNodesCoveredCount / GetOneOnZero(TotalWorkflowNodesCount);
            var retult = Math.Round(calc * 100, 2);
            return retult;
        }

        private double GetOneOnZero(int count)
        {
            var wNodesCount = (double)count;
            var total = wNodesCount == 0 ? 1 : wNodesCount;
            return total;
        }

        private int GetTotalWorkflowNodesCoveredCount()
        {
            return _workflowCoverageReports.Sum(o => o.CoveredWorkflowNodesIds.Count());
        }

        private int GetTotalWorkflowsNodesCount()
        {
            return _workflows.Where(o => o != null).Sum(o => o.WorkflowNodes.Count());
        }
    }
}
