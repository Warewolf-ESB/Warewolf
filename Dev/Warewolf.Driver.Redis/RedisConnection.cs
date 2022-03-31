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
using StackExchange.Redis;
using Warewolf.Interfaces;

namespace Warewolf.Driver.Redis
{
    public class RedisConnection : IRedisConnection
    {
        public RedisConnection(string hostName, int port, string password)
        {
            var client = GetClient(hostName, port, password);
            Cache = new RedisCache(client);
        }

        private static IDatabase GetClient(string hostName, int port, string password)
        {
            var config = new ConfigurationOptions
            {
                ClientName = hostName,
                Password = password,
                AbortOnConnectFail = false,
                ConnectRetry = 3,
                SyncTimeout = 50000,
                EndPoints =
                {
                    { hostName, port }
                }
            };

            var redis = ConnectionMultiplexer.Connect(config);
            var db = redis.GetDatabase();
            return db;
        }

        public IRedisCache Cache { get; private set; }
    }

    internal class RedisCache : IRedisCache
    {
        private readonly IDatabase _database;

        public RedisCache(IDatabase database)
        {
            _database = database;
        }

        public string Get(string key)
        {
            return _database.StringGet(key);
        }

        public bool Set(string key, string value, TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds == 0)
            {
                return _database.StringSet(key, value);
            }

            return _database.StringSet(key, value, timeSpan);
        }

        public bool Remove(string key)
        {
            return _database.KeyDelete(key);
        }

        public long Increment(string key, string value)
        {
            return _database.StringIncrement(key, uint.Parse(value));
        }

        public long Decrement(string key, string value)
        {
            return _database.StringDecrement(key, uint.Parse(value));
        }
    }
}