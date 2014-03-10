using System;
using System.Collections.Generic;
using System.Reflection;
using Dev2.Activities;
using Dev2.Enums;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.FindMissingStrategies
{
    /// <summary>
    /// Responsible for the find missing logic that applys to all the activities that have a collection property and some static properties on them
    /// </summary>
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

            if (activityType == typeof(DsfDataSplitActivity))
            {
                DsfDataSplitActivity dsAct = activity as DsfDataSplitActivity;
                if (dsAct != null)
                {
                    results.AddRange(InternalFindMissing(dsAct.ResultsCollection));
                    if (!string.IsNullOrEmpty(dsAct.SourceString))
                    {
                        results.Add(dsAct.SourceString);
                    }
                }
            }
            else if (activityType == typeof(DsfDataMergeActivity))
            {
                DsfDataMergeActivity dmAct = activity as DsfDataMergeActivity;
                if (dmAct != null)
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
                DsfXPathActivity xpAct = activity as DsfXPathActivity;
                if (xpAct != null)
                {
                    results.AddRange(InternalFindMissing(xpAct.ResultsCollection));
                    if (!string.IsNullOrEmpty(xpAct.SourceString))
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
