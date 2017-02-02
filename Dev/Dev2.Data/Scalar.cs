
using System;
using System.Collections.Generic;
using Dev2.Data.Binary_Objects;

namespace Dev2.Data
{
    public interface IScalar
    {
        string Name { get; set; }
        enDev2ColumnArgumentDirection IODirection { get; set; }
        string Description { get; set; }
        bool IsEditable { get; set; }
        string Value { get; set; }
    }

    public class Scalar : IScalar, IEquatable<IScalar>
    {
        public string Name { get; set; }
        public enDev2ColumnArgumentDirection IODirection { get; set; }
        public string Description { get; set; }
        public bool IsEditable { get; set; }
        public string Value { get; set; }

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IScalar other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Name, other.Name);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
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
            return Equals((Scalar)obj);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Scalar left, Scalar right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Scalar left, Scalar right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region ComparerEqualityComparer

        private sealed class ComparerEqualityComparer : IEqualityComparer<IScalar>
        {
            public bool Equals(IScalar x, IScalar y)
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
                return string.Equals(x.Name, y.Name);
            }

            public int GetHashCode(IScalar obj)
            {
                return obj.Name?.GetHashCode() ?? 0;
            }
        }

        private static readonly IEqualityComparer<IScalar> ComparerInstance = new ComparerEqualityComparer();
        public static IEqualityComparer<IScalar> Comparer => ComparerInstance;

        #endregion
    }
}