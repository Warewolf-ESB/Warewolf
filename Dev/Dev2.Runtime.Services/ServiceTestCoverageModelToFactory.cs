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
using Dev2.Data;
using Dev2.Runtime.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Runtime
{
    public class ServiceTestCoverageModelToFactory : IServiceTestCoverageModelToFactory
    {
        private readonly IResourceCatalog _resourceCatalog;

        public ServiceTestCoverageModelToFactory(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }
        public IServiceTestCoverageModelTo New(Guid workflowId, ICoverageArgs args, List<IServiceTestModelTO> serviceTestModelTos)
        {
            var workflow = _resourceCatalog.GetWorkflow(workflowId);
            var coverageReports = new WorkflowCoverageReports(workflow);
            var coverageModelTo = new ServiceTestCoverageModelTo
            {
                WorkflowId = workflowId,
                OldReportName = args?.OldReportName,
                ReportName = args?.ReportName,
                LastRunDate = DateTime.Now,
                AllTestNodesCovered = serviceTestModelTos
                .Select(test => new SingleTestNodesCovered(test.TestName, test.TestSteps))
                .ToArray()
            };
            coverageReports.Add(coverageModelTo);

            (double TotalCoverage, _, _) = coverageReports.GetTotalCoverage();

            coverageModelTo.TotalCoverage = TotalCoverage;

            return coverageModelTo;
        }
    }
}
