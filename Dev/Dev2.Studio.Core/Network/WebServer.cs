using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Dev2.Studio.Core.Interfaces;
using Dev2.Util;

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
        public static void SendAsync(WebServerMethod method, IContextualResourceModel resourceModel, string payload)
        {
            Send(method, resourceModel, payload, args => { });
        }

        public static void SendAsync(WebServerMethod method, IContextualResourceModel resourceModel, string payload, Action<UploadStringCompletedEventArgs> asyncCallback)
        {
            Send(method, resourceModel, payload, asyncCallback);
        }

        public static void Send(WebServerMethod method, IContextualResourceModel resourceModel, string payload, Action<UploadStringCompletedEventArgs> asyncCallback = null)
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

            var data = string.Empty;

            var relativeUrl = string.Format("/services/{0}?wid={1}", resourceModel.ResourceName, clientContext.WorkspaceID);
            if(!string.IsNullOrEmpty(payload))
            {
                if(method == WebServerMethod.GET)
                {
                    relativeUrl += "&" + payload;
                }
                else
                {
                    data = payload;
                }
            }

            Uri url;
            if(!Uri.TryCreate(resourceModel.Environment.Connection.WebServerUri, relativeUrl, out url))
            {
                Uri.TryCreate(new Uri(AppSettings.LocalHost), relativeUrl, out url);
            }

            using(var webClient = new WebClient { Credentials = CredentialCache.DefaultCredentials, Encoding = Encoding.UTF8 })
            {
                if(asyncCallback == null)
                {
                    webClient.UploadString(url, method.ToString(), data);
                }
                else
                {
                    webClient.UploadStringCompleted += (sender, args) => asyncCallback(args);
                    webClient.UploadStringAsync(url, method.ToString(), data);
                }
            }
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
            var relativeUrl = string.Format("/services/{0}.xml?", resourceModel.ResourceName);
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
