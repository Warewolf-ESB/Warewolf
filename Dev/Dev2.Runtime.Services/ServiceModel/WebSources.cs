/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Runtime.DynamicProxy;
using Warewolf.Common.Interfaces.NetStandard20;
using Warewolf.Common.NetStandard20;
using Warewolf.Data.Options;
using System.Linq;

namespace Dev2.Runtime.ServiceModel
{
    public class WebExecuteStringArgs
    {
        public IEnumerable<IFormDataParameters> FormDataParameters { get; set; }
        public IWebRequestFactory WebRequestFactory { get; set; }
        public bool IsManualChecked { get; set; }
        public bool IsFormDataChecked { get; set; }
    }

    public delegate string WebExecuteString(WebSource source, WebRequestMethod method, string relativeUri, string data, bool throwError, out ErrorResultTO errors, string[] headers = null, WebExecuteStringArgs webExecuteStringArgs = null);
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
        public static string Execute(IWebSource source, WebRequestMethod method, string relativeUri, string data, bool throwError, out ErrorResultTO errors, string[] headers, WebExecuteStringArgs webExecuteStringArgs = null)
        {
            if (webExecuteStringArgs == null)
            {
                webExecuteStringArgs = new WebExecuteStringArgs
                {
                    IsManualChecked = true,
                    IsFormDataChecked = false,
                    FormDataParameters = new List<IFormDataParameters>(),
                    WebRequestFactory = new WebRequestFactory()
                };
            }
            return Execute(source, method, headers, relativeUri, webExecuteStringArgs.IsManualChecked, webExecuteStringArgs.IsFormDataChecked, data, throwError, out errors, webExecuteStringArgs?.FormDataParameters, webExecuteStringArgs.WebRequestFactory);
        }
        
        public static string Execute(IWebSource source, WebRequestMethod method, IEnumerable<string> headers, string relativeUrl, bool isNoneChecked, bool isFormDataChecked, string data, bool throwError, out ErrorResultTO errors, IEnumerable<IFormDataParameters> formDataParameters = null, IWebRequestFactory webRequestFactory = null)
        {
            IWebClientWrapper client = null;

            if (webRequestFactory == null)
            {
                webRequestFactory = new WebRequestFactory();
            }

            errors = new ErrorResultTO();
            try
            {
                ValidateSource(source);
                client = CreateWebClient(source.AuthenticationType, source.UserName, source.Password, source.Client, headers);
                var address = GetAddress(source, relativeUrl);
                var contentType = client.Headers[HttpRequestHeader.ContentType];

                if (isFormDataChecked)
                {
                    VerifyArgument.IsNotNullOrWhitespace("Content-Type", contentType);
                    var formDataBoundary = contentType.Split('=').Last();
                    var bytesData = GetMultipartFormData(formDataParameters, formDataBoundary);
                    return PerformMultipartWebRequest(webRequestFactory, client, address, bytesData);
                }
                if (isNoneChecked && (contentType != null && contentType.ToLowerInvariant().Contains("multipart")))
                {
                    var bytesData = ConvertToHttpNewLine(ref data);
                    return PerformMultipartWebRequest(webRequestFactory, client, address, bytesData);
                }

                switch (method)
                {
                    case WebRequestMethod.Get:
                        return client.DownloadData(address).ToBase64String();
                    case WebRequestMethod.Put:
                        return client.UploadData(address, method.ToString().ToUpperInvariant(), data.ToBytesArray()).ToBase64String();
                    default: //classic calls are handled here like: delete and post
                        return client.UploadString(address, method.ToString().ToUpperInvariant(), data);
                }
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
                client?.Dispose();
            }

            return string.Empty;
        }

        private static byte[] GetMultipartFormData(IEnumerable<IFormDataParameters> postParameters, string boundary)
        {
            var encoding = Encoding.UTF8;
            Stream formDataStream = new MemoryStream();
            bool needsCLRF = false;

            var dds = postParameters.GetEnumerator();
            while (dds.MoveNext())
            {
                var conditionExpression = dds.Current;
                var formValueType = conditionExpression;

                if (needsCLRF)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsCLRF = true;

                if (formValueType is FileParameter fileToUpload)
                {

                    var fileKey = fileToUpload.Key;
                    var header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        fileKey,
                        fileToUpload.FileName ?? fileKey,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    var fileBytes = fileToUpload.FileBytes;

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));
                    
                    formDataStream.Write(fileBytes, 0, fileBytes.Length);
                }
                else if (formValueType is TextParameter textToUpload)
                {

                    var postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        textToUpload.Key,
                        textToUpload.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            var footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            formDataStream.Position = 0;
            var formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }


        private static void ValidateSource(IWebSource source)
        {
            if (string.IsNullOrEmpty(source?.Address))
            {
                throw new Exception(Constants.ErrorMessages.WebAddressError);
            }
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

        public static string PerformMultipartWebRequest(IWebRequestFactory webRequestFactory, IWebClientWrapper client, string address, byte[] bytesData)
        {
            var wr = webRequestFactory.New(address);
            wr.Headers[HttpRequestHeader.Authorization] = client.Headers[HttpRequestHeader.Authorization];
            wr.ContentType = client.Headers[HttpRequestHeader.ContentType];
            wr.Method = "POST";
            wr.ContentLength = bytesData.Length;

            using (var requestStream = wr.GetRequestStream())
            {
                requestStream.Write(bytesData, 0, bytesData.Length);
                requestStream.Close();
            }

            using (var wresp = wr.GetResponse() as HttpWebResponse)
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

        private static byte[] ConvertToHttpNewLine(ref string data)
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
