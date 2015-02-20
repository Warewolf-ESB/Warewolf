using System;
using System.Collections.Generic;
using System.Reflection;
using Dev2.Common.Interfaces.Toolbox;

namespace Warewolf.Core
{
 

    public class WarewolfType : IWarewolfType, IEquatable<WarewolfType>
    {
        public WarewolfType(string fullyQualifiedName, Version version, string containingAssemblyPath)
        {
            ContainingAssemblyPath = containingAssemblyPath;
            Version = version;
            FullyQualifiedName = fullyQualifiedName;           
        }

        #region Implementation of IWarewolfType

        public string FullyQualifiedName { get; private set; }
        public Version Version { get; private set; }
        public string ContainingAssemblyPath { get; private set; }

        public Type ActualType { get; set; }

        #endregion

        #region Overrides of Object

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(WarewolfType other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(FullyQualifiedName, other.FullyQualifiedName) && Equals(Version, other.Version) && string.Equals(ContainingAssemblyPath, other.ContainingAssemblyPath);
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
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((WarewolfType)obj);
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
                var hashCode = (FullyQualifiedName != null ? FullyQualifiedName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Version != null ? Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ContainingAssemblyPath != null ? ContainingAssemblyPath.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(WarewolfType left, WarewolfType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WarewolfType left, WarewolfType right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region WarewolfTypeEqualityComparer

        sealed class WarewolfTypeEqualityComparer : IEqualityComparer<WarewolfType>
        {
            public bool Equals(WarewolfType x, WarewolfType y)
            {
                if(ReferenceEquals(x, y))
                {
                    return true;
                }
                if(ReferenceEquals(x, null))
                {
                    return false;
                }
                if(ReferenceEquals(y, null))
                {
                    return false;
                }
                if(x.GetType() != y.GetType())
                {
                    return false;
                }
                return string.Equals(x.FullyQualifiedName, y.FullyQualifiedName) && Equals(x.Version, y.Version) && string.Equals(x.ContainingAssemblyPath, y.ContainingAssemblyPath);
            }

            public int GetHashCode(WarewolfType obj)
            {
                unchecked
                {
                    var hashCode = (obj.FullyQualifiedName != null ? obj.FullyQualifiedName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Version != null ? obj.Version.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.ContainingAssemblyPath != null ? obj.ContainingAssemblyPath.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        static readonly IEqualityComparer<WarewolfType> WarewolfTypeComparerInstance = new WarewolfTypeEqualityComparer();
        public static IEqualityComparer<WarewolfType> WarewolfTypeComparer
        {
            get
            {
                return WarewolfTypeComparerInstance;
            }
        }

        #endregion

        #endregion
    }
}
