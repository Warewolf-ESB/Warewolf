#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
    public class ApisJson : IEquatable<ApisJson>
    {
        public bool Equals(ApisJson other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Name, other.Name) && string.Equals(Description, other.Description) && string.Equals(Image, other.Image) && string.Equals(Url, other.Url) && Created.Equals(other.Created) && Modified.Equals(other.Modified) && string.Equals(SpecificationVersion, other.SpecificationVersion);
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
            return Equals((ApisJson)obj);
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        public override int GetHashCode()
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            unchecked
            {
                var hashCode = Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Image?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Url?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Tags?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Created.GetHashCode();
                hashCode = (hashCode * 397) ^ Modified.GetHashCode();
                hashCode = (hashCode * 397) ^ (SpecificationVersion?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Apis?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Include?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Maintainers?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(ApisJson left, ApisJson right) => Equals(left, right);

        public static bool operator !=(ApisJson left, ApisJson right) => !Equals(left, right);

        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
        public List<string> Tags { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string SpecificationVersion { get; set; }
        public List<SingleApi> Apis { get; set; }
        public List<IncludeApi> Include { get; set; }
        public List<MaintainerApi> Maintainers { get; set; }
    }
}