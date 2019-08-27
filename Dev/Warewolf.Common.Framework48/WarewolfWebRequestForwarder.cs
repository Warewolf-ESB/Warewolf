/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.DB;
using System;
using System.Collections.Generic;
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
        readonly ICollection<IServiceInput> _valueKeys;
        private readonly IHttpClientFactory _httpClientFactory;

        private WarewolfWebRequestForwarder()
        {
        }

        public WarewolfWebRequestForwarder(IHttpClientFactory httpClientFactory, string url, ICollection<IServiceInput> valueKeys)
        {
            _httpClientFactory = httpClientFactory;
            _url = url;
            _valueKeys = valueKeys;
        }

        public async void Consume(byte[] body)
        {
            var postBody = BuildPostBody(body); 

            using (await SendEventToWarewolf(_url, postBody))
            {
                // empty block
            }
        }

        private string BuildPostBody(byte[] body)
        {
            var returnedQueueMessage = Encoding.UTF8.GetString(body);
            return returnedQueueMessage;
        }

        private async Task<HttpResponseMessage> SendEventToWarewolf(string uri,string postData)
        {
            using (var client = _httpClientFactory.New(uri))
            {
                return await client.PostAsync(uri,postData);
            }
        }
    }
}
