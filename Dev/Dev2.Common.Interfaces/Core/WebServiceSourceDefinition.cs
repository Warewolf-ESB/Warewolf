using System;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces.Core
{
    public class WebServiceSourceDefinition : IWebServiceSource, IEquatable<WebServiceSourceDefinition>
    {
        #region Implementation of IDbSource

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

        #region Equality members
        
        public bool Equals(WebServiceSourceDefinition other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(HostName, other.HostName) && Id == other.Id && string.Equals(Name, other.Name) && string.Equals(UserName, other.UserName) && string.Equals(Password, other.Password) && AuthenticationType == other.AuthenticationType && string.Equals(DefaultQuery, other.DefaultQuery);
        }

        public bool Equals(IWebServiceSource other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(HostName, other.HostName) && Id == other.Id && string.Equals(Name, other.Name) && string.Equals(UserName, other.UserName) && string.Equals(Password, other.Password) && AuthenticationType == other.AuthenticationType && string.Equals(DefaultQuery, other.DefaultQuery);
        }
        
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

        #endregion

        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string DefaultQuery { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }

        #endregion
    }
}
