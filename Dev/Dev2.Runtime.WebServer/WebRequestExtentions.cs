#pragma warning disable
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
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.WebServer.TransferObjects;

namespace Dev2.Runtime.WebServer
{
    static class WebRequestExtentions
    {
        public static void BindRequestVariablesToDataObject( this WebRequestTO request, ref IDSFDataObject dataObject)
        {
            if (dataObject == null || request == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(request.Bookmark))
            {
                dataObject.CurrentBookmarkName = request.Bookmark;
            }

            if (!string.IsNullOrEmpty(request.InstanceID) && Guid.TryParse(request.InstanceID, out Guid tmpId))
            {
                dataObject.WorkflowInstanceId = tmpId;
            }

            if (!string.IsNullOrEmpty(request.ServiceName) && string.IsNullOrEmpty(dataObject.ServiceName))
            {
                dataObject.ServiceName = request.ServiceName;
            }
            foreach (string key in request.Variables)
            {
                dataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(key), request.Variables[key], 0);
            }
        }

        public static string GetPathForAllResources(this WebRequestTO webRequest)
        {
            var uri = new Uri(webRequest.WebServerUrl);
            if (!uri.IsAbsoluteUri)
            {
                throw new Exception("expected absolute uri");
            }
            var path = uri.AbsolutePath;
            var isCoverageReport = path.EndsWith(".coverage") || path.EndsWith(".coverage.json");
            var isTestReport = path.EndsWith(".tests") || path.EndsWith(".tests.trx");
            if (!isCoverageReport && !isTestReport)
            {
                return path;
            }

            var publicValue = webRequest.Variables["isPublic"];
            var isPublic = bool.Parse(publicValue ?? "False");
            var firstForwardSlash = path.IndexOf('/', 1);
            if (firstForwardSlash > 0)
            {
                path = path.Substring(firstForwardSlash + 1);
            }

            var lastForwardSlash = path.LastIndexOf('/');
            if (lastForwardSlash > 0)
            {
                path = path.Substring(0, lastForwardSlash);
            }

            //var path = isPublic ? RemoveAccessType(webServerUrl, "public", "Public") : RemoveAccessType(webServerUrl, "secure", "Secure");
            //path = path.TrimStart('/').TrimEnd('/');
            return path.Replace("/", "\\");
        }

        private static string RemoveAccessType(string webServerUrl, string lower, string firstCap)
        {
            var startIndex = webServerUrl.IndexOf(lower + "/", StringComparison.InvariantCultureIgnoreCase);
            var removeEmitionType = webServerUrl.Substring(startIndex)
                .Replace("/.tests.trx", "")
                .Replace("/.tests", "")
                .Replace("/.coverage.json", "")
                .Replace("/.coverage", "");
            var removeAccessType = removeEmitionType.Replace(lower, "").Replace(firstCap, "");
            return removeAccessType;
        }
    }
}
