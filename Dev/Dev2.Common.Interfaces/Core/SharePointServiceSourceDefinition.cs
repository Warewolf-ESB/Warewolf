using System;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces.Core
{
    public class SharePointServiceSourceDefinition : ISharepointServerSource, IEquatable<SharePointServiceSourceDefinition>
    {

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

        public SharePointServiceSourceDefinition()
        {
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string Server { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }
        public bool IsSharepointOnline { get; set; }

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(SharePointServiceSourceDefinition other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Server, other.Server) && string.Equals(UserName, other.UserName) && string.Equals(Password, other.Password) && AuthenticationType == other.AuthenticationType;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ISharepointServerSource other)
        {
            return Equals(other as SharePointServiceSourceDefinition);
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
            return Equals((SharePointServiceSourceDefinition)obj);
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
                var hashCode = Server?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (UserName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Password?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)AuthenticationType;
                return hashCode;
            }
        }

        public static bool operator ==(SharePointServiceSourceDefinition left, SharePointServiceSourceDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SharePointServiceSourceDefinition left, SharePointServiceSourceDefinition right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
