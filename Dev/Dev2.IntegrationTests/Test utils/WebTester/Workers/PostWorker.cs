
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Integration.Tests.Interfaces;

namespace Dev2.Integration.Tests.MEF.WebTester
{
    public sealed class PostWorker : IWorker
    {
        private string _postData { get; set; }
        private string _url { get; set; }

        private string _responseData { get; set; }

        public PostWorker()
        {
            _postData = string.Empty;
            _url = "about:blank";
        }

        public PostWorker(string postdata)
        {
            string[] postwithurl = SplitPostFromUrl(postdata);

            _url = postwithurl[0];
            _postData = postwithurl[1];
        }


        public PostWorker(string url, string postData)
        {
            _postData = postData;
            _url = url;
        }

        public void DoWork()
        {
            AsynchronousRequest async = new AsynchronousRequest();
            async.ScanSite(_url, _postData);
            _responseData = async.GetResponseData();
        }

        private string[] SplitPostFromUrl(string PostWithUrl)
        {
            string[] alldata = PostWithUrl.Split('?');
            return alldata;
        }

        public string GetResponseData()
        {
            return _responseData;
        }
    }
}
