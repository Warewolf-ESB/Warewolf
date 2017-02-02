/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using System.Net;
using System.Text;
using Dev2.Integration.Tests.Interfaces;

namespace Dev2.Integration.Tests.MEF.WebTester
{
    public class AsynchronousRequest : ILogger
    {
        private RequestState state;
        private WebRequest request;
        public long totaltime = 0;
        private string responseData;
        HttpWebResponse _response;

        public bool WasHTTPS { get; set; }

        public void ScanSite(string url)
        {
            request = CreateRequest(url);
            MethodGet();
        }

        public void ScanResponse(string url)
        {
            request = CreateRequest(url);
            MethodGetResponse();
        }

        public void ScanSite(string url, string postData)
        {
            request = CreateRequest(url);
            state = new RequestState(request);

            MethodPost(url, postData);
        }

        static WebRequest CreateRequest(string url)
        {
            var result = WebRequest.Create(url);
            result.Timeout = 1000 * 3600; // it can wait up to 1 hour ;)
            result.Credentials = CredentialCache.DefaultCredentials;
            return result;
        }

        public void MethodPost(string url, string postData)
        {
            request.Method = "POST";
            state._requestData.Append(postData);
            request.ContentLength = Encoding.ASCII.GetBytes(state._requestData.ToString()).Length;

            byte[] bytedata = Encoding.UTF8.GetBytes(state._requestData.ToString());
            using(Stream postStream = request.GetRequestStream())
            {
                postStream.Write(bytedata, 0, bytedata.Length);
                postStream.Close();
            }

            using(var responseStream = request.GetResponse() as HttpWebResponse)
            {
                if(responseStream != null)
                {
                    using(StreamReader reader = new StreamReader(responseStream.GetResponseStream()))
                    {
                        responseData = reader.ReadToEnd();
                    }
                }
            }
        }

        private void MethodGet()
        {
            request.Method = "GET";
            request.Timeout = 300000; // wait up to five minutes ;) 

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
                {
                    WasHTTPS = true;
                    return true;
                };

            using(var response = request.GetResponse() as HttpWebResponse)
            {
                if(response != null)
                {
                    using(StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseData = reader.ReadToEnd();
                    }
                }
            }

        }

        private void MethodGetResponse()
        {
            request.Method = "GET";
            _response = request.GetResponse() as HttpWebResponse;


        }

        public string GetResponseData()
        {
            return responseData;
        }

        public HttpWebResponse GetResponse()
        {
            return _response;
        }
        
    }
}
