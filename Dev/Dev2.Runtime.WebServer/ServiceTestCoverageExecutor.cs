/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Communication;
using Dev2.Data.Decision;
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
        public static DataListFormat GetTestCoverageReports(IDSFDataObject dataObject, Guid workspaceGuid, Dev2JsonSerializer serializer, ITestCoverageCatalog testCoverageCatalog, IResourceCatalog resourceCatalog, out string executePayload)
        {
            DataListFormat formatter = null;
            if (dataObject.TestsResourceIds?.Any() ?? false)
            {
                if (dataObject.ReturnType == EmitionTypes.CoverJson)
                {
                    formatter = dataObject.RunCoverageAndReturnJSON(testCoverageCatalog, resourceCatalog, workspaceGuid, serializer, out executePayload);
                }
                else if (dataObject.ReturnType == EmitionTypes.Cover)
                {
                    formatter = dataObject.RunCoverageAndReturnHTML(testCoverageCatalog, resourceCatalog, workspaceGuid, serializer, out executePayload);
                }
                else
                {
                    executePayload = null;
                }
                dataObject.ResourceID = Guid.Empty;
            }
            else
            {
                executePayload = null;
                throw new Exception("do not expect this to be executed any longer");
            }

            Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
            dataObject.Environment = null;
            return formatter;
        }
    }
}
