/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using TestBase.Interfaces;

namespace TestBase
{
    public sealed class PostWorker : IWorker
    {
        string _postData { get; set; }
        string _url { get; set; }

        string _responseData { get; set; }

        public PostWorker()
        {
            _postData = string.Empty;
            _url = "about:blank";
        }

        public PostWorker(string postdata)
        {
            var postwithurl = SplitPostFromUrl(postdata);

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
            var async = new AsynchronousRequest();
            async.ScanSite(_url, _postData);
            _responseData = async.GetResponseData();
        }

        string[] SplitPostFromUrl(string PostWithUrl)
        {
            var alldata = PostWithUrl.Split('?');
            return alldata;
        }

        public string GetResponseData()
        {
            return _responseData;
        }
    }
}
