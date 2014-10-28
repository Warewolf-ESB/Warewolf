/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
    public class SimulationKey : ISimulationKey
    {
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
            if (obj == null)
            {
                return false;
            }

            var item = obj as ISimulationKey;
            return item != null && Equals(item);
        }

        public override int GetHashCode()
        {
            string workflowID = string.IsNullOrEmpty(WorkflowID) ? string.Empty : WorkflowID;
            string activityID = string.IsNullOrEmpty(ActivityID) ? string.Empty : ActivityID;
            string scenarioID = string.IsNullOrEmpty(ScenarioID) ? string.Empty : ScenarioID;
            return workflowID.GetHashCode() ^
                   activityID.GetHashCode() ^
                   scenarioID.GetHashCode();
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