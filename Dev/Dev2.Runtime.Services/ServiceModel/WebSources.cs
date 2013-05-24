using System;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    // PBI 5656 - 2013.05.20 - TWR - Created
    public class WebSources : ExceptionManager
    {
        readonly IResourceCatalog _resourceCatalog;

        #region CTOR

        public WebSources()
            : this(ResourceCatalog.Instance)
        {
        }

        public WebSources(IResourceCatalog resourceCatalog)
        {
            if(resourceCatalog == null)
            {
                throw new ArgumentNullException("resourceCatalog");
            }
            _resourceCatalog = resourceCatalog;
        }

        #endregion

        #region Get

        // POST: Service/WebSources/Get
        public WebSource Get(string resourceID, Guid workspaceID, Guid dataListID)
        {
            var result = new WebSource();
            try
            {
                var xmlStr = Resources.ReadXml(workspaceID, ResourceType.WebSource, resourceID);
                if(!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    result = new WebSource(xml);
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

        #region Save

        // POST: Service/WebSources/Save
        public string Save(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var source = JsonConvert.DeserializeObject<WebSource>(args);

                _resourceCatalog.SaveResource(workspaceID, source);
                if(workspaceID != GlobalConstants.ServerWorkspaceID)
                {
                    _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, source);
                }

                return source.ToString();
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message }.ToString();
            }
        }

        #endregion

        #region Test

        // POST: Service/WebSources/Test
        public ValidationResult Test(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var source = JsonConvert.DeserializeObject<WebSource>(args);
                return CanConnectServer(source);
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message };
            }
        }

        #endregion

        #region CanConnectServer

        ValidationResult CanConnectServer(WebSource source)
        {
            try
            {
                return new ValidationResult
                {
                    Result = FixTestResult(Execute(source, WebRequestMethod.Get, source.DefaultQuery, (string)null))
                };
            }
            catch(WebException wex)
            {
                RaiseError(wex);

                var errors = new StringBuilder();
                Exception ex = wex;
                while(ex != null)
                {
                    errors.AppendFormat("{0} ", ex.Message);
                    ex = ex.InnerException;
                }
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = errors.ToString()
                };
            }
        }

        #endregion

        #region Execute

        public static string Execute(WebSource source, WebRequestMethod method, string relativeUri, string data)
        {
            var client = CreateWebClient(source);
            try
            {
                return Execute(client, string.Format("{0}{1}", source.Address, relativeUri), method, data);
            }
            finally
            {
                client.Dispose();
            }
        }


        public static byte[] Execute(WebSource source, WebRequestMethod method, string relativeUri, byte[] data)
        {
            var client = CreateWebClient(source);
            try
            {
                return Execute(client, string.Format("{0}{1}", source.Address, relativeUri), method, data);
            }
            finally
            {
                client.Dispose();
            }
        }

        #region CreateWebClient

        static WebClient CreateWebClient(WebSource source, string contentType = null)
        {
            var client = new WebClient();

            if(source.AuthenticationType == AuthenticationType.User)
            {
                client.Credentials = new NetworkCredential(source.UserName, source.Password);
            }

            if(!string.IsNullOrEmpty(contentType))
            {
                client.Headers.Add("Content-Type", contentType);
            }
            return client;
        }

        #endregion

        #endregion

        #region Execute(client, address, method, data)

        static byte[] Execute(WebClient client, string address, WebRequestMethod method, byte[] data)
        {
            EnsureContentType(client);
            switch(method)
            {
                case WebRequestMethod.Get:
                    return client.DownloadData(address);

                default:
                    return client.UploadData(address, method.ToString().ToUpperInvariant(), data);
            }
        }

        static string Execute(WebClient client, string address, WebRequestMethod method, string data)
        {
            EnsureContentType(client);
            switch(method)
            {
                case WebRequestMethod.Get:
                    return client.DownloadString(address);

                default:
                    return client.UploadString(address, method.ToString().ToUpperInvariant(), data);
            }
        }

        static void EnsureContentType(WebClient client)
        {
            var contentType = client.Headers["Content-Type"];
            if(string.IsNullOrEmpty(contentType))
            {
                contentType = "application/x-www-form-urlencoded";
            }
            client.Headers["Content-Type"] = contentType;
        }

        #endregion

        #region FixTestResult

        static string FixTestResult(string result)
        {
            if(string.IsNullOrEmpty(result))
            {
                return result;
            }
            return result;

            #region Html - get body content

            var idx = result.IndexOf("<body>", StringComparison.InvariantCultureIgnoreCase);
            if(idx != -1)
            {
                idx += 6; //
                result = result.Remove(0, idx);

                idx = result.IndexOf("</body>", StringComparison.InvariantCultureIgnoreCase);
                if(idx != -1)
                {
                    result = result.Remove(idx, result.Length - idx);
                }
            }

            #endregion

            return result;
        }

        #endregion

    }

}