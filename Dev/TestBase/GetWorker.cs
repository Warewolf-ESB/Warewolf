/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Net;
using TestBase.Interfaces;

namespace TestBase
{
    public class GetWorker : IWorker
    {
        string _url { get; set; }
        static string dataToCompareTo { get; set; }

        public bool WasHTTPS { get; set; }

        public void DoWork()
        {
            var async = new AsynchronousRequest();
            async.ScanSite(_url);
            dataToCompareTo = async.GetResponseData();
            WasHTTPS = async.WasHTTPS;
        }

        public GetWorker(string url)
        {
            _url = url;
        }

        public string GetResponseData()
        {
            return dataToCompareTo;
        }

        public HttpWebResponse GetResponse()
        {
            var async = new AsynchronousRequest();
            async.ScanResponse(_url);
            return async.GetResponse();
        }
    }
}
