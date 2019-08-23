/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Triggers;

namespace Warewolf.Common
{
    public class WarewolfWebRequestForwarder : IConsumer
    {
        readonly string _url;
        readonly string _valueKey;
        private readonly IHttpClientFactory _httpClientFactory;

        private WarewolfWebRequestForwarder()
        {
        }

        public WarewolfWebRequestForwarder(IHttpClientFactory httpClientFactory, string url, string valueKey)
        {
            _httpClientFactory = httpClientFactory;
            _url = url;
            _valueKey = valueKey;
        }

        public async void Consume(byte[] body)
        {
            var builder = BuildUri(_url, body); 

            using (await SendEventToWarewolf(builder.ToString()))
            {
                // empty block
            }
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

        private async Task<HttpResponseMessage> SendEventToWarewolf(string uri)
        {
            using (var client = _httpClientFactory.New(uri))
            {
                return await client.GetAsync(uri);
            }
        }

        private string BuildQueryString(string data)
        {
            var encodedData = WebUtility.UrlEncode(data);
            return $"{_valueKey}={encodedData}";
        }
    }
}
