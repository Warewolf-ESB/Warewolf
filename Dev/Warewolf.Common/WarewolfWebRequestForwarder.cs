/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using System;
using System.Net.Http;
using System.Text;
using System.Web;

namespace Warewolf.Common
{
    public class WarewolfWebRequestForwarder : WebRequestForwarderBase,  IConsumer
    {
        readonly string _url;
        readonly string _valueKey;

        private WarewolfWebRequestForwarder(IHttpClient httpClient) : base(httpClient)
        {
        }

        public WarewolfWebRequestForwarder(IHttpClient httpClient, string url, string valueKey)
            : this(httpClient)
        {
            _url = url;
            _valueKey = valueKey;
        }

        public void Consume(byte[] body)
        {
            var builder = BuildUri(_url, body);

            SendEventToWarewolf(builder.ToString());
        }

        private UriBuilder BuildUri(string url, byte[] body)
        {
            var queryStr = BuildQueryString(Encoding.UTF8.GetString(body));

            var builder = new UriBuilder(url)
            {
                Query = queryStr
            };
            return builder;
        }

        private HttpResponseMessage SendEventToWarewolf(string url)
        {
            return SendUrl(_url).Result;
        }

        private string BuildQueryString(string data)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query[_valueKey] = data;
            return query.ToString();
        }
    }
}
