using Dev2.Common.Interfaces.Core.DynamicServices;
using System;

namespace Dev2.Common.Interfaces.Core
{
    public class WcfServiceSourceDefinition : IWcfServerSource, IEquatable<WcfServiceSourceDefinition>
    {
        public WcfServiceSourceDefinition(IWcfSource wcfsource)
        {
            Id = wcfsource.Id;
            Name = wcfsource.ResourceName;
            Path = wcfsource.GetSavePath();
            ResourceName = wcfsource.Name;
            EndpointUrl = wcfsource.EndpointUrl;
        }

        public WcfServiceSourceDefinition()
        {
        }

        public string ResourceName { get; set; }
        public Guid ResourceID { get; set; }
        public string EndpointUrl { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }
        public enSourceType Type { get; set; }
        public string ResourceType { get; set; }

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(WcfServiceSourceDefinition other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(EndpointUrl, other.EndpointUrl);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IWcfServerSource other)
        {
            return Equals(other as WcfServiceSourceDefinition);
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
            return Equals((WcfServiceSourceDefinition)obj);
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
                var hashCode = EndpointUrl?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (EndpointUrl?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(WcfServiceSourceDefinition left, WcfServiceSourceDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WcfServiceSourceDefinition left, WcfServiceSourceDefinition right)
        {
            return !Equals(left, right);
        }

        #endregion Equality members
    }
}