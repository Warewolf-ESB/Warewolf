/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using ServiceStack.Redis;
using Warewolf.Interfaces;

namespace Warewolf.Driver.Redis
{
    public class RedisConnection : IRedisConnection
    {
        public RedisConnection(string hostName, int port, string password)
        {
            var client = !string.IsNullOrWhiteSpace(password) ? new RedisClient(hostName, port, password) : new RedisClient(hostName, port);
            Cache = new RedisCache(client);
        }

        public IRedisCache Cache { get; private set; }
    }

    internal class RedisCache : IRedisCache
    {
        private readonly IRedisClient _client;
        public RedisCache(IRedisClient client)
        {
            _client = client;
        }

        public string Get(string key)
        {
            using (var client = _client)
            {
                return client.Get<string>(key);
            }
        }

        public bool Set(string key, string value, TimeSpan timeSpan)
        {
            using (var client = _client)
            {
                if (timeSpan.TotalSeconds == 0)
                {
                    return client.Set(key, value);
                }

                return client.Set(key, value, timeSpan);
            }
        }

        public bool Remove(string key)
        {
            using (var client = _client)
            {
                return client.Remove(key);
            }
        }

        public long Increment(string key, string value)
        {
            using (var client = _client)
            {
                return client.Increment(key, uint.Parse(value));
            }
        }

        public long Decrement(string key, string value)
        {
            using (var client = _client)
            {
                return client.Decrement(key, uint.Parse(value));
            }
        }
    }
}