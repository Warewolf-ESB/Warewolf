/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces.Core
{
    public class WebServiceSourceDefinition : IWebServiceSource, IEquatable<WebServiceSourceDefinition>
    {
        [ExcludeFromCodeCoverage]
        public WebServiceSourceDefinition()
        {

        }

        public WebServiceSourceDefinition(IWebSource db)
        {
            AuthenticationType = db.AuthenticationType;
            DefaultQuery = db.DefaultQuery;
            Id = db.ResourceID;
            Name = db.ResourceName;
            Password = db.Password;
            HostName = db.Address;
            Path = db.GetSavePath();
            UserName = db.UserName;
        }

        public bool Equals(WebServiceSourceDefinition other)
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
            equals &= string.Equals(UserName, other.UserName);
            equals &= string.Equals(Password, other.Password);
            equals &= AuthenticationType == other.AuthenticationType;
            equals &= string.Equals(DefaultQuery, other.DefaultQuery);

            return equals;
        }

        public bool Equals(IWebServiceSource other)
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
            equals &= string.Equals(UserName, other.UserName);
            equals &= string.Equals(Password, other.Password);
            equals &= AuthenticationType == other.AuthenticationType;
            equals &= string.Equals(DefaultQuery, other.DefaultQuery);

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
            return Equals((WebServiceSourceDefinition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = HostName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (UserName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Password?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)AuthenticationType;
                hashCode = (hashCode * 397) ^ (DefaultQuery?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(WebServiceSourceDefinition left, WebServiceSourceDefinition right) => Equals(left, right);

        public static bool operator !=(WebServiceSourceDefinition left, WebServiceSourceDefinition right) => !Equals(left, right);

        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string DefaultQuery { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }
    }
}
