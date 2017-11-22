using System;
using System.Windows.Input;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces.Core
{
    public class ServerSource : IServerSource, IEquatable<ServerSource>
    {
        public ServerSource()
        {
        }

        #region Implementation of IServerSource

        public string ServerName { get; set; }
        public string Address { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ICommand TestCommand { get; set; }
        public string TestMessage { get; set; }
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string ResourcePath { get; set; }

        #endregion

        #region Implementation of IEquatable<IServerSource>
        
        public bool Equals(IServerSource other)
        {
            return Equals(other as ServerSource);
        }

        #endregion

        #region Equality members
        
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

        public override int GetHashCode() => throw new NotImplementedException();

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