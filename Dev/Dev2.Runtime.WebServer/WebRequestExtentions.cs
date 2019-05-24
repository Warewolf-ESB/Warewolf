#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
            if (dataObject != null && request != null)
            {
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
        }

        public static string GetPathForAllResources(this WebRequestTO webRequest)
        {
            var publicvalue = webRequest.Variables["isPublic"];
            var isPublic = bool.Parse(publicvalue ?? "False");
            var path = "";
            var webServerUrl = webRequest.WebServerUrl;
            if (isPublic)
            {
                var pathStartIndex = webServerUrl.IndexOf("public/", StringComparison.InvariantCultureIgnoreCase);
                path = webServerUrl.Substring(pathStartIndex)
                                   .Replace("/.tests.trx", "")
                                   .Replace("/.tests", "")
                                   .Replace("/.tests.trx", "")
                                   .Replace("public", "")
                                   .Replace("Public", "")
                                   .TrimStart('/')
                                   .TrimEnd('/');
            }
            if (!isPublic)
            {
                var pathStartIndex = webServerUrl.IndexOf("secure/", StringComparison.InvariantCultureIgnoreCase);
                path = webServerUrl.Substring(pathStartIndex)
                                    .Replace("/.tests.trx", "")
                                    .Replace("/.tests", "")
                                    .Replace("/.tests.trx", "")
                                    .Replace("secure", "")
                                    .Replace("Secure", "")
                                    .TrimStart('/')
                                    .TrimEnd('/');
            }
            return path.Replace("/", "\\");
        }
    }
}
