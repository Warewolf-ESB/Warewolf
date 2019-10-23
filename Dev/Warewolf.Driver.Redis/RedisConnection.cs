/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using StackExchange.Redis;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Interfaces;

namespace Warewolf.Driver.Redis
{
    internal class RedisConnection : IRedisConnection
    {
        public RedisConnection(string hostName)
        {
            var connection = ConnectionMultiplexer.Connect(hostName);
            Cache = new RedisCache(connection.GetDatabase());
        }
        public IRedisCache Cache { get; private set; }
    }

    internal class RedisCache : IRedisCache
    {
        private IDatabase _database;

        public RedisCache(IDatabase database)
        {
            _database = database;
        }

        public bool HashSet(string key,IDictionary<string,string> dictionary)
        {
            var entries = dictionary.Select(item => new HashEntry(item.Key, item.Value)).ToArray();
            _database.HashSet(key, entries);
            return true;
        }

        public string StringGet(string key) => _database.StringGet(key);
        public bool StringSet(string key, string value) => _database.StringSet(key,value);
    }
}