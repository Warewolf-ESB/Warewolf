/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;
using Dev2.Common.ExtMethods;
using System.Linq;
using Warewolf.Interfaces.Data;

namespace Warewolf.Common
{
    public class WarewolfWebRequestForwarder : IConsumer
    {
        readonly string _url;
        readonly ICollection<IServiceInput> _valueKeys;
        private readonly MessageToInputsMapper _messageToInputsMapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPublisher _publisher;

        private WarewolfWebRequestForwarder()
        {
        }

        public WarewolfWebRequestForwarder(IHttpClientFactory httpClientFactory, IPublisher publisher, string url, ICollection<IServiceInput> valueKeys)
        {
            _httpClientFactory = httpClientFactory;
            _publisher = publisher;
            _url = url;
            _valueKeys = valueKeys;
            _messageToInputsMapper = new MessageToInputsMapper();
        }

        public async Task<ConsumerResult> Consume(byte[] body)
        {
            var postBody = BuildPostBody(body); 

            using (var execution = await SendEventToWarewolf(_url, postBody))
            {
                if (!execution.IsSuccessStatusCode)
                {
                    _publisher.Publish(Encoding.UTF8.GetBytes(postBody));
                    return ConsumerResult.Failed;
                }
            }
            return ConsumerResult.Success;
        }

        private string BuildPostBody(byte[] body)
        {
            var returnedQueueMessage = Encoding.UTF8.GetString(body);
            var inputs = _valueKeys.Select(v => (v.Name, v.Value)).ToList();
            var mappedData = _messageToInputsMapper.Map(returnedQueueMessage, inputs, returnedQueueMessage.IsJSON(), returnedQueueMessage.IsXml(),false);
            return mappedData;
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
