using System;
using Dev2.Common.Interfaces.DB;

namespace Warewolf.Studio.ViewModels
{
    public class DbInput:IDbInput, IEquatable<DbInput>
    {
        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(DbInput other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Name, other.Name) && string.Equals(Value, other.Value) && string.Equals(DefaultValue, other.DefaultValue) && RequiredField.Equals(other.RequiredField) && EmptyIsNull.Equals(other.EmptyIsNull);
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
            return Equals((DbInput)obj);
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
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DefaultValue != null ? DefaultValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RequiredField.GetHashCode();
                hashCode = (hashCode * 397) ^ EmptyIsNull.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DbInput left, DbInput right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DbInput left, DbInput right)
        {
            return !Equals(left, right);
        }

        #endregion

        public DbInput(string name, string value)
        {
            Name = name;
            Value = value;
            DefaultValue = "";
            RequiredField = true;
            EmptyIsNull = true;
        }

        #region Implementation of IDbInput

        public string Name { get; set; }

        public string Value { get; set; }
        public string DefaultValue { get; set; }
        public bool RequiredField { get; set; }
        public bool EmptyIsNull { get; set; }

        #endregion
    }
}
