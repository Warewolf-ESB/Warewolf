using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    [Serializable]
    public class DllListing : IFileListing
    {
        public DllListing(IFileListing selectedDll)
        {
            Name = selectedDll.Name;
            FullName = selectedDll.FullName;
            IsDirectory = selectedDll.IsDirectory;
            ClsId = (selectedDll as DllListing)?.ClsId;
            Is32Bit = (selectedDll as DllListing)?.Is32Bit ?? false;
        }

        public DllListing()
        {
        }
        #region Equality members

        public bool Equals(IFileListing other)
        {
            return string.Equals(Name, other.Name) && string.Equals(FullName, other.FullName) && IsDirectory == other.IsDirectory;
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
            return Equals((DllListing)obj);
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
                hashCode = (hashCode * 397) ^ (Children?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (FullName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ IsDirectory.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DllListing left, DllListing right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DllListing left, DllListing right)
        {
            return !Equals(left, right);
        }

        #endregion

        public string Name { get; set; }
        public ICollection<IFileListing> Children { get; set; }
        public string FullName { get; set; }
        public bool IsDirectory { get; set; }
        public bool Is32Bit { get; set; }
        public string ClsId { get; set; }
    }
}
