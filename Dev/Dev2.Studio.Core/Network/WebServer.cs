using System;
using System.Diagnostics;
using System.Net.Sockets;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Network
{
    #region WebServerMethod

    public enum WebServerMethod
    {
        // ReSharper disable once InconsistentNaming
        POST,
        GET
    }

    #endregion

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
                var controller = new CommunicationController();
                controller.ServiceName = resourceModel.Category;
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

        public static void OpenInBrowser(WebServerMethod post, IContextualResourceModel resourceModel, string xmlData, bool isXml)
        {
            if(resourceModel == null || resourceModel.Environment == null || !resourceModel.Environment.IsConnected)
            {
                return;
            }
            var relativeUrl = string.Format("/services/{0}.xml?", resourceModel.Category);
            if(isXml)
            {
                relativeUrl += xmlData;
            }
            else
            {
                relativeUrl += xmlData;
            }
            Uri url;
            Uri.TryCreate(resourceModel.Environment.Connection.WebServerUri, relativeUrl, out url);
            Process.Start(url.ToString());
        }
    }
}
