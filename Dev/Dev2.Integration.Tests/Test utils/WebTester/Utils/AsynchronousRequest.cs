using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Dev2.Integration.Tests.Interfaces;

namespace Dev2.Integration.Tests.MEF.WebTester
{
    public class AsynchronousRequest : ILogger
    {
        private Stopwatch stopWatch;
        private RequestState state;
        private WebRequest request;
        public long totaltime = 0;
        private string responseData;
        HttpWebResponse _response;

        public void ScanSite(string url)
        {
            request = HttpWebRequest.Create(url);
            MethodGet(url);
        }
        
        public void ScanResponse(string url)
        {
            request = HttpWebRequest.Create(url);
            MethodGetResponse();
        }

        public void ScanSite(string url, string postData)
        {
            request = HttpWebRequest.Create(url);
            state = new RequestState(request);

            MethodPost(url, postData);

        }

        public void MethodPost(string url, string postData)
        {
            request.Method = "POST";
            state._requestData.Append(postData);
            request.ContentLength = (Encoding.ASCII.GetBytes(state._requestData.ToString())).Length;

            byte[] bytedata = UTF8Encoding.UTF8.GetBytes(state._requestData.ToString());
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(bytedata, 0, bytedata.Length);
                postStream.Close();
            }

            using (var responseStream = request.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(responseStream.GetResponseStream()))
                {
                    responseData = reader.ReadToEnd();
                }
            }
        }

        private void MethodGet(string url)
        {
            request.Method = "GET";
            request.Timeout = 300000; // wait up to five minutes ;) 

            using (var response = request.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                            responseData = reader.ReadToEnd();
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

        #region LoadTesterScaffolding

        public void ScanSiteStopWatch(string url, string postData)
        {
            request = HttpWebRequest.Create(url);
            stopWatch = new Stopwatch();
            stopWatch.Start();

            state = new RequestState(request);

            if (postData == null)
            {
                MethodGet(url);
            }
            else
            {
                MethodPost(url, postData);
            }
            stopWatch.Stop();
            long elapseTime = stopWatch.ElapsedMilliseconds;
            Console.WriteLine("----------------------- ELAPSED TIME BETWEEN CALLS ---------------------\n{0}: {1} {2} ms on threadId " + Thread.CurrentThread.ManagedThreadId + "\n------------------------------------------------------------------------", "Get Execution on url ", url.ToString(), elapseTime.ToString());
            stopWatch.Reset();
        }

        #endregion 

        #region AsyncCallBack

        //private void UpdateItem(IAsyncResult result)
        //{
        //    state = (RequestState)result.AsyncState;
        //    request = (WebRequest)state._request;
        //    WebResponse response = null;
        //    try
        //    {
        //        response = (WebResponse)request.EndGetResponse(result);
        //    }
        //    catch (WebException httpex)
        //    {
        //        Console.WriteLine("Exception: {0}", httpex);
        //        return;
        //    }
        //    stopWatch.Stop();
        //    Stream ResponseStream = response.GetResponseStream();

        //    state._responseStream = ResponseStream;
        //    IAsyncResult iarRead = ResponseStream.BeginRead(state._bufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), state);
        //    allDone.WaitOne();
        //}

        //private void ReadCallBack(IAsyncResult result)
        //{
        //    state = (RequestState)result.AsyncState;
        //    Stream responseStream = state._responseStream;
        //    int read = responseStream.EndRead(result);

        //    if (read > 0)
        //    {
        //        char[] charBuffer = new Char[BUFFER_SIZE];
        //        int len = state.StreamDecode.GetChars(state._bufferRead, 0, read, charBuffer, 0);
        //        String str = new String(charBuffer, 0, state._bufferRead.Length);
        //        state._requestData.Append(Encoding.ASCII.GetString(state._bufferRead, 0, len));
        //        IAsyncResult ar = responseStream.BeginRead(
        //            state._bufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), state);
        //        allDone.WaitOne();
        //    }
        //    else
        //    {
        //        if (state._requestData.Length > 0)
        //        {
        //            string strContent;
        //            strContent = state._requestData.ToString();
        //        }
        //        responseStream.Close();
        //        allDone.Set();
        //    }

        //    return;
        //}

        #endregion

        private void WriteData(RequestState state)
        {
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("Return Data: {0}", state._requestData.ToString());
            Console.WriteLine("--------------------------------------------------");
        }

        public void WriteToLog()
        {
            throw new NotImplementedException();
        }
    }
}
