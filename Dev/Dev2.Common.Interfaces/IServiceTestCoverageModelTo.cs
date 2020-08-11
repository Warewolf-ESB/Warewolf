/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IServiceTestCoverageModelTo
    {
        ISingleTestNodesCovered[] AllTestNodesCovered { get; }
        string OldReportName { get; }
        string ReportName { get; }
        Guid WorkflowId { get; }
        double TotalCoverage { get; set; }
        DateTime LastRunDate { get; }
    }

    public interface ICoverageArgs
    {
        string OldReportName { get; set; }
        string ReportName { get; set; }
    }
    public class CoverageArgs : ICoverageArgs
    {
        public string OldReportName { get; set; }
        public string ReportName { get; set; }
    }

    public interface IServiceTestCoverageModelToFactory
    {
        IServiceTestCoverageModelTo New(Guid workflowId, ICoverageArgs args, List<IServiceTestModelTO> serviceTestModelTos);
    }
}
