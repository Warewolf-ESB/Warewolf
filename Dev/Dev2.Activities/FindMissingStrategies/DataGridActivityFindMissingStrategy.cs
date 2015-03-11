
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.FindMissingStrategies
{
    /// <summary>
    /// Responsible for the find missing logic that applys to all the activities that only have a collection property on them
    /// </summary>
    public class DataGridActivityFindMissingStrategy : IFindMissingStrategy
    {
        #region Implementation of ISpookyLoadable<Enum>

        public Enum HandlesType()
        {
            return enFindMissingType.DataGridActivity;
        }

        /// <summary>
        /// Gets all the fields for a specific activity
        /// </summary>
        /// <param name="activity">The activity that the fields will be retrieved from</param>
        /// <returns>Returns all the fields in a list of strings</returns>
        public List<string> GetActivityFields(object activity)
        {
            List<string> results = new List<string>();
            Type activityType = activity.GetType();

            if (activityType == typeof(DsfBaseConvertActivity))
            {
                DsfBaseConvertActivity bcAct = activity as DsfBaseConvertActivity;
                if (bcAct != null)
                {
                    results.AddRange(InternalFindMissing(bcAct.ConvertCollection));
                }
            }
            else if (activityType == typeof(DsfCaseConvertActivity))
            {
                DsfCaseConvertActivity ccAct = activity as DsfCaseConvertActivity;
                if (ccAct != null)
                {
                    results.AddRange(InternalFindMissing(ccAct.ConvertCollection));
                }
            }
            else if (activityType == typeof(DsfMultiAssignActivity))
            {
                DsfMultiAssignActivity maAct = activity as DsfMultiAssignActivity;
                if (maAct != null)
                {
                    results.AddRange(InternalFindMissing(maAct.FieldsCollection));
                }
            }
            else if (activityType == typeof(DsfGatherSystemInformationActivity))
            {
                DsfGatherSystemInformationActivity maAct = activity as DsfGatherSystemInformationActivity;
                if (maAct != null)
                {
                    results.AddRange(InternalFindMissing(maAct.SystemInformationCollection));
                }
            }
            return results;
        }

        #endregion

        #region Private Methods

        private IList<string> InternalFindMissing<T>(IEnumerable<T> data)
        {
            IList<string> results = new List<string>();
            foreach (T row in data)
            {
                IEnumerable<PropertyInfo> properties = StringAttributeRefectionUtils.ExtractAdornedProperties<FindMissingAttribute>(row);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    object property = propertyInfo.GetValue(row, null);
                    if (property != null)
                    {
                        results.Add(property.ToString());
                    }
                }
            }
            return results;
        }

        #endregion
    }
}
