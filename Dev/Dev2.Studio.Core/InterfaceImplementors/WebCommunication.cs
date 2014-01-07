using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Text;

namespace Dev2.Studio.Core
{
    [Export(typeof(IWebCommunication))]
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

                        Stream responseStream = response.GetResponseStream();
                        StreamReader streamReader = new StreamReader(responseStream);
                        string content = streamReader.ReadToEnd();

                        return WebCommunicationResponseFactory.CreateWebCommunicationResponse(contentType, contentLength,
                            content);
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

                        Stream responseStream = response.GetResponseStream();
                        StreamReader streamReader = new StreamReader(responseStream);
                        string content = streamReader.ReadToEnd();

                        return WebCommunicationResponseFactory.CreateWebCommunicationResponse(contentType, contentLength,
                            content);
                    }
                }
            }
            return default(IWebCommunicationResponse);
        }
    }
}
