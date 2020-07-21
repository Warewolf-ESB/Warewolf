/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Dev2.Runtime.WebServer
{
    internal class AllCoverageReports
    {
        public List<WorkflowCoverageReports> AllCoverageReportsSummary { get; } = new List<WorkflowCoverageReports>();
        public JToken StartTime { get; internal set; }
        public JToken EndTime { get; internal set; }

        internal void Add(WorkflowCoverageReports item)
        {
            AllCoverageReportsSummary.Add(item);
        }
    }

}
