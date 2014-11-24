
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Runtime.ServiceModel
{
    public delegate string WebExecuteString(WebSource source, WebRequestMethod method, string relativeUri, string data, bool throwError, out ErrorResultTO errors, string[] headers = null);
    public delegate string WebExecuteBinary(WebSource source, WebRequestMethod method, string relativeUri, byte[] data, bool throwError, out ErrorResultTO errors, string[] headers = null);

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
        public WebSource Get(string resourceId, Guid workspaceId, Guid dataListId)
        {
            var result = new WebSource();
            try
            {
                var xmlStr = ResourceCatalog.Instance.GetResourceContents(workspaceId, Guid.Parse(resourceId)).ToString();
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
        public string Save(string args, Guid workspaceId, Guid dataListId)
        {
            try
            {
                var source = JsonConvert.DeserializeObject<WebSource>(args);

                _resourceCatalog.SaveResource(workspaceId, source);
                if(workspaceId != GlobalConstants.ServerWorkspaceID)
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
        public ValidationResult Test(string args, Guid workspaceId, Guid dataListId)
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
                ErrorResultTO errors;
                return new ValidationResult
                {
                    Result = Execute(source, WebRequestMethod.Get, source.DefaultQuery, (string)null, true, out errors)
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
            finally
            {
                source.DisposeClient();
            }
        }

        #endregion

        #region Execute

        public static string Execute(WebSource source, WebRequestMethod method, string relativeUri, string data, bool throwError, out ErrorResultTO errors, string[] headers = null)
        {
            EnsureWebClient(source, headers);
            return Execute(source.Client, string.Format("{0}{1}", source.Address, relativeUri), method, data, throwError, out errors);
        }

        public static byte[] Execute(WebSource source, WebRequestMethod method, string relativeUri, byte[] data, bool throwError, out ErrorResultTO errors, string[] headers = null)
        {
            EnsureWebClient(source, headers);
            return Execute(source.Client, string.Format("{0}{1}", source.Address, relativeUri), method, data, throwError, out errors);
        }

        #endregion

        #region Execute(client, address, method, data)

        // ReSharper disable UnusedParameter.Local
        static byte[] Execute(WebClient client, string address, WebRequestMethod method, byte[] data, bool throwError, out ErrorResultTO errors)
        // ReSharper restore UnusedParameter.Local
        {
            EnsureContentType(client);
            errors = new ErrorResultTO();
            switch(method)
            {
                case WebRequestMethod.Get:
                    return client.DownloadData(address);

                default:
                    return client.UploadData(address, method.ToString().ToUpperInvariant(), data);
            }
        }

        static string Execute(WebClient client, string address, WebRequestMethod method, string data, bool throwError, out ErrorResultTO errors)
        {
            EnsureContentType(client);
            errors = new ErrorResultTO();
            try
            {
                switch(method)
                {
                    case WebRequestMethod.Get:
                        return FixResponse(client.DownloadString(address));

                    default:
                        return FixResponse(client.UploadString(address, method.ToString().ToUpperInvariant(), data));
                }
            }
            catch(Exception e)
            {
                errors.AddError(e.Message);
                if(throwError)
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

        #region FixResponse

        static string FixResponse(string result)
        {
            if(string.IsNullOrEmpty(result))
            {
                return result;
            }
            return WebUtility.HtmlDecode(result);
        }

        #endregion

        #region EnsureWebClient

        static void EnsureWebClient(WebSource source, IEnumerable<string> headers)
        {
            if(source.Client != null)
            {
                return;
            }

            source.Client = new WebClient();

            if(source.AuthenticationType == AuthenticationType.User)
            {
                source.Client.Credentials = new NetworkCredential(source.UserName, source.Password);
            }

            if(headers != null)
            {
                foreach(var header in headers)
                {
                    source.Client.Headers.Add(header.Trim());
                }
            }
        }

        #endregion
    }

}
