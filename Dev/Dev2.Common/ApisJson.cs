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