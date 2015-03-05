using System;
using Dev2.Common.Interfaces.DB;

namespace Warewolf.Core
{
    public class DbOutputMapping : IDbOutputMapping, IEquatable<DbOutputMapping>
    {
        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(DbOutputMapping other)
        {
            return false;
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
            return Equals((DbOutputMapping)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return 397 ^ Name.GetHashCode() ^ OutputName.GetHashCode();
        }

        public static bool operator ==(DbOutputMapping left, DbOutputMapping right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DbOutputMapping left, DbOutputMapping right)
        {
            return !Equals(left, right);
        }

        #endregion

        public DbOutputMapping(string name, string mapping)
        {
            Name = name;
            OutputName = mapping;
        }

        #region Implementation of IDbOutputMapping

        public string Name { get; set; }
        public string OutputName { get; set; }
        public string RecordSetName { get; set; }

        #endregion
    }
}