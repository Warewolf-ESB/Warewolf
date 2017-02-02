

using System;
using Dev2.Common.Interfaces;

namespace WarewolfParserInterop
{
    public class AssignValue : IAssignValue, IEquatable<AssignValue>
    {
        public AssignValue(string name, string value)
        {
            Value = value;
            Name = name;
        }

        #region Implementation of IAssignValue

        public string Name { get; private set; }
        public string Value { get; private set; }



        #endregion

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param> //fenerated
        public bool Equals(AssignValue other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Name, other.Name) && string.Equals(Value, other.Value);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param> //fenerated
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
            if(obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((AssignValue)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns> //fenerated
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name?.GetHashCode() ?? 0) * 397) ^ (Value?.GetHashCode() ?? 0);
            }
        } //fenerated
        public static bool operator ==(AssignValue left, AssignValue right)
        {
            return Equals(left, right);
        } //fenerated
        public static bool operator !=(AssignValue left, AssignValue right)
        {
            return !Equals(left, right);
        }

        #endregion
    }


   

}
