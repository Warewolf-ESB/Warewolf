using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;
using System.Net;
using System.IO;
using System.ComponentModel.Composition;
using Dev2.Studio.Core.Factories;

namespace Dev2.Studio.Core {
    [Export(typeof(IWebCommunication))]
    public class WebCommunication : IWebCommunication {

        public IWebCommunicationResponse Get(string uri) {
            HttpWebRequest request = WebRequest.Create(string.Format("{0}", uri)) as HttpWebRequest;
            if (request != null) {
                request.Method = "GET";

                using (var response = request.GetResponse() as HttpWebResponse) {
                    long contentLength = response.ContentLength;
                    string contentType = response.ContentType;

                    Stream responseStream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(responseStream);
                    string content = streamReader.ReadToEnd();

                    return WebCommunicationResponseFactory.CreateWebCommunicationResponse(contentType, contentLength,
                                                                                          content);
                }
            }
            return default(IWebCommunicationResponse);
        }

        public IWebCommunicationResponse Post(string uri, string data) {
            HttpWebRequest request = WebRequest.Create(string.Format("{0}", uri)) as HttpWebRequest;
            if (request != null) {
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] byteData = Encoding.UTF8.GetBytes(data);
                request.ContentLength = byteData.Length;
                using (Stream postStream = request.GetRequestStream()) {
                    postStream.Write(byteData, 0, byteData.Length);
                }
                using (var response = request.GetResponse() as HttpWebResponse) {
                    long contentLength = response.ContentLength;
                    string contentType = response.ContentType;

                    Stream responseStream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(responseStream);
                    string content = streamReader.ReadToEnd();

                    return WebCommunicationResponseFactory.CreateWebCommunicationResponse(contentType, contentLength,
                                                                                          content);
                }
            }
            return default(IWebCommunicationResponse);
        }
    }
}
