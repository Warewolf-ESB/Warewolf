using System;
using System.Windows.Input;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces.Core
{
    public class ServerSource : IServerSource, IEquatable<ServerSource>
    {
        public ServerSource(IConnection connection)
        {
            Address = connection.Address;
            ID = connection.ResourceID;
            AuthenticationType = connection.AuthenticationType;
            UserName = connection.UserName;
            Password = connection.Password;
            ResourcePath = "";
            ServerName = "";
            Name = connection.ResourceName;
        }

        public ServerSource()
        {
        }

        #region Implementation of IServerSource

        public string ServerName { get; set; }
        /// <summary>
        /// The server address that we are trying to connect to
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        ///  Windows or user or publlic
        /// </summary>
        public AuthenticationType AuthenticationType { get; set; }
        /// <summary>
        /// User Name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Test if connection is successful
        /// </summary>
        public ICommand TestCommand { get; set; }
        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        public string TestMessage { get; set; }
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string ResourcePath { get; set; }

        #endregion

        #region Implementation of IEquatable<IServerSource>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IServerSource other)
        {
            return Equals(other as ServerSource);
        }

        #endregion

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ServerSource other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            var equals = string.Equals(Address, other.Address,StringComparison.InvariantCultureIgnoreCase) && AuthenticationType == other.AuthenticationType && string.Equals(UserName, other.UserName, StringComparison.InvariantCultureIgnoreCase) && string.Equals(Password, other.Password) && string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
            return equals;
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
            return Equals((ServerSource)obj);
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
                var hashCode = Address?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (int)AuthenticationType;
                hashCode = (hashCode * 397) ^ (UserName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Password?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ ID.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (ResourcePath?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(ServerSource left, ServerSource right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ServerSource left, ServerSource right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}