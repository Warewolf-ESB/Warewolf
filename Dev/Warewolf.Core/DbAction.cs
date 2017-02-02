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
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.DB;

namespace Warewolf.Core
{
    public class DbAction : IDbAction, IEquatable<DbAction>
    {
        #region Implementation of IDbAction

        public IList<IServiceInput> Inputs { get; set; }
        public string Name { get; set; }
        public Guid SourceId { get; set; }
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
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (GetHashCode() == other.GetHashCode())
                return true;

            bool inputseq;
            if(Inputs != null&& other.Inputs != null)
            {

                inputseq = Inputs.Zip(other.Inputs, (a, b) => new Tuple<IServiceInput, IServiceInput>(a, b)).All(a => a.Item1.Equals(a.Item2));
                    
              
            }
            else
            {
                inputseq =Equals(Inputs, other.Inputs) ;
            }
            
            return inputseq&& string.Equals(Name, other.Name);
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
                return ((Inputs?.GetHashCode() ?? 0) * 397) ^ (Name?.GetHashCode() ?? 0);
            }
        }

        public  string GetIdentifier()
        {
            return SourceId + Name;
            
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

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
