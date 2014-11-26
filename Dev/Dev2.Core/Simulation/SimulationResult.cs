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
using Dev2.DataList.Contract.Binary_Objects;

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
        public IBinaryDataList Value { get; set; }

        #endregion

        #region ISerializable

        protected SimulationResult(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            Key = (ISimulationKey) info.GetValue("Key", typeof (ISimulationKey));
            Value = (IBinaryDataList) info.GetValue("Value", typeof (IBinaryDataList));
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

        public bool Equals(ISimulationResult other)
        {
            if (other == null)
            {
                return false;
            }
            return Key.Equals(other.Key);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var item = obj as ISimulationResult;
            return item != null && Equals(item);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        #endregion
    }
}