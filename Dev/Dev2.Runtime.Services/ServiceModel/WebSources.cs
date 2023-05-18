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
using System.Web;

namespace Dev2.Runtime.ServiceModel
{
    public class WebExecuteStringArgs
    {
        public IEnumerable<IFormDataParameters> FormDataParameters { get; set; }
        public IWebRequestFactory WebRequestFactory { get; set; }
        public bool IsManualChecked { get; set; }
        public bool IsFormDataChecked { get; set; }
        public bool IsUrlEncodedChecked { get; set; }
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
                    Result = Execute(source, WebRequestMethod.Get, source.DefaultQuery, null, true, out var errors)
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
                    IsUrlEncodedChecked = false,
                    FormDataParameters = new List<IFormDataParameters>(),
                    WebRequestFactory = new WebRequestFactory()
                };
            }
            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsManualChecked", "true"));
            return Execute(source, method, headers, relativeUri, data, throwError, out errors, webExecuteStringArgs.FormDataParameters, webExecuteStringArgs.WebRequestFactory, settings: settings);
        }
        
        public static string Execute(IWebPostOptions options, out ErrorResultTO errors)
        {
            return Execute(source: options.Source, method: options.Method, headers: options.Headers, relativeUrl: options.Query,
                data: options.PostData, throwError: true, errors: out errors, formDataParameters: options.Parameters, settings: options.Settings, timeout: options.Timeout);
        }
        
        public static string Execute(IWebSource source, WebRequestMethod method, IEnumerable<string> headers, string relativeUrl,
            bool isNoneChecked, bool isFormDataChecked, string data, bool throwError, out ErrorResultTO errors,
            IEnumerable<IFormDataParameters> formDataParameters = null)
        {
            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsManualChecked", isNoneChecked.ToString()));
            settings.Add(new NameValue("IsFormDataChecked", isFormDataChecked.ToString()));
            
            return Execute(source: source, method: method, headers: headers, relativeUrl: relativeUrl,
                data: data, throwError: true, errors: out errors, formDataParameters: formDataParameters, settings: settings);
        }
        
        public static string Execute(IWebSource source, WebRequestMethod method, IEnumerable<string> headers, string relativeUrl,
            string data, bool throwError, out ErrorResultTO errors,
            IEnumerable<IFormDataParameters> formDataParameters = null, IWebRequestFactory webRequestFactory = null, IEnumerable<INameValue> settings = null, int timeout = 0)
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
                
                var isManualChecked = Convert.ToBoolean(settings?.FirstOrDefault(s => s.Name == "IsManualChecked")?.Value);
                var isFormDataChecked = Convert.ToBoolean(settings?.FirstOrDefault(s => s.Name == "IsFormDataChecked")?.Value);
                var isUrlEncodedChecked = Convert.ToBoolean(settings?.FirstOrDefault(s => s.Name == "IsUrlEncodedChecked")?.Value);
                
                if (isFormDataChecked || isUrlEncodedChecked)
                {
                    VerifyArgument.IsNotNullOrWhitespace("Content-Type", contentType);
                    var formDataBoundary = contentType.Split('=').Last();
                    var bytesData = isFormDataChecked ? GetMultipartFormData(formDataParameters, formDataBoundary) : GetFormUrlEncodedData(formDataParameters, formDataBoundary);
                    
                    return PerformMultipartWebRequest(webRequestFactory, client, address, bytesData, timeout);
                }

                if (isManualChecked && contentType != null && (contentType.ToLowerInvariant().Contains("multipart") || contentType.ToLowerInvariant().Contains("x-www")))
                {
                    var bytesData = ConvertToHttpNewLine(ref data);
                    return PerformMultipartWebRequest(webRequestFactory, client, address, bytesData, timeout);
                }

                if (method == WebRequestMethod.Post)
                {
                    var bytesData = Encoding.ASCII.GetBytes(data);
                    return PerformMultipartWebRequest(webRequestFactory, client, address, bytesData, timeout, headers);
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
                    using (var reader = new StreamReader(responseStream))
                    {
                        return reader.ReadToEnd();
                    }
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
            using (Stream formDataStream = new MemoryStream())
            {
                var needsClrf = false;

                var dds = postParameters.GetEnumerator();
                while (dds.MoveNext())
                {
                    var conditionExpression = dds.Current;

                    if (needsClrf)
                    {
                        formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));
                    }

                    needsClrf = true;

                    if (conditionExpression is FileParameter fileToUpload)
                    {
                        var fileKey = fileToUpload.Key;
                        var header = $"--{boundary}\r\nContent-Disposition: form-data; name=\"{fileKey}\"; filename=\"{fileToUpload.FileName ?? fileKey}\"\r\nContent-Type: {fileToUpload.ContentType ?? "application/octet-stream"}\r\n\r\n";

                        var fileBytes = fileToUpload.FileBytes;

                        formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                        formDataStream.Write(fileBytes, 0, fileBytes.Length);
                    }
                    else if (conditionExpression is TextParameter textToUpload)
                    {
                        var postData = $"--{boundary}\r\nContent-Disposition: form-data; name=\"{textToUpload.Key}\"\r\n\r\n{textToUpload.Value}";
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
        }
        
        private static byte[] GetFormUrlEncodedData(IEnumerable<IFormDataParameters> postParameters, string boundary)
        {
            var encoding = Encoding.UTF8;
            using (Stream formDataStream = new MemoryStream())
            {
                var dds = postParameters.GetEnumerator();
                while (dds.MoveNext())
                {
                    var conditionExpression = dds.Current;
                    var formValueType = conditionExpression;

                    var textToUpload = formValueType as TextParameter;
                    var postData = $"{textToUpload.Key}={textToUpload.Value}&";
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }

                formDataStream.Position = 0;
                var formData = new byte[formDataStream.Length];
                formDataStream.Read(formData, 0, formData.Length);
                formDataStream.Close();

                return formData;
            }
        }

        private static void ValidateSource(IWebSource source)
        {
            if (string.IsNullOrEmpty(source?.Address))
            {
                throw new Exception(Constants.ErrorMessages.WebAddressError);
            }
        }

        private static string GetAddress(IWebSource source, string relativeUri)
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

        public static string PerformMultipartWebRequest(IWebRequestFactory webRequestFactory, IWebClientWrapper client, string address, byte[] bytesData, int timeout = 0, IEnumerable<string> headers = null)
        {
            var wr = webRequestFactory.New(address);
            wr.Headers[HttpRequestHeader.Authorization] = client.Headers[HttpRequestHeader.Authorization];
            wr.ContentType = client.Headers[HttpRequestHeader.ContentType];
            wr.Method = "POST";
            wr.ContentLength = bytesData.Length;
            AddHeaders(wr, headers);
            if (timeout > 0)
            {
                wr.Timeout = timeout * 1000;
            }

            using (var requestStream = wr.GetRequestStream())
            {
                requestStream.Write(bytesData, 0, bytesData.Length);
                requestStream.Close();
            }

            using (var wresp = wr.GetResponse() as HttpWebResponse)
            {
                if (wresp != null && IsSuccessCode(wresp.StatusCode))
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

                var wrespStatusCode = wresp?.StatusCode ?? HttpStatusCode.Ambiguous;
                throw new ApplicationException("Error while upload files. Server status code: " + wrespStatusCode);
            }
        }

        private static bool IsSuccessCode(HttpStatusCode code)
        {
            return (int)code >= 200 && (int)code < 300;
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
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;

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
        
        private static void AddHeaders(IWebRequest wr, IEnumerable<string> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (header != ":" && !header.ToLower().StartsWith("content-type:"))
                    {
                        wr.AddHeader(header.Trim());
                    }
                }
            }
        }
    }
}
