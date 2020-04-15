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
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Web;
using System;
using System.Linq;

namespace Dev2.Runtime.WebServer
{
    internal class CoverageDataContext : ICoverageDataObject
    {
        private readonly string _originalWebServerUrl;
        public EmitionTypes ReturnType { get; }
        public Guid ResourceID { get; }
        public Guid[] CoverageReportResourceIds { get; set; }

        public CoverageDataContext(Guid resourceID, EmitionTypes emissionType, string originalWebServerUrl)
        {
            ResourceID = resourceID;
            ReturnType = emissionType;
            _originalWebServerUrl = originalWebServerUrl;
        }

        public string GetTestUrl(string resourcePath)
        {
            var myUri = new Uri(_originalWebServerUrl);
            var security = "";
            foreach (var segment in myUri.Segments)
            {
                if (segment.Contains("public"))
                    security = segment;
                if (segment.Contains("secure"))
                    security = segment;
            }

            var filepath = resourcePath.Replace("\\", "/");
            var hostname = myUri.Scheme + "://" + myUri.Authority + "/" + security + filepath + ".tests";
            return hostname;
        }
    }

    public static class ServiceTestCoverageExecutor
    {
        public static DataListFormat GetTestCoverageReports(ICoverageDataObject coverageObject, Guid workspaceGuid, Dev2JsonSerializer serializer, ITestCoverageCatalog testCoverageCatalog, IResourceCatalog resourceCatalog, out string executePayload)
        {
            DataListFormat formatter = null;
            if (coverageObject.CoverageReportResourceIds.Any())
            {
                if (coverageObject.ReturnType == EmitionTypes.CoverJson)
                {
                    formatter = coverageObject.RunCoverageAndReturnJSON(testCoverageCatalog, resourceCatalog, workspaceGuid, serializer, out executePayload);
                }
                else if (coverageObject.ReturnType == EmitionTypes.Cover)
                {
                    formatter = coverageObject.RunCoverageAndReturnHTML(testCoverageCatalog, resourceCatalog, workspaceGuid, out executePayload);
                }
                else
                {
                    executePayload = null;
                }
            }
            else
            {
                executePayload = null;
                throw new Exception("do not expect this to be executed any longer");
            }
            return formatter;
        }
    }
}
