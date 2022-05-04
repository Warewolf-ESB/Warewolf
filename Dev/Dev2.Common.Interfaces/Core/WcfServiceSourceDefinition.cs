#if NETFRAMEWORK
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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

        public override bool Equals(object obj)
        {
            if (obj is WcfServiceSourceDefinition wsd)
            {
                return Equals(wsd);
            }
            if (obj is IWcfServerSource wcfServerSource)
            {
                return Equals(wcfServerSource);
            }
            return false;
        }

        public bool Equals(WcfServiceSourceDefinition other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(EndpointUrl, other.EndpointUrl);
        }

        public bool Equals(IWcfServerSource other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(EndpointUrl, other.EndpointUrl);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EndpointUrl?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (EndpointUrl?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(WcfServiceSourceDefinition left, WcfServiceSourceDefinition right) => left.Equals(right);

        public static bool operator !=(WcfServiceSourceDefinition left, WcfServiceSourceDefinition right) => !left.Equals(right);
    }
}
#endif