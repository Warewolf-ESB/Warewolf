/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Warewolf.Interfaces;

namespace Warewolf.Driver.Redis
{
    public abstract class RedisCacheBase
    {
        readonly Lazy<IRedisConnection> _connection;

        protected RedisCacheBase(Func<IRedisConnection> createConnection)
        {
            if (createConnection is null) throw new ArgumentNullException(nameof(createConnection));
            _connection = new Lazy<IRedisConnection>(() => createConnection?.Invoke());
        }

        public RedisCacheBase()
        {
        }

        private IRedisCache Cache => _connection.Value.Cache;

        public bool Set(string key, string value, TimeSpan timeSpan)
        {
            if (string.IsNullOrWhiteSpace(key)) { throw new ArgumentNullException(nameof(key)); }
            if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentNullException($"Data input {nameof(value)} can not be null."); }

            return Cache.Set(key, value, timeSpan);
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            return Cache.Get(key);
        }
        public bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            return Cache.Remove(key);
        }
        public long Increment(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            return Cache.Increment(key,value);
        }
        public long Decrement(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            return Cache.Decrement(key, value); ;
        }
    }

    public class RedisCacheImpl : RedisCacheBase
    {
        // TODO: Suggestion to implement IRedisConnection redisConnection so that we can Mock the parameters.
        //       We can use the real connection for Integration testing
        public RedisCacheImpl() { }
        public RedisCacheImpl(string hostName, int port, string password) : this(() => new RedisConnection(hostName, port, password)) { }
        public RedisCacheImpl(Func<IRedisConnection> createConnection) : base(createConnection)
        {
        }
    }
}
