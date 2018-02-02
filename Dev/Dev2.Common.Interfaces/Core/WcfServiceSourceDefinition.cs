using Dev2.Common.Interfaces.Core.DynamicServices;
using System;

namespace Dev2.Common.Interfaces.Core
{
    public class WcfServiceSourceDefinition : IWcfServerSource, IEquatable<WcfServiceSourceDefinition>
    {
        public string ResourceName { get; set; }
        public Guid ResourceID { get; set; }
        public string EndpointUrl { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }
        public enSourceType Type { get; set; }
        public string ResourceType { get; set; }

        #region Equality members
        
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

        public bool Equals(IWcfServerSource other) => Equals(other as WcfServiceSourceDefinition);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EndpointUrl?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (EndpointUrl?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(WcfServiceSourceDefinition left, WcfServiceSourceDefinition right) => Equals(left, right);

        public static bool operator !=(WcfServiceSourceDefinition left, WcfServiceSourceDefinition right) => !Equals(left, right);

        #endregion Equality members
    }
}