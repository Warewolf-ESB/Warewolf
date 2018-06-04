using System;

namespace Dev2.Common.Interfaces.Core
{
    public class EmailServiceSourceDefinition : IEmailServiceSource, IEquatable<EmailServiceSourceDefinition>
    {
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
        #region Equality members
        
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

        public bool Equals(IEmailServiceSource other)
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
        public string ResourceName { get; set; }
    }
}
