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

namespace Dev2.Common
{
    public class IncludeApi : IEquatable<IncludeApi>
    {
        public bool Equals(IncludeApi other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Name, other.Name) && string.Equals(Url, other.Url);
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
            return Equals((IncludeApi)obj);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name?.GetHashCode() ?? 0) * 397) ^ (Url?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(IncludeApi left, IncludeApi right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IncludeApi left, IncludeApi right)
        {
            return !Equals(left, right);
        }

        public string Name { get; set; }
        public string Url { get; set; }
    }
}