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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Dev2.Activities;
using Dev2.Activities.Sharepoint;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.FindMissingStrategies
{
    /// <summary>
    /// Responsible for the find missing logic that apply to all the activities that have a collection property and some static properties on them
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")] //This is loaded based on SpookyAction implementing IFindMissingStrategy
    class MixedActivityFindMissingStrategy : IFindMissingStrategy
    {
        #region Implementation of ISpookyLoadable<Enum>

        public Enum HandlesType()
        {
            return enFindMissingType.MixedActivity;
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

            if(activityType == typeof(DsfDataSplitActivity))
            {
                DsfDataSplitActivity dsAct = activity as DsfDataSplitActivity;
                if(dsAct != null)
                {
                    results.AddRange(InternalFindMissing(dsAct.ResultsCollection));
                    if(!string.IsNullOrEmpty(dsAct.SourceString))
                    {
                        results.Add(dsAct.SourceString);
                    }
                }
            }            
            
            if(activityType == typeof(DsfCreateJsonActivity))
            {
                var dsAct = activity as DsfCreateJsonActivity;
                if(dsAct != null)
                {
                    results.AddRange(InternalFindMissing(dsAct.JsonMappings));
                    if(!string.IsNullOrEmpty(dsAct.JsonString))
                    {
                        results.Add(dsAct.JsonString);
                    }
                }
            }
            if (activityType == typeof(SharepointReadListActivity))
            {
                var dsAct = activity as SharepointReadListActivity;
                if (dsAct != null)
                {
                    results.AddRange(InternalFindMissing(dsAct.ReadListItems));
                    if(dsAct.FilterCriteria != null)
                    {
                        results.AddRange(InternalFindMissing(dsAct.FilterCriteria));
                    }
                }
            }
            if (activityType == typeof(SharepointCreateListItemActivity))
            {
                var dsAct = activity as SharepointCreateListItemActivity;
                if (dsAct != null)
                {
                    results.AddRange(InternalFindMissing(dsAct.ReadListItems));
                    results.Add(dsAct.Result);
                }
            }
            if (activityType == typeof(SharepointDeleteListItemActivity))
            {
                var dsAct = activity as SharepointDeleteListItemActivity;
                if (dsAct != null)
                {
                    results.AddRange(InternalFindMissing(dsAct.FilterCriteria));
                    results.Add(dsAct.DeleteCount);
                }
                
            }
            if (activityType == typeof(SharepointUpdateListItemActivity))
            {
                var dsAct = activity as SharepointUpdateListItemActivity;
                if (dsAct != null)
                {
                    results.AddRange(InternalFindMissing(dsAct.ReadListItems));
                    results.AddRange(InternalFindMissing(dsAct.FilterCriteria));
                    results.Add(dsAct.Result);
                }
            }
            else if(activityType == typeof(DsfDataMergeActivity))
            {
                DsfDataMergeActivity dmAct = activity as DsfDataMergeActivity;
                if(dmAct != null)
                {
                    results.AddRange(InternalFindMissing(dmAct.MergeCollection));
                    if(!string.IsNullOrEmpty(dmAct.Result))
                    {
                        results.Add(dmAct.Result);
                    }
                }
            }
            else if(activityType == typeof(DsfXPathActivity))
            {
                DsfXPathActivity xpAct = activity as DsfXPathActivity;
                if(xpAct != null)
                {
                    results.AddRange(InternalFindMissing(xpAct.ResultsCollection));
                    if(!string.IsNullOrEmpty(xpAct.SourceString))
                    {
                        results.Add(xpAct.SourceString);
                    }
                }
            }
            else if(activityType == typeof(DsfSqlBulkInsertActivity))
            {
                var sbiAct = activity as DsfSqlBulkInsertActivity;
                if(sbiAct != null)
                {
                    results.AddRange(InternalFindMissing(sbiAct.InputMappings));
                    if(!string.IsNullOrEmpty(sbiAct.Result))
                    {
                        results.Add(sbiAct.Result);
                    }
                }
            }
            else if(activityType == typeof(DsfFindRecordsMultipleCriteriaActivity))
            {
                DsfFindRecordsMultipleCriteriaActivity frmAct = activity as DsfFindRecordsMultipleCriteriaActivity;
                if(frmAct != null)
                {
                    results.AddRange(InternalFindMissing(frmAct.ResultsCollection));
                    if(!string.IsNullOrEmpty(frmAct.FieldsToSearch))
                    {
                        results.Add(frmAct.FieldsToSearch);
                    }
                    if(!string.IsNullOrEmpty(frmAct.Result))
                    {
                        results.Add(frmAct.Result);
                    }
                }
            }

            var act = activity as DsfNativeActivity<string>;
            if(act != null)
            {
                if(!string.IsNullOrEmpty(act.OnErrorVariable))
                {
                    results.Add(act.OnErrorVariable);
                }

                if(!string.IsNullOrEmpty(act.OnErrorWorkflow))
                {
                    results.Add(act.OnErrorWorkflow);
                }
            }

            return results;
        }

        #endregion

        #region Private Methods

        private static IEnumerable<string> InternalFindMissing<T>(IEnumerable<T> data)
        {
            IList<string> results = new List<string>();
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(T row in data)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                IEnumerable<PropertyInfo> properties = StringAttributeRefectionUtils.ExtractAdornedProperties<FindMissingAttribute>(row);
                foreach(PropertyInfo propertyInfo in properties)
                {
                    object property = propertyInfo.GetValue(row, null);
                    if(property != null)
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
