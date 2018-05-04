/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;


namespace Dev2.Runtime.ServiceModel
{
    public delegate string WebExecuteString(WebSource source, WebRequestMethod method, string relativeUri, string data, bool throwError, out ErrorResultTO errors, string[] headers = null);
    public delegate string WebExecuteBinary(WebSource source, WebRequestMethod method, string relativeUri, byte[] data, bool throwError, out ErrorResultTO errors, string[] headers = null);

    // PBI 5656 - 2013.05.20 - TWR - Created
    public class WebSources : ExceptionManager
    {
        public WebSources()
            : this(ResourceCatalog.Instance)
        {
        }

        public WebSources(IResourceCatalog resourceCatalog)
        {
            if (resourceCatalog == null)
            {
                throw new ArgumentNullException(nameof(resourceCatalog));
            }
        }
        // POST: Service/WebSources/Get

        public WebSource Get(string resourceId, Guid workspaceId, Guid dataListId)
        {
            var result = new WebSource();
            try
            {
                var xmlStr = ResourceCatalog.Instance.GetResourceContents(workspaceId, Guid.Parse(resourceId)).ToString();
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

        // POST: Service/WebSources/Test

        public ValidationResult Test(string args, Guid workspaceId, Guid dataListId)
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

        public ValidationResult Test(WebSource source)
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

        ValidationResult CanConnectServer(WebSource source)
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

        public static string Execute(WebSource source, WebRequestMethod method, string relativeUri, string data, bool throwError, out ErrorResultTO errors) => Execute(source, method, relativeUri, data, throwError, out errors, null);
        public static string Execute(WebSource source, WebRequestMethod method, string relativeUri, string data, bool throwError, out ErrorResultTO errors, string[] headers)
        {
            CreateWebClient(source, headers);
            return Execute(source.Client, GetAddress(source, relativeUri), method, data, throwError, out errors);
        }

        static string GetAddress(WebSource source, string relativeUri)
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

        public static byte[] Execute(WebSource source, WebRequestMethod method, string relativeUri, byte[] data, bool throwError, out ErrorResultTO errors) => Execute(source, method, relativeUri, data, throwError, out errors, null);
        public static byte[] Execute(WebSource source, WebRequestMethod method, string relativeUri, byte[] data, bool throwError, out ErrorResultTO errors, string[] headers)
        {
            CreateWebClient(source, headers);
            return Execute(source.Client, GetAddress(source, relativeUri), method, data, out errors);
        }

        static byte[] Execute(WebClient client, string address, WebRequestMethod method, byte[] data, out ErrorResultTO errors)

        {
            errors = new ErrorResultTO();
            return method == WebRequestMethod.Get ? client.DownloadData(address) : client.UploadData(address, method.ToString().ToUpperInvariant(), data);
        }

        static string Execute(WebClient client, string address, WebRequestMethod method, string data, bool throwError, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            try
            {
                return method == WebRequestMethod.Get ? client.DownloadString(address) : client.UploadString(address, method.ToString().ToUpperInvariant(), data);
            }            
            catch(WebException webex)
            {
                if(webex.Response is HttpWebResponse httpResponse)
                {
                    using (var responseStream = httpResponse.GetResponseStream())
                    {
                        var reader = new StreamReader(responseStream);
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
                // clean up client ;)
                client.Dispose();
            }

            return string.Empty;
        }


        public static void CreateWebClient(WebSource source, IEnumerable<string> headers)
        {
            if (source != null && source.Client != null)
            {
                return;
            }

            if (source != null)
            {
                source.Client = new WebClient();

                if (source.AuthenticationType == AuthenticationType.User)
                {
                    source.Client.Credentials = new NetworkCredential(source.UserName, source.Password);
                }
                source.Client.Headers.Add("user-agent", GlobalConstants.UserAgentString);

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (header != ":")
                        {
                            source.Client.Headers.Add(header.Trim());
                        }
                    }
                }
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            }
        }
    }
}
