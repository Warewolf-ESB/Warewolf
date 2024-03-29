/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Activities.Sharepoint;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.FindMissingStrategies
{
    class MixedActivityFindMissingStrategy : IFindMissingStrategy
    {
        #region Implementation of ISpookyLoadable<Enum>

        public Enum HandlesType() => enFindMissingType.MixedActivity;

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        public List<string> GetActivityFields(object activity)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var results = new List<string>();
            var activityType = activity.GetType();

            if (activityType == typeof(DsfDataSplitActivity) && activity is DsfDataSplitActivity dataSplitActivity)
            {
                results.AddRange(InternalFindMissing(dataSplitActivity.ResultsCollection));
                if (!string.IsNullOrEmpty(dataSplitActivity.SourceString))
                {
                    results.Add(dataSplitActivity.SourceString);
                }
            }


            if (activityType == typeof(DsfCreateJsonActivity) && activity is DsfCreateJsonActivity createJsonActivity)
            {
                results.AddRange(InternalFindMissing(createJsonActivity.JsonMappings));
                if (!string.IsNullOrEmpty(createJsonActivity.JsonString))
                {
                    results.Add(createJsonActivity.JsonString);
                }
            }

            if (activityType == typeof(SharepointReadListActivity) && activity is SharepointReadListActivity sharepointReadListActivity)
            {
                results.AddRange(InternalFindMissing(sharepointReadListActivity.ReadListItems));
                if (sharepointReadListActivity.FilterCriteria != null)
                {
                    results.AddRange(InternalFindMissing(sharepointReadListActivity.FilterCriteria));
                }
            }

            if (activityType == typeof(SharepointCreateListItemActivity) && activity is SharepointCreateListItemActivity sharepointCreateListItemActivity)
            {
                results.AddRange(InternalFindMissing(sharepointCreateListItemActivity.ReadListItems));
                results.Add(sharepointCreateListItemActivity.Result);
            }

            if (activityType == typeof(SharepointDeleteListItemActivity) && activity is SharepointDeleteListItemActivity sharepointDeleteListItemActivity)
            {
                results.AddRange(InternalFindMissing(sharepointDeleteListItemActivity.FilterCriteria));
                results.Add(sharepointDeleteListItemActivity.DeleteCount);
            }

            if (activityType == typeof(SharepointUpdateListItemActivity))
            {
                if (activity is SharepointUpdateListItemActivity dsAct)
                {
                    results.AddRange(InternalFindMissing(dsAct.ReadListItems));
                    results.AddRange(InternalFindMissing(dsAct.FilterCriteria));
                    results.Add(dsAct.Result);
                }
            }
            else if (activityType == typeof(DsfDataMergeActivity))
            {
                if (activity is DsfDataMergeActivity dmAct)
                {
                    results.AddRange(InternalFindMissing(dmAct.MergeCollection));
                    if (!string.IsNullOrEmpty(dmAct.Result))
                    {
                        results.Add(dmAct.Result);
                    }
                }
            }
            else if (activityType == typeof(DsfXPathActivity))
            {
                if (activity is DsfXPathActivity xpAct)
                {
                    results.AddRange(InternalFindMissing(xpAct.ResultsCollection));
                    if (!string.IsNullOrEmpty(xpAct.SourceString))
                    {
                        results.Add(xpAct.SourceString);
                    }
                }
            }
            else if (activityType == typeof(DsfSqlBulkInsertActivity))
            {
                if (activity is DsfSqlBulkInsertActivity sbiAct)
                {
                    results.AddRange(InternalFindMissing(sbiAct.InputMappings));
                    if (!string.IsNullOrEmpty(sbiAct.Result))
                    {
                        results.Add(sbiAct.Result);
                    }
                }
            }
            else
            {
                if (activityType == typeof(DsfFindRecordsMultipleCriteriaActivity) && activity is DsfFindRecordsMultipleCriteriaActivity frmAct)
                {
                    results.AddRange(InternalFindMissing(frmAct.ResultsCollection));
                    if (!string.IsNullOrEmpty(frmAct.FieldsToSearch))
                    {
                        results.Add(frmAct.FieldsToSearch);
                    }
                    if (!string.IsNullOrEmpty(frmAct.Result))
                    {
                        results.Add(frmAct.Result);
                    }
                }

            }

            if (activity is DsfNativeActivity<string> act)
            {
                if (!string.IsNullOrEmpty(act.OnErrorVariable))
                {
                    results.Add(act.OnErrorVariable);
                }

                if (!string.IsNullOrEmpty(act.OnErrorWorkflow))
                {
                    results.Add(act.OnErrorWorkflow);
                }
            }

            return results;
        }

        #endregion

        #region Private Methods

        static IEnumerable<string> InternalFindMissing<T>(IEnumerable<T> data)
        {
            IList<string> results = new List<string>();

            foreach (T row in data)

            {
                var properties = StringAttributeRefectionUtils.ExtractAdornedProperties<FindMissingAttribute>(row);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    var property = propertyInfo.GetValue(row, null);
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
