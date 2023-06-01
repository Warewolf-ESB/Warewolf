/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    [Serializable]
    public class DllListing : IFileListing
    {
        public DllListing(IDllListingModel selectedDll)
        {
            Name = selectedDll.Name;
            FullName = selectedDll.FullName;
            IsDirectory = selectedDll.IsDirectory;
            ClsId = selectedDll.ClsId;
            Is32Bit = selectedDll.Is32Bit;
        }

        public DllListing()
        {
        }

        public bool Equals(IFileListing other)
        {
            var equals = true;
            equals &= string.Equals(Name, other.Name);
            equals &= string.Equals(FullName, other.FullName);
            equals &= IsDirectory == other.IsDirectory;

            return equals;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
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
            return Equals((DllListing)obj);
        }
        
        public override int GetHashCode()
        {
            unchecked
			{
				var hashCode = Name?.GetHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ (Children?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (FullName?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ IsDirectory.GetHashCode();
				return hashCode;
			}
        }

        public static bool operator ==(DllListing left, DllListing right) => Equals(left, right);

        public static bool operator !=(DllListing left, DllListing right) => !Equals(left, right);

        public string Name { get; set; }
        public ICollection<IFileListing> Children { get; set; }
        public string FullName { get; set; }
        public bool IsDirectory { get; set; }
        public bool Is32Bit { get; set; }
        public string ClsId { get; set; }
    }
}
