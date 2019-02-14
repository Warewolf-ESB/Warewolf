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
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces.Core
{
    public class SharePointServiceSourceDefinition : ISharepointServerSource, IEquatable<SharePointServiceSourceDefinition>
    {
        [ExcludeFromCodeCoverage]
        public SharePointServiceSourceDefinition()
        {
        }

        public SharePointServiceSourceDefinition(ISharepointSource db)
        {
            AuthenticationType = db.AuthenticationType;
            Server = db.Server;
            Path = db.GetSavePath();
            Id = db.ResourceID;
            Name = db.ResourceName;
            Password = db.Password;
            UserName = db.UserName;
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string Server { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }
        public bool IsSharepointOnline { get; set; }
        
        public bool Equals(SharePointServiceSourceDefinition other)
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
            equals &= string.Equals(Server, other.Server);
            equals &= string.Equals(UserName, other.UserName);
            equals &= string.Equals(Password, other.Password);
            equals &= AuthenticationType == other.AuthenticationType;
            return equals;
        }

        public bool Equals(ISharepointServerSource other)
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
            equals &= string.Equals(Server, other.Server);
            equals &= string.Equals(UserName, other.UserName);
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
            return Equals((SharePointServiceSourceDefinition)obj);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Server?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (UserName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Password?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)AuthenticationType;
                return hashCode;
            }
        }

        public static bool operator ==(SharePointServiceSourceDefinition left, SharePointServiceSourceDefinition right) => Equals(left, right);

        public static bool operator !=(SharePointServiceSourceDefinition left, SharePointServiceSourceDefinition right) => !Equals(left, right);
    }
}
