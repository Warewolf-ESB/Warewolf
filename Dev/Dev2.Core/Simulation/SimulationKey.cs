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
using System.Runtime.Serialization;

namespace Dev2.Simulation
{
    [Serializable]
    public class SimulationKey : ISimulationKey, IEquatable<SimulationKey>
    {
        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(SimulationKey other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(WorkflowID, other.WorkflowID) && string.Equals(ActivityID, other.ActivityID) && string.Equals(ScenarioID, other.ScenarioID);
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
                var hashCode = WorkflowID?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (ActivityID?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (ScenarioID?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(SimulationKey left, SimulationKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SimulationKey left, SimulationKey right)
        {
            return !Equals(left, right);
        }

        #endregion

        public SimulationKey()
        {
        }

        #region ISerializable

        protected SimulationKey(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            WorkflowID = info.GetString("WorkflowID");
            ActivityID = info.GetString("ActivityID");
            ScenarioID = info.GetString("ScenarioID");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("WorkflowID", WorkflowID);
            info.AddValue("ActivityID", ActivityID);
            info.AddValue("ScenarioID", ScenarioID);
        }

        #endregion

        #region IEquatable

        public bool Equals(ISimulationKey other)
        {
            if (other == null)
            {
                return false;
            }
            return WorkflowID.Equals(other.WorkflowID)
                   && ActivityID.Equals(other.ActivityID)
                   && ScenarioID.Equals(other.ScenarioID);
        }

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
            return Equals((SimulationKey)obj);
        }


        #endregion

        public string WorkflowID { get; set; }
        public string ActivityID { get; set; }
        public string ScenarioID { get; set; }

        /// <summary>
        ///     Overridden to return file name friendly string for the key
        /// </summary>
        /// <returns>The string representation of the key.</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(ScenarioID)
                ? string.Format("{0}-{1}", WorkflowID, ActivityID)
                : string.Format("{0}-{1}-{2}", WorkflowID, ActivityID, ScenarioID);
        }
    }
}