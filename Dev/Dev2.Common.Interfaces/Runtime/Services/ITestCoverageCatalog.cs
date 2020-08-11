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

namespace Dev2.Common.Interfaces.Runtime.Services
{
    public interface ITestCoverageCatalog
    {
        IServiceTestCoverageModelTo GenerateSingleTestCoverage(Guid resourceID, IServiceTestModelTO test);
        IServiceTestCoverageModelTo GenerateAllTestsCoverage(string resourceName, Guid resourceId, List<IServiceTestModelTO> serviceTestModelTos);
        IServiceTestCoverageModelTo FetchReport(Guid workflowId, string reportName);
        void ReloadAllReports();
        void DeleteAllCoverageReports(Guid workflowId);
        List<IServiceTestCoverageModelTo> Fetch(Guid coverageResourceId);
    }
}
