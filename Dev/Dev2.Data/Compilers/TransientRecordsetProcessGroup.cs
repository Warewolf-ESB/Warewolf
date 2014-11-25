
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.DataList.Contract;

namespace Dev2.Data.Compilers
{
    /// <summary>
    /// Used to process recordset data for upsert ;)
    /// </summary>
    internal class TransientRecordsetProcessGroup
    {

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string TargetValue { get; private set; }

        /// <summary>
        /// Gets the target value.
        /// </summary>
        /// <value>
        /// The target value.
        /// </value>
        public enRecordsetIndexType IdxType { get; private set; }

        public bool IsTargetRecordSet { get; private set; }


        internal TransientRecordsetProcessGroup(string targetValue, enRecordsetIndexType typeOf, bool isRS)
        {
            TargetValue = targetValue;
            IdxType = typeOf;
            IsTargetRecordSet = isRS;
        }
    }
}
