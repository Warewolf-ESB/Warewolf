/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading.Tasks;
using Warewolf.Data;

namespace Warewolf.Streams
{
    internal class DeserializingConsumer<T> : IConsumer
    {
        private readonly IConsumer<T> _consumer;
        private readonly IDeserializer _deserializer;

        public DeserializingConsumer(IDeserializer deserializer, IConsumer<T> consumer)
        {
            _deserializer = deserializer;
            _consumer = consumer;
        }

        public Task<ConsumerResult> Consume(byte[] body, object parameters)
        {
            var item = _deserializer.Deserialize<T>(body);
            return _consumer.Consume(item, parameters);
        }
    }
}
