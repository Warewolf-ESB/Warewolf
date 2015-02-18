using System;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces.Core
{
    public class DbSourceDefinition : IDbSource, IEquatable<DbSourceDefinition>
    {
        #region Implementation of IDbSource

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(DbSourceDefinition other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(ServerName, other.ServerName) && Type == other.Type && string.Equals(UserName, other.UserName) && string.Equals(Password, other.Password) && AuthenticationType == other.AuthenticationType && string.Equals(DbName, other.DbName);
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
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((DbSourceDefinition)obj);
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
                var hashCode = (ServerName != null ? ServerName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Type;
                hashCode = (hashCode * 397) ^ (UserName != null ? UserName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)AuthenticationType;
                hashCode = (hashCode * 397) ^ (DbName != null ? DbName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(DbSourceDefinition left, DbSourceDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DbSourceDefinition left, DbSourceDefinition right)
        {
            return !Equals(left, right);
        }

        #endregion

        public string ServerName { get; set; }
        public enSourceType Type { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string DbName { get; set; }
        public string Name { get; set; }
        public string Path  { get; set; }
        public Guid Id { get; set; }

        #endregion
    }
}