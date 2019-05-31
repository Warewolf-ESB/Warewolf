#pragma warning disable
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
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Common.Interfaces.Core
{
    public class EmailServiceSourceDefinition : IEmailServiceSource, IEquatable<EmailServiceSourceDefinition>
    {
        [ExcludeFromCodeCoverage]
        public EmailServiceSourceDefinition()
        {

        }

        public EmailServiceSourceDefinition(IEmailSource db)
        {
            Id = db.ResourceID;
            HostName = db.Host;
            Password = db.Password;
            UserName = db.UserName;
            Path = "";
            Port = db.Port;
            Timeout = db.Timeout;
            ResourceName = db.ResourceName;
            EnableSsl = db.EnableSsl;
        }

        public bool Equals(EmailServiceSourceDefinition other)
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
            equals &= string.Equals(UserName, other.UserName);
            equals &= string.Equals(Password, other.Password);
            equals &= EnableSsl == other.EnableSsl;
            equals &= Port == other.Port;
            equals &= Timeout == other.Timeout;

            return equals;
        }

        public bool Equals(IEmailServiceSource other)
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
            equals &= string.Equals(UserName, other.UserName);
            equals &= string.Equals(Password, other.Password);
            equals &= EnableSsl == other.EnableSsl;
            equals &= Port == other.Port;
            equals &= Timeout == other.Timeout;

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
            return Equals((EmailServiceSourceDefinition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = HostName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (UserName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Password?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(EmailServiceSourceDefinition left, EmailServiceSourceDefinition right) => Equals(left, right);

        public static bool operator !=(EmailServiceSourceDefinition left, EmailServiceSourceDefinition right) => !Equals(left, right);

        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public int Port { get; set; }
        public int Timeout { get; set; }
        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }
        public string ResourceName { get; set; }
    }
}
