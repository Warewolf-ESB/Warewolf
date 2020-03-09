/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.ServerProxyLayer;
using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces.Core
{
    public class ElasticsearchSourceDefinition : IElasticsearchServiceSource, IEquatable<ElasticsearchSourceDefinition>
    {
        [ExcludeFromCodeCoverage]
        public ElasticsearchSourceDefinition()
        {

        }
        public ElasticsearchSourceDefinition(IElasticsearchSource source)
        {
            AuthenticationType = source.AuthenticationType;
            Id = source.ResourceID;
            Name = source.ResourceName;
            Password = source.Password;
            Username = source.Username;
            HostName = source.HostName;
            Port = source.Port;
            Path = source.GetSavePath();
        }
        public string Username { get; set; }
        public string HostName { get; set; }
        public string Password { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string Port { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }

     public bool Equals(IElasticsearchServiceSource other)
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
            equals &= string.Equals(Password, other.Password);
            equals &= string.Equals(Username, other.Username);
            equals &= Id == other.Id;
            equals &= string.Equals(Name, other.Name);
            equals &= AuthenticationType == other.AuthenticationType;
            return equals;
        }

        public bool Equals(ElasticsearchSourceDefinition other)
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
            equals &= string.Equals(Password, other.Password);
            equals &= string.Equals(Username, other.Username);
            equals &= Id == other.Id;
            equals &= string.Equals(Name, other.Name);
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
            return Equals((ElasticsearchSourceDefinition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = HostName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Password?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Username?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(ElasticsearchSourceDefinition left, ElasticsearchSourceDefinition right) => Equals(left, right);

        public static bool operator !=(ElasticsearchSourceDefinition left, ElasticsearchSourceDefinition right) => !Equals(left, right);
    }
}