using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Email;

namespace Dev2.Common.Interfaces.Core
{
    public class EmailServiceSourceDefinition : IEmailServiceSource, IEquatable<EmailServiceSourceDefinition>
    {
        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(EmailServiceSourceDefinition other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(HostName, other.HostName) && string.Equals(UserName, other.UserName) && string.Equals(Password, other.Password) 
                && EnableSsl == other.EnableSsl && string.Equals(Port, other.Port) && string.Equals(Timeout, other.Timeout);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
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

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (HostName != null ? HostName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UserName != null ? UserName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(EmailServiceSourceDefinition left, EmailServiceSourceDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EmailServiceSourceDefinition left, EmailServiceSourceDefinition right)
        {
            return !Equals(left, right);
        }

        #endregion

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
    }
}
