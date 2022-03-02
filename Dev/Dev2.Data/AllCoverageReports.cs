/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Runtime.WebServer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Data
{
    public class AllCoverageReports
    {
        public AllCoverageReports()
        {
            StartTime = DateTime.Now;
        }

        public JToken StartTime { get; }
        public JToken EndTime { get; set; }
        private List<IWorkflowCoverageReports> _allCoverageReportsSummary { get; } = new List<IWorkflowCoverageReports>();
        public IEnumerable<IWorkflowCoverageReports> WithTestReports => _allCoverageReportsSummary.Where(o => o.HasTestReports) ?? new List<IWorkflowCoverageReports>();

        public double TotalReportsCoverage => CalculateTotalReportsCoverage();


        internal void Add(IWorkflowCoverageReports item)
        {
            _allCoverageReportsSummary.Add(item);
        }

        private double CalculateSumOfAllReports()
        {
            return WithTestReports
                .Sum(x => x.TotalCoverage * 100);
        }

        private double CalculateTotalReportCount()
        {
            var count = WithTestReports.Count();
            return (count == 0) ? 1 : (count == 1) ? 1 : 100 * count;
        }

        private double CalculateTotalReportsCoverage()
        {
            var sumAllOfReports = CalculateSumOfAllReports();
            var totalReportCount = CalculateTotalReportCount();
            return (sumAllOfReports / (totalReportCount == 1 ? totalReportCount * 100 : totalReportCount)) * 100;
        }

        public IEnumerable<IWorkflowCoverageReportsTO> Calcute()
        {
            return WithTestReports
                   .Select(o => o.TryExecute());
        }
    }

}
