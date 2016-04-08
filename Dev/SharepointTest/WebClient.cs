using System;
using System.Globalization;
using System.IO;
using System.Net;
using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Tools.Network;
// ReSharper disable RedundantAssignment

namespace Warewolf.SharePoint
{
    public class WarewolfSharepointUtils
    {
        public RequestResult AddListItem(string _uri, string listTitle, string json)
        {
            Uri webUri = new Uri(_uri);
            var result = new RequestResult { RequestSuccess = true };
            using (var client = new SharepointClient(webUri, CredentialCache.DefaultNetworkCredentials))
            {
                try
                {
                    client.InsertListItem(listTitle, json);
                }
                catch (Exception ex)
                {
                    result.RequestSuccess = false;
                    result.ErrorString = ex.Message;
                    return result;
                }
            }

            return result;
        }

        public RequestResult GetAllListItemsByListName(string _uri, string listTitle)
        {
            Uri uri = new Uri(_uri);
            var result = new RequestResult { RequestSuccess = true };
            using (var client = new SharepointClient(uri, CredentialCache.DefaultNetworkCredentials))
            {
                try
                {
                    var retval = client.GetListItem(listTitle, 6);

                    //convert to XML first..
                    Serializer serializer = new Serializer();
                    Serialized serialisedData = new Serialized();

                    serialisedData = serializer.JSONtoXML(retval);

                    result.RequestResponse = serialisedData.Data;

                }
                catch (Exception ex)
                {
                    result.RequestSuccess = false;
                    result.ErrorString = ex.Message;
                }
            }
            return result;
        }

        public RequestResult GetListItemswithQuery(string _uri, string listTitle, string qry)
        {
            Uri uri = new Uri(_uri);
            var result = new RequestResult { RequestSuccess = true };
            using (var client = new SharepointClient(uri, CredentialCache.DefaultNetworkCredentials))
            {
                try
                {
                    var retval = client.GetListItemswithQuery(listTitle, qry);

                    //convert to XML first..
                    Serializer serializer = new Serializer();
                    Serialized serialisedData = new Serialized();
                    serialisedData = serializer.JSONtoXML(retval);
                    
                    result.RequestResponse = serialisedData.Data;

                }
                catch (Exception ex)
                {
                    result.RequestSuccess = false;
                    result.ErrorString = ex.Message;
                }
            }
            return result;
        }

        public RequestResult GetListItemById(string _uri, int id, string listTitle)
        {
            Uri uri = new Uri(_uri);
            var result = new RequestResult { RequestSuccess = true };
            using (var client = new SharepointClient(uri, CredentialCache.DefaultNetworkCredentials))
            {
                try
                {
                    var retval = client.GetListItem(listTitle, id);

                    //convert to XML first..
                    Serializer serializer = new Serializer();
                    Serialized serialisedData = new Serialized();
                    serialisedData = serializer.JSONtoXML(retval);
                    result.RequestResponse = serialisedData.Data;
                }
                catch (Exception ex)
                {
                    result.RequestSuccess = false;
                    result.ErrorString = ex.Message;
                }
            }
            return result;
        }

        public RequestResult UpdateListItemById(string _uri, int id, string listTitle, string updateJson)
        {
            Uri uri = new Uri(_uri);
            var result = new RequestResult { RequestSuccess = true };
            var jsonData = GetListItemById(_uri, id, listTitle).RequestResponse;
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData);
            dynamic metadata = data.d.results[0].__metadata;
            string uriString = metadata.uri;
            string match = metadata.etag;
            
            using (var client = new SharepointClient(uri, CredentialCache.DefaultNetworkCredentials))
            {
                try
                {
                    client.UpdateListItem(uriString, updateJson, match);
                }
                catch (Exception ex)
                {
                    result.ErrorString = ex.Message;
                    result.RequestSuccess = false;
                    return result;
                }
            }
            return result;
        }


        public RequestResult UpdateListItemByIdWithImpersonation(string userName, string domainName, string password, string _uri, int id, string listTitle, string updateJson)
        {
            Uri uri = new Uri(_uri);
            var result = new RequestResult { RequestSuccess = true };
            var jsonData = GetListItemById(_uri, id, listTitle).RequestResponse;
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData);
            dynamic metadata = data.d.results[0].__metadata;
            string uriString = metadata.uri;
            string match = metadata.etag;

            // Impersonate a user
            var imp = new Impersonator();
            imp.Impersonate(userName, domainName, password);

            using (var client = new SharepointClient(uri, CredentialCache.DefaultNetworkCredentials))
            {
                try
                {
                    client.UpdateListItem(uriString, updateJson, match);
                }
                catch (Exception ex)
                {
                    result.ErrorString = ex.Message;
                    result.RequestSuccess = false;
                    return result;
                }
            }
            return result;
        }


        public RequestResult DeleteListItemById(string _uri, int id, string listTitle)
        {
            Uri uri = new Uri(_uri);
            var result = new RequestResult { RequestSuccess = true };
            var jsonData = GetListItemById(_uri, id, listTitle).RequestResponse;
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData);
            dynamic metadata = data.d.results[0].__metadata;
            string uriString = metadata.uri;
            string match = metadata.etag;
            using (var client = new SharepointClient(uri, CredentialCache.DefaultNetworkCredentials))
            {
                try
                {
                    client.DeleteListItem(uriString, match);
                }
                catch (Exception ex)
                {
                    result.RequestSuccess = false;
                    result.ErrorString = ex.Message;
                    return result;
                }
            }
            
            return result;
        }

        public RequestResult SendEmail(string _uri, string from, string to, string subject, string body)
        {
            Uri uri = new Uri(_uri);
            var result = new RequestResult { RequestSuccess = true };
            dynamic emailObj = new
            {
                properties = new
                {
                    __metadata = new
                    {
                        type = "SP.Utilities.EmailProperties"
                    },
                    From = from,
                    To = new
                    {
                        results = to.Split(',')
                    },
                    Body = body,
                    Subject = subject
                }
            };
            using (var client = new SharepointClient(uri, CredentialCache.DefaultNetworkCredentials))
            {
                try
                {
                    client.SendEmail(JsonConvert.SerializeObject(emailObj));
                }
                catch (WebException ex)
                {
                    var errorMessage = ex.Message;
                    if (ex.Response != null)
                    {
                        var responseStream = ex.Response.GetResponseStream();
                        if (responseStream != null)
                        {
                            var stream = new StreamReader(responseStream);
                            var readToEnd = stream.ReadToEnd();
                            if (!string.IsNullOrEmpty(readToEnd))
                            {
                                errorMessage += " :" + readToEnd;
                            }
                        }
                    }
                    result.RequestSuccess = false;

                    result.ErrorString = errorMessage;
                    return result;
                }
            }
            return result;
        }

        public OutOfOfficeResult GetOutOfOffice(String urlname, string emailAddress)
        {
            var result = new OutOfOfficeResult { IsOutOfOffice = false };
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2010);
            service.UseDefaultCredentials = true;
            service.PreAuthenticate = true;
            service.Url = new Uri(urlname);
            service.TraceEnabled = true;
            var userOofSettings = service.GetUserOofSettings(emailAddress);
            if (userOofSettings != null)
            {
                if (userOofSettings.State == OofState.Enabled)
                {
                    result.IsOutOfOffice = true;
                }
            }
            return result;
        }
    }

    public class OutOfOfficeResult
    {
        public bool IsOutOfOffice { get; set; }
    }

    internal class SharepointClient : IDisposable
    {
        public SharepointClient(Uri webUri, ICredentials credentials)
        {
            _client = new WebClient { Credentials = credentials };
            _client.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
            _client.Headers.Add(HttpRequestHeader.ContentType, "application/json;odata=verbose");
            _client.Headers.Add(HttpRequestHeader.Accept, "application/json;odata=verbose");
            WebUri = webUri;
        }

        /// <summary>
        /// Request Form Digest
        /// </summary>
        /// <returns></returns>
        private string RequestFormDigest(string requestType)
        {
            var endpointUri = new Uri(WebUri, "_api/contextinfo");
            var result = _client.UploadString(endpointUri, requestType);
            JToken t = JToken.Parse(result);
            return t["d"]["GetContextWebInformation"]["FormDigestValue"].ToString();
        }

        Uri WebUri { get; set; }

        public void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }

        private readonly WebClient _client;

        /// <summary>
        /// Returns a specific list item
        /// </summary>
        /// <param name="listTitle"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetListItem(string listTitle, int id)
        {
            _client.Headers.Add("X-HTTP-Method", "GET");
            _client.Headers.Add("Content-Type", "application/json;odata=verbose");

            var endpointUri = new Uri(WebUri, string.Format("/_api/web/lists/getbytitle('{0}')/items?$filter=Id eq {1}", listTitle, id));
            return _client.DownloadString(endpointUri);
        }

        /// <summary>
        /// Gets all of the list items. Returns XML for parsiing
        /// </summary>
        /// <param name="listTitle"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public string GetListItemswithQuery(string listTitle, string query)
        {
            _client.Headers.Add("X-HTTP-Method", "GET");
            _client.Headers.Add("Content-Type", "application/json;odata=verbose");

            var endpointUri = new Uri(WebUri, string.Format("/_api/web/lists/getbytitle('{0}')/items{1}", listTitle, query));
            return _client.DownloadString(endpointUri);
        }

        /// <summary>
        /// Inserts a new list item
        /// </summary>
        /// <param name="listTitle"></param>
        /// <param name="json"></param>
        public void InsertListItem(string listTitle, string json)
        {
            const string RequestType = "POST";
            var formDigestValue = RequestFormDigest(RequestType);
            _client.Headers.Add("X-RequestDigest", formDigestValue);
            _client.Headers.Add("X-HTTP-Method", RequestType);
            _client.Headers.Add("Content-Type", "application/json;odata=verbose");
            _client.Headers.Add("ContentLength", json.Length.ToString(CultureInfo.InvariantCulture));

            var endpointUri = new Uri(WebUri, string.Format("/_api/web/lists/getbytitle('{0}')/items", listTitle));
            _client.UploadString(endpointUri, RequestType, json);
        }


        /// <summary>
        /// Updates a list item
        /// </summary>
        /// <param name="uriString"></param>
        /// <param name="json"></param>
        /// <param name="match"></param>
        public void UpdateListItem(string uriString, string json, string match)
        {
            const string RequestType = "MERGE";
            var formDigestValue = RequestFormDigest(RequestType);
            _client.Headers.Add("X-RequestDigest", formDigestValue);
            _client.Headers.Add("X-HTTP-Method", RequestType);
            _client.Headers.Add("Content-Type", "application/json;odata=verbose");
            _client.Headers.Add("ContentLength", json.Length.ToString(CultureInfo.InvariantCulture));
            _client.Headers.Add("If-Match", match);

            var endpointUri = new Uri(uriString);
            _client.UploadString(endpointUri, RequestType, json);
        }

        /// <summary>
        /// Deletes a list item
        /// </summary>
        /// <param name="uriString"></param>
        /// <param name="match"></param>
        public void DeleteListItem(string uriString, string match)
        {
            const string RequestType = "DELETE";
            var formDigestValue = RequestFormDigest(RequestType);
            _client.Headers.Add("X-RequestDigest", formDigestValue);
            _client.Headers.Add("X-HTTP-Method", RequestType);
            _client.Headers.Add("Content-Type", "application/json;odata=verbose");
            _client.Headers.Add("If-Match", match);

            var endpointUri = new Uri(uriString);
            _client.UploadString(endpointUri, RequestType, "");
        }

        public void SendEmail(string jsonEmailPayload)
        {
            var formDigestValue = RequestFormDigest("POST");
            _client.Headers.Add("X-RequestDigest", formDigestValue);
            _client.Headers.Add("Content-Type", "application/json;odata=verbose");
            var endpointUri = new Uri(WebUri, string.Format("/_api/SP.Utilities.Utility.SendEmail"));
            _client.UploadString(endpointUri, jsonEmailPayload);
        }

    }

    public class RequestResult
    {
        public bool RequestSuccess { get; set; }
        public string ErrorString { get; set; }
        public string RequestResponse { get; set; }
    }
}
