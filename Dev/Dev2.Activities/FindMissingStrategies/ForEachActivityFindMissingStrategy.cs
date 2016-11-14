/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Activities.SelectAndApply;
using Dev2.Factories;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;
// ReSharper disable LoopCanBeConvertedToQuery

namespace Dev2.FindMissingStrategies
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")] //This is loaded based on SpookyAction implementing IFindMissingStrategy
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

            DsfSelectAndApplyActivity selectAndApply = activity as DsfSelectAndApplyActivity;
            if(selectAndApply != null)
            {
                IFindMissingStrategy strategy;
                enFindMissingType findMissingType;
                var boolAct = selectAndApply.ApplyActivityFunc.Handler as DsfNativeActivity<bool>;
                if(boolAct == null)
                {
                    DsfNativeActivity<string> stringAct = selectAndApply.ApplyActivityFunc.Handler as DsfNativeActivity<string>;
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
