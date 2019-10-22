/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Common.Interfaces.Core
{
    public class RedisSourceDefinition : IRedisServiceSource, IEquatable<RedisSourceDefinition>
    {
        [ExcludeFromCodeCoverage]
        public RedisSourceDefinition()
        {

        }

        public RedisSourceDefinition(IRedisSource redisSource)
        {
            AuthenticationType = redisSource.AuthenticationType;
            Id = redisSource.ResourceID;
            Name = redisSource.ResourceName;
            Password = redisSource.Password;
            HostName = redisSource.HostName;
            Path = redisSource.GetSavePath();
        }

        public string HostName { get; set; }
        public string Password { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string Port { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }

        public bool Equals(IRedisServiceSource other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var equals = true;
            equals &= string.Equals(HostName, other.HostName);
            equals &= Id == other.Id;
            equals &= string.Equals(Name, other.Name);
            equals &= string.Equals(Password, other.Password);
            equals &= AuthenticationType == other.AuthenticationType;

            return equals;
        }

        public bool Equals(RedisSourceDefinition other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var equals = true;
            equals &= string.Equals(HostName, other.HostName);
            equals &= Id == other.Id;
            equals &= string.Equals(Name, other.Name);
            equals &= string.Equals(Password, other.Password);
            equals &= AuthenticationType == other.AuthenticationType;

            return equals;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((RedisSourceDefinition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = HostName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Password?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)AuthenticationType;
                return hashCode;
            }
        }

        public static bool operator ==(RedisSourceDefinition left, RedisSourceDefinition right) => Equals(left, right);

        public static bool operator !=(RedisSourceDefinition left, RedisSourceDefinition right) => !Equals(left, right);
    }
}
