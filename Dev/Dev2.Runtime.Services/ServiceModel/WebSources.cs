/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Warewolf.Common.Interfaces.NetStandard20;
using Warewolf.Common.NetStandard20;

namespace Dev2.Runtime.ServiceModel
{
    public delegate string WebExecuteString(WebSource source, WebRequestMethod method, string relativeUri, string data, bool throwError, out ErrorResultTO errors, string[] headers = null);
    public delegate string WebExecuteBinary(WebSource source, WebRequestMethod method, string relativeUri, byte[] data, bool throwError, out ErrorResultTO errors, string[] headers = null);

    public class WebSources : ExceptionManager
    {
        readonly IResourceCatalog _resourceCatalog;

        public WebSources()
            : this(ResourceCatalog.Instance)
        {
        }

        public WebSources(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog ?? throw new ArgumentNullException(nameof(resourceCatalog));
        }

        public IWebSource Get(string resourceId, Guid workspaceId)
        {
            var result = new WebSource();
            try
            {
                var xmlStr = _resourceCatalog.GetResourceContents(workspaceId, Guid.Parse(resourceId)).ToString();
                if (!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    result = new WebSource(xml);
                }
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        public ValidationResult Test(string args)
        {
            try
            {
                var source = JsonConvert.DeserializeObject<WebSource>(args);
                return CanConnectServer(source);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message };
            }
        }

        public ValidationResult Test(IWebSource source)
        {
            try
            {
                return CanConnectServer(source);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message };
            }
        }

        ValidationResult CanConnectServer(IWebSource source)
        {
            try
            {
                return new ValidationResult
                {
                    Result = Execute(source, WebRequestMethod.Get, source.DefaultQuery, (string)null, true, out ErrorResultTO errors)
                };
            }
            catch (WebException wex)
            {
                RaiseError(wex);

                var errors = new StringBuilder();
                Exception ex = wex;
                while (ex != null)
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
            finally
            {
                source.DisposeClient();
            }
        }

        public static string Execute(IWebSource source, WebRequestMethod method, string relativeUri, string data, bool throwError, out ErrorResultTO errors) => Execute(source, method, relativeUri, data, throwError, out errors, null);
        public static string Execute(IWebSource source, WebRequestMethod method, string relativeUri, string data, bool throwError, out ErrorResultTO errors, string[] headers)
        {
            return Execute(source, method, headers, relativeUri, data, throwError, out errors);
        }

        public static string Execute(IWebSource source, WebRequestMethod method, IEnumerable<string> headers, string relativeUrl, string data, bool throwError, out ErrorResultTO errors)
        {
            var client = CreateWebClient(source.AuthenticationType, source.UserName, source.Password, source.Client, headers);

            errors = new ErrorResultTO();
            try
            {
                var address = GetAddress(source, relativeUrl);
                var contentType = client.Headers[HttpRequestHeader.ContentType];
                if (contentType != null && contentType.ToLowerInvariant().Contains("multipart"))
                {
                    return PerformMultipartWebRequest(new WebRequestFactory(), client, address, data);
                }

                return method == WebRequestMethod.Get ? client.DownloadData(address).ToBase64String() : client.UploadData(address, method.ToString().ToUpperInvariant(), data.ToBytesArray()).ToBase64String();
            }
            catch (WebException webex) when (webex.Response is HttpWebResponse httpResponse)
            {
                errors.AddError(webex.Message);
                using (var responseStream = httpResponse.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream);
                    return reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                errors.AddError(e.Message);
                if (throwError)
                {
                    throw;
                }
            }
            finally
            {
                client.Dispose();
            }

            return string.Empty;
        }

        static string GetAddress(IWebSource source, string relativeUri)
        {
            if (source == null)
            {
                if (string.IsNullOrEmpty(relativeUri))
                {
                    return "";
                }
                return relativeUri;
            }
            if (!string.IsNullOrEmpty(source.Address) && relativeUri.Contains(source.Address))
            {
                return relativeUri;
            }
            return $"{source.Address}{relativeUri}";
        }

        public static byte[] Execute(IWebSource source, WebRequestMethod method, string relativeUri, byte[] data, out ErrorResultTO errors) => Execute(source, method, relativeUri, data, out errors, null);
        public static byte[] Execute(IWebSource source, WebRequestMethod method, string relativeUri, byte[] data, out ErrorResultTO errors, string[] headers)
        {
            return Execute(source, relativeUri, method, headers, data, out errors);
        }

        static byte[] Execute(IWebSource source, string relativeUri, WebRequestMethod method, IEnumerable<string> headers, byte[] data, out ErrorResultTO errors)
        {
            var client = CreateWebClient(source.AuthenticationType, source.UserName, source.Password,source.Client, headers);

            var address = GetAddress(source, relativeUri);
            errors = new ErrorResultTO();
            return method == WebRequestMethod.Get ? client.DownloadData(address) : client.UploadData(address, method.ToString().ToUpperInvariant(), data);
        }

        public static string PerformMultipartWebRequest(IWebRequestFactory webRequestFactory, IWebClientWrapper client, string address, string data)
        {
            var wr = webRequestFactory.New(address);
            wr.Headers[HttpRequestHeader.Authorization] = client.Headers[HttpRequestHeader.Authorization];
            wr.ContentType = client.Headers[HttpRequestHeader.ContentType];
            wr.Method = "POST";
            var byteData = ConvertToHttpNewLine(ref data);
            wr.ContentLength = byteData.Length;

            using (var requestStream = wr.GetRequestStream())
            {
                requestStream.Write(byteData, 0, byteData.Length);
                requestStream.Close();
            }

            using (var wresp = wr.GetResponse())
            {
                if (wresp.StatusCode == HttpStatusCode.OK)
                {
                    using (var responseStream = wresp.GetResponseStream())
                    {
                        if (responseStream == null)
                        {
                            return null;
                        }
                        using (var responseReader = new StreamReader(responseStream))
                        {
                            return responseReader.ReadToEnd();
                        }
                    }
                }

                throw new ApplicationException("Error while upload files. Server status code: " + wresp.StatusCode);
            }
        }

        //TODO: ConvertToHttpNewLine should now be made private and tested using IWebRequest  
        internal static byte[] ConvertToHttpNewLine(ref string data)
        {
            data = data.Replace("\r\n", "\n");
            data = data.Replace("\n", GlobalConstants.HTTPNewLine);
            var byteData = data.ToBytesArray();
            return byteData;
        }

        private static IWebClientWrapper CreateWebClient(AuthenticationType authenticationType, string userName, string password, IWebClientWrapper webClient, IEnumerable<string> headers)
        {
            if (webClient is null)
            {
                webClient = new WebClientWrapper();
            }

            if (authenticationType == AuthenticationType.User)
            {
                webClient.Credentials = new NetworkCredential(userName, password);
            }
            webClient.Headers.Add("user-agent", GlobalConstants.UserAgentString);
            AddHeaders(webClient, headers);
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            return webClient;
        }

        private static void AddHeaders(IWebClientWrapper webClient, IEnumerable<string> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (header != ":")
                    {
                        webClient.Headers.Add(header.Trim());
                    }
                }
            }
        }
    }
}
