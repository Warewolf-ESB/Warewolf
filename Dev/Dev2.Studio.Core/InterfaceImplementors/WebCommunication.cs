using System.IO;
using System.Net;
using System.Text;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core
{
    public class WebCommunication : IWebCommunication
    {

        public IWebCommunicationResponse Get(string uri)
        {
            HttpWebRequest request = WebRequest.Create(string.Format("{0}", uri)) as HttpWebRequest;
            if(request != null)
            {
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Method = "GET";

                using(var response = request.GetResponse() as HttpWebResponse)
                {
                    if(response != null)
                    {
                        long contentLength = response.ContentLength;
                        string contentType = response.ContentType;

                        using(Stream responseStream = response.GetResponseStream())
                        {
                            if(responseStream != null)
                            {
                                using(StreamReader streamReader = new StreamReader(responseStream))
                                {
                                    string content = streamReader.ReadToEnd();

                                    return WebCommunicationResponseFactory.CreateWebCommunicationResponse(contentType, contentLength, content);
                                }
                            }
                        }
                    }
                }
            }
            return default(IWebCommunicationResponse);
        }

        public IWebCommunicationResponse Post(string uri, string data)
        {
            HttpWebRequest request = WebRequest.Create(string.Format("{0}", uri)) as HttpWebRequest;
            if(request != null)
            {
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] byteData = Encoding.UTF8.GetBytes(data);
                request.ContentLength = byteData.Length;
                using(Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }
                using(var response = request.GetResponse() as HttpWebResponse)
                {
                    if(response != null)
                    {
                        long contentLength = response.ContentLength;
                        string contentType = response.ContentType;

                        using(Stream responseStream = response.GetResponseStream())
                        {
                            if(responseStream != null)
                            {
                                using(StreamReader streamReader = new StreamReader(responseStream))
                                {
                                    string content = streamReader.ReadToEnd();

                                    return WebCommunicationResponseFactory.CreateWebCommunicationResponse(contentType, contentLength, content);
                                }
                            }
                        }
                    }
                }
            }
            return default(IWebCommunicationResponse);
        }
    }
}
