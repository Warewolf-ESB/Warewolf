using System;
using System.Collections.Generic;
using System.Reflection;
using Dev2.Enums;
using Dev2.Factories;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.FindMissingStrategies
{
    public class ForEachActivityFindMissingStrategy : IFindMissingStrategy
    {
        #region Implementation of ISpookyLoadable<Enum>

        public Enum HandlesType()
        {
            return enFindMissingType.ForEach;
        }

        /// <summary>
        /// Gets all the fields for a specific activity
        /// </summary>
        /// <param name="activity">The activity that the fields will be retrieved from</param>
        /// <returns>Returns all the fields in a list of strings</returns>
        public List<string> GetActivityFields(object activity)
        {
            List<string> results = new List<string>();
            Dev2FindMissingStrategyFactory stratFac = new Dev2FindMissingStrategyFactory();
            DsfForEachActivity forEachActivity = activity as DsfForEachActivity;
            if(forEachActivity != null)
            {
                IFindMissingStrategy strategy;
                enFindMissingType findMissingType;
                var boolAct = forEachActivity.DataFunc.Handler as DsfNativeActivity<bool>;
                if(boolAct == null)
                {
                    DsfNativeActivity<string> stringAct = forEachActivity.DataFunc.Handler as DsfNativeActivity<string>;
                    if(stringAct != null)
                    {
                        findMissingType = stringAct.GetFindMissingType();
                        strategy = stratFac.CreateFindMissingStrategy(findMissingType);
                        results.AddRange(strategy.GetActivityFields(stringAct));
                    }
                }
                else
                {
                    findMissingType = boolAct.GetFindMissingType();
                    strategy = stratFac.CreateFindMissingStrategy(findMissingType);
                    results.AddRange(strategy.GetActivityFields(boolAct));
                }
            }

            IEnumerable<PropertyInfo> properties = StringAttributeRefectionUtils.ExtractAdornedProperties<FindMissingAttribute>(activity);
            foreach(PropertyInfo propertyInfo in properties)
            {
                object property = propertyInfo.GetValue(activity, null);
                if(property != null)
                {
                    results.Add(property.ToString());
                }
            }

            return results;
        }

        #endregion
    }
}
