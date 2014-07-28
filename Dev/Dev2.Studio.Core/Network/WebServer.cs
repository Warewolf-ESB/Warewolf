using System;
using System.Diagnostics;
using System.Net.Sockets;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Network
{
    #region WebServerMethod

    public enum WebServerMethod
    {
        // ReSharper disable InconsistentNaming
        POST,
        GET
    }

    #endregion

    public enum UrlType
    {
        XML,
        JSON
    }

    public static class WebServer
    {

        public static void Send(WebServerMethod method, IContextualResourceModel resourceModel, string payload, IAsyncWorker asyncWorker)
        {
            if(resourceModel == null || resourceModel.Environment == null || !resourceModel.Environment.IsConnected)
            {
                return;
            }

            var clientContext = resourceModel.Environment.Connection;
            if(clientContext == null)
            {
                return;
            }
            asyncWorker.Start(() =>
            {
                var controller = new CommunicationController { ServiceName = resourceModel.Category };
                controller.AddPayloadArgument("DebugPayload", payload);
                controller.ExecuteCommand<string>(clientContext, clientContext.WorkspaceID);
            }, () => { });

        }

        public static bool IsServerUp(IContextualResourceModel resourceModel)
        {
            string host = resourceModel.Environment.Connection.WebServerUri.AbsoluteUri;
            int port = resourceModel.Environment.Connection.WebServerUri.Port;
            try
            {
                // Do NOT use TcpClient(ip, port) else it causes a 1 second delay when you initially resolve to an IPv6 IP
                // http://msdn.microsoft.com/en-us/library/115ytk56.aspx - Remarks
                using(TcpClient client = new TcpClient())
                {
                    client.Connect(host, port);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void OpenInBrowser(WebServerMethod post, IContextualResourceModel resourceModel, string xmlData)
        {
            Uri url = GetWorkflowUri(resourceModel, xmlData, UrlType.XML);
            if(url != null)
            {
                Process.Start(url.ToString());
            }
        }

        public static Uri GetWorkflowUri(IContextualResourceModel resourceModel, string xmlData, UrlType urlType)
        {
            if(resourceModel == null || resourceModel.Environment == null || resourceModel.Environment.Connection == null || !resourceModel.Environment.IsConnected)
            {
                return null;
            }
            var environmentConnection = resourceModel.Environment.Connection;

            string urlExtension = "xml";
            switch(urlType)
            {
                case UrlType.XML:
                    break;
                case UrlType.JSON:
                    urlExtension = "json";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("urlType");
            }

            var relativeUrl = string.Format("/services/{0}.{1}?", resourceModel.Category, urlExtension);
            relativeUrl += xmlData;
            relativeUrl += "&wid=" + environmentConnection.WorkspaceID;
            Uri url;
            Uri.TryCreate(environmentConnection.WebServerUri, relativeUrl, out url);
            return url;
        }
    }
}
