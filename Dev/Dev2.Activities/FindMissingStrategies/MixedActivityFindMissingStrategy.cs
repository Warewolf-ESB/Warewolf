using System;
using System.Collections.Generic;
using System.Reflection;
using Dev2.Enums;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.FindMissingStrategies
{
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
                    results.AddRange(DataSplitActivityPropertyExtraction(dsAct));             
                }
            }
            else if (activityType == typeof(DsfDataMergeActivity))
            {
                DsfDataMergeActivity dmAct = activity as DsfDataMergeActivity;
                if (dmAct != null)
                {
                    results.AddRange(DataMergeActivityPropertyExtraction(dmAct));
                }
            }
            return results;
        }        

        #endregion

        #region Private Methods

        private IList<string> DataSplitActivityPropertyExtraction(DsfDataSplitActivity dsAct)
        {
            IList<string> results = new List<string>();
            foreach (DataSplitDTO dataSplitDto in dsAct.ResultsCollection)
            {
                IEnumerable<PropertyInfo> properties = StringAttributeRefectionUtils.ExtractAdornedProperties<FindMissingAttribute>(dataSplitDto);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    object property = propertyInfo.GetValue(dataSplitDto, null);
                    if(property != null)
                    {
                        results.Add(property.ToString());    
                    }
                }
            }
            if (!string.IsNullOrEmpty(dsAct.SourceString))
            {
                results.Add(dsAct.SourceString);
            }
            return results;
        }

        private IList<string> DataMergeActivityPropertyExtraction(DsfDataMergeActivity dmAct)
        {
            IList<string> results = new List<string>();
            foreach (DataMergeDTO dataMergeDto in dmAct.MergeCollection)
            {
                IEnumerable<PropertyInfo> properties = StringAttributeRefectionUtils.ExtractAdornedProperties<FindMissingAttribute>(dataMergeDto);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    object property = propertyInfo.GetValue(dataMergeDto, null);
                    if (property != null)
                    {
                        results.Add(property.ToString());
                    }
                }
            }
            if (!string.IsNullOrEmpty(dmAct.Result))
            {
                results.Add(dmAct.Result);
            }
            return results;
        }

        #endregion
    }
}
