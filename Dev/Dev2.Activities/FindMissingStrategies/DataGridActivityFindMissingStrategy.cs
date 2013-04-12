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
                    results.AddRange(BaseConvertActivityPropertyExtraction(bcAct));
                }
            }
            else if (activityType == typeof(DsfCaseConvertActivity))
            {
                DsfCaseConvertActivity ccAct = activity as DsfCaseConvertActivity;
                if (ccAct != null)
                {
                    results.AddRange(CaseConvertActivityPropertyExtraction(ccAct));
                }
            }
            else if (activityType == typeof(DsfMultiAssignActivity))
            {
                DsfMultiAssignActivity maAct = activity as DsfMultiAssignActivity;
                if (maAct != null)
                {
                    results.AddRange(MultiAssignActivityPropertyExtraction(maAct));
                }
            }
            return results;
        }

        #endregion

        #region Private Methods

        private IList<string> BaseConvertActivityPropertyExtraction(DsfBaseConvertActivity bcAct)
        {
            IList<string> results = new List<string>();
            foreach (BaseConvertTO baseConvertTO in bcAct.ConvertCollection)
            {
                IEnumerable<PropertyInfo> properties = StringAttributeRefectionUtils.ExtractAdornedProperties<FindMissingAttribute>(baseConvertTO);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    object property = propertyInfo.GetValue(baseConvertTO, null);
                    if (property != null)
                    {
                        results.Add(property.ToString());
                    }
                }
            }
            return results;
        }

        private IList<string> CaseConvertActivityPropertyExtraction(DsfCaseConvertActivity ccAct)
        {
            IList<string> results = new List<string>();
            foreach (CaseConvertTO caseConvertTO in ccAct.ConvertCollection)
            {
                IEnumerable<PropertyInfo> properties = StringAttributeRefectionUtils.ExtractAdornedProperties<FindMissingAttribute>(caseConvertTO);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    object property = propertyInfo.GetValue(caseConvertTO, null);
                    if (property != null)
                    {
                        results.Add(property.ToString());
                    }
                }
            }
            return results;
        }

        private IList<string> MultiAssignActivityPropertyExtraction(DsfMultiAssignActivity maAct)
        {
            IList<string> results = new List<string>();
            foreach (ActivityDTO activityDto in maAct.FieldsCollection)
            {
                IEnumerable<PropertyInfo> properties = StringAttributeRefectionUtils.ExtractAdornedProperties<FindMissingAttribute>(activityDto);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    object property = propertyInfo.GetValue(activityDto, null);
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
