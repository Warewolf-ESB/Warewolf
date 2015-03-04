using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Warewolf.Studio.ViewModels
{
    public class  DbAction:IDbAction, IEquatable<DbAction>
    {
        #region Implementation of IDbAction

        public IList<IDbInput> Inputs { get; set; }
        public string Name { get; set; }

        #endregion

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(DbAction other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(Inputs, other.Inputs) && string.Equals(Name, other.Name);
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
            return Equals((DbAction)obj);
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
                return ((Inputs != null ? Inputs.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public static bool operator ==(DbAction left, DbAction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DbAction left, DbAction right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}