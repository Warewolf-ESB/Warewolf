﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using ServiceStack.Redis;
using Warewolf.Interfaces;

namespace Warewolf.Driver.Redis
{
    public class RedisConnection : IRedisConnection
    {
        private static readonly ConcurrentDictionary<(string, int, string), RedisClient> RedisConnectionPool = new ConcurrentDictionary<(string, int, string), RedisClient>();

        public RedisConnection(string hostName, int port, string password)
        {
            try
            {
                RedisClient client = RedisConnectionPool.GetOrAdd((hostName, port, password), !string.IsNullOrWhiteSpace(password) ? new RedisClient(hostName, port, password) : new RedisClient(hostName, port));

                if (client.ServerVersion != null)
                {
                    Cache = new RedisCache(client);
                }
            }
            catch
            {
                RedisConnectionPool.TryRemove((hostName, port, password), out RedisClient redisClient);
                throw;
            }
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

        public string Get(string key) => _client.Get<string>(key);

        public bool Set(string key, string value, TimeSpan timeSpan) => timeSpan.TotalSeconds == 0 ? _client.Set(key, value) : _client.Set(key, value, timeSpan);

        public bool Remove(string key)
        {
            return _client.Remove(key);
        }

        public long Increment(string key, string value)
        {
            return _client.Increment(key, uint.Parse(value));
        }

        public long Decrement(string key, string value)
        {
            return _client.Decrement(key, uint.Parse(value));
        }
    }
}