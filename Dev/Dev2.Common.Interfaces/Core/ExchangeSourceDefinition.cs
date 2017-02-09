using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using System;

// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable MergeConditionalExpression

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

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
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

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>

        public bool Equals(IExchangeSource other)
        {
            return Equals(other as ExchangeSourceDefinition);
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
            return Equals((ExchangeSourceDefinition)obj);
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
                var hashCode = AutoDiscoverUrl != null ? AutoDiscoverUrl.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (UserName != null ? UserName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ExchangeSourceDefinition left, ExchangeSourceDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ExchangeSourceDefinition left, ExchangeSourceDefinition right)
        {
            return !Equals(left, right);
        }

        #endregion Equality members
    }
}