using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Warewolf.Studio.ViewModels
{
    public class DatabaseService : IDatabaseService, IEquatable<DatabaseService>
    {
        
        #region Implementation of IDatabaseService
        public IDbSource Source { get; set; }
        public IDbAction Action { get; set; }
        public IList<IDbInput> Inputs { get; set; }
        public IList<IDbOutputMapping> OutputMappings { get; set; }
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
        public bool Equals(DatabaseService other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(Source, other.Source) && string.Equals(Action, other.Action) && Equals(Inputs, other.Inputs) && Equals(OutputMappings, other.OutputMappings);
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
            return Equals((DatabaseService)obj);
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
                var hashCode = (Source != null ? Source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Action != null ? Action.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Inputs != null ? Inputs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputMappings != null ? OutputMappings.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(DatabaseService left, DatabaseService right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DatabaseService left, DatabaseService right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}