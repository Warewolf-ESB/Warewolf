/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Dev2.Common
{
    public class SingleApi : IEquatable<SingleApi>
    {
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(SingleApi other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Name, other.Name) && string.Equals(Description, other.Description) && string.Equals(Image, other.Image) && string.Equals(HumanUrl, other.HumanUrl) && string.Equals(BaseUrl, other.BaseUrl) && string.Equals(Version, other.Version) && Equals(Tags, other.Tags) && Equals(Properties, other.Properties) && Equals(Contact, other.Contact);
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
            return Equals((SingleApi)obj);
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
                var hashCode = Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Image?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (HumanUrl?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (BaseUrl?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Version?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Tags?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Properties?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Contact?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(SingleApi left, SingleApi right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SingleApi left, SingleApi right)
        {
            return !Equals(left, right);
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        [Newtonsoft.Json.JsonProperty("humanUrl")]
        public string HumanUrl { get; set; }

        [Newtonsoft.Json.JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }

        public string Version { get; set; }
        public List<string> Tags { get; set; }

        [Newtonsoft.Json.JsonProperty("properties")]
        public List<PropertyApi> Properties { get; set; }

        [Newtonsoft.Json.JsonProperty("contact")]
        public List<MaintainerApi> Contact { get; set; }
    }
}