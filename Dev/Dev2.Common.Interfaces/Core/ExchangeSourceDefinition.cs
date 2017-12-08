using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using System;


namespace Dev2.Common.Interfaces.Core
{
    public class ExchangeSourceDefinition : IExchangeSource, IEquatable<ExchangeSourceDefinition>
    {
        public ExchangeSourceDefinition(IExchange db)
        {
            AutoDiscoverUrl = db.AutoDiscoverUrl;
            Id = db.ResourceID;
            Password = db.Password;
            UserName = db.UserName;
            Path = "";
            Timeout = db.Timeout;
            ResourceName = db.ResourceName;
        }

        public ExchangeSourceDefinition()
        {
        }

        public Guid ResourceID { get; set; }
        public string AutoDiscoverUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public enSourceType Type { get; set; }
        public string ResourceType { get; set; }
        public int Timeout { get; set; }
        public string EmailTo { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }
        public string ResourceName { get; set; }

        #region Equality members
        
        public bool Equals(ExchangeSourceDefinition other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(AutoDiscoverUrl, other.AutoDiscoverUrl) && string.Equals(UserName, other.UserName) && string.Equals(Password, other.Password)
                && Timeout == other.Timeout;
        }
        
        public bool Equals(IExchangeSource other)
        {
            return Equals(other as ExchangeSourceDefinition);
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
            return Equals((ExchangeSourceDefinition)obj);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = AutoDiscoverUrl != null ? AutoDiscoverUrl.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (UserName != null ? UserName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion Equality members
    }
}