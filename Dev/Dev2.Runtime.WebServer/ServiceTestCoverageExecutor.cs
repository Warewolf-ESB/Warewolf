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
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Web;
using System;
using System.Linq;

namespace Dev2.Runtime.WebServer
{
    public static class ServiceTestCoverageExecutor
    {
        public static DataListFormat GetTestCoverageReports(ICoverageDataObject coverageObject, Guid workspaceGuid, Dev2JsonSerializer serializer, ITestCoverageCatalog testCoverageCatalog, ITestCatalog testCatalog, IResourceCatalog resourceCatalog, out string executePayload)
        {
            DataListFormat formatter = null;
            if (coverageObject.CoverageReportResourceIds?.Any() ?? false)
            {
                if (coverageObject.ReturnType == EmitionTypes.CoverJson)
                {
                    formatter = coverageObject.RunCoverageAndReturnJSON(testCoverageCatalog, testCatalog, resourceCatalog, workspaceGuid, serializer, out executePayload);
                }
                else if (coverageObject.ReturnType == EmitionTypes.Cover)
                {
                    formatter = coverageObject.RunCoverageAndReturnHTML(testCoverageCatalog, testCatalog, resourceCatalog, workspaceGuid, out executePayload);
                }
                else
                {
                    executePayload = null;
                }
            }
            else
            {
                executePayload = null;
                Common.Dev2Logger.Warn("No test coverage reports found to execute for requested resource", Common.GlobalConstants.WarewolfWarn);
            }
            return formatter ?? DataListFormat.CreateFormat("HTML", EmitionTypes.Cover, "text/html; charset=utf-8");

        }
    }
}
