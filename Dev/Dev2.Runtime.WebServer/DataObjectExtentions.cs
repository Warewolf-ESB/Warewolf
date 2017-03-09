using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using Dev2.Common;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Web;

namespace Dev2.Runtime.WebServer
{
    internal static class DataObjectExtentions
    {
        public static string SetEmitionType(this IDSFDataObject dataObject, string serviceName, NameValueCollection headers)
        {
            int loc;
            if (!string.IsNullOrEmpty(serviceName) && (loc = serviceName.LastIndexOf(".", StringComparison.Ordinal)) > 0)
            {
                // default it to xml
                dataObject.ReturnType = EmitionTypes.XML;

                if (loc > 0)
                {
                    var typeOf = serviceName.Substring(loc + 1).ToUpper();
                    EmitionTypes myType;
                    if (Enum.TryParse(typeOf, out myType))
                    {
                        dataObject.ReturnType = myType;
                    }

                    if (typeOf.StartsWith("tests", StringComparison.InvariantCultureIgnoreCase))
                    {
                        dataObject.IsServiceTestExecution = true;
                        var idx = serviceName.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
                        if (idx > loc)
                        {
                            var testName = serviceName.Substring(idx + 1).ToUpper();
                            dataObject.TestName = string.IsNullOrEmpty(testName) ? "*" : testName;
                        }
                        else
                        {
                            dataObject.TestName = "*";
                        }
                        dataObject.ReturnType = EmitionTypes.TEST;
                    }

                    if (typeOf.Equals("api", StringComparison.OrdinalIgnoreCase))
                    {
                        dataObject.ReturnType = EmitionTypes.SWAGGER;
                    }
                    serviceName = serviceName.Substring(0, loc);
                    dataObject.ServiceName = serviceName;
                }
            }
            else
            {
                dataObject.SetContentType(headers);
            }
            return serviceName;
        }

        public static void SetContentType(this IDSFDataObject dataObject, NameValueCollection headers)
        {
            if (headers != null)
            {
                var contentType = headers.Get("Content-Type");
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = headers.Get("ContentType");
                }
                if (!string.IsNullOrEmpty(contentType) && !dataObject.IsServiceTestExecution)
                {
                    if (contentType.ToLowerInvariant().Contains("json"))
                    {
                        dataObject.ReturnType = EmitionTypes.JSON;
                    }
                    if (contentType.ToLowerInvariant().Contains("xml"))
                    {
                        dataObject.ReturnType = EmitionTypes.XML;
                    }
                }
            }
            else
            {
                dataObject.ReturnType = EmitionTypes.XML;
            }
        }

        public static void RemoteInvoke(this IDSFDataObject dataObject, NameValueCollection headers)
        {
            Dev2Logger.Debug("Remote Invoke");

            var isRemote = headers.Get(HttpRequestHeader.Cookie.ToString());
            var remoteId = headers.Get(HttpRequestHeader.From.ToString());

            if (isRemote != null && remoteId != null)
            {
                if (isRemote.Equals(GlobalConstants.RemoteServerInvoke))
                {
                    // we have a remote invoke ;)
                    dataObject.RemoteInvoke = true;
                }
                if (isRemote.Equals(GlobalConstants.RemoteDebugServerInvoke))
                {
                    // we have a remote invoke ;)
                    dataObject.RemoteNonDebugInvoke = true;
                }

                dataObject.RemoteInvokerID = remoteId;
            }
        }

        public static void SetTestResourceIds(this IDSFDataObject dataObject, IResourceCatalog catalog, WebRequestTO webRequest)
        {
            var pathOfAllResources = webRequest.GetPathForAllResources();
            dataObject.ResourceID = Guid.Empty;
            if (string.IsNullOrEmpty(pathOfAllResources))
            {
                var resources = catalog.GetResources(GlobalConstants.ServerWorkspaceID);
                dataObject.TestsResourceIds = resources.Select(p => p.ResourceID).ToList();
            }
            else
            {
                var resources = catalog.GetResources(GlobalConstants.ServerWorkspaceID);
                var resourcesToRunTestsFor = resources?.Where(a => a.GetResourcePath(GlobalConstants.ServerWorkspaceID)
                                               .StartsWith(pathOfAllResources, StringComparison.InvariantCultureIgnoreCase));
                dataObject.TestsResourceIds = resourcesToRunTestsFor?.Select(p => p.ResourceID).ToList();
            }
        }

    }
}
