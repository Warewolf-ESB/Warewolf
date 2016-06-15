/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Runtime.Serialization;

namespace Dev2.Simulation
{
    [Serializable]
    public class SimulationResult : ISimulationResult
    {
        public SimulationResult()
        {
        }

        #region Key

        /// <summary>
        ///     Gets or sets the unique key.
        /// </summary>
        public ISimulationKey Key { get; set; }

        #endregion

        #region Value

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public object Value { get; set; }

        #endregion

        #region ISerializable

        protected SimulationResult(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            Key = (ISimulationKey) info.GetValue("Key", typeof (ISimulationKey));
            Value = info.GetValue("Value", typeof (object));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("Key", Key);
            info.AddValue("Value", Value);
        }

        #endregion

        #region IEquatable

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ISimulationResult other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(Key, other.Key);
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
            return Equals((SimulationResult)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return Key != null ? Key.GetHashCode() : 0;
        }

        public static bool operator ==(SimulationResult left, SimulationResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SimulationResult left, SimulationResult right)
        {
            return !Equals(left, right);
        }

        #endregion

        #endregion
    }
}