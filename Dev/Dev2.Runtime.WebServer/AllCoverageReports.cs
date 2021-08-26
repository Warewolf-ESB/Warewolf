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
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Runtime.WebServer
{
    internal class AllCoverageReports
    {
        public JToken StartTime { get; internal set; }
        public JToken EndTime { get; internal set; }
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
                   .Sum(o => o.Reports
                   .Sum(x => x.TotalCoverage * 100));
        }

        private double CalculateTotalReportCount()
        {
            var count = WithTestReports.Count();
            return (count == 0) ? 1 : 100 * count;
        }

        private double CalculateTotalReportsCoverage()
        {
            var sumAllOfResports = CalculateSumOfAllReports();
            var totalReportCount = CalculateTotalReportCount();
            return sumAllOfResports / totalReportCount * 100;
        }
    }

}
