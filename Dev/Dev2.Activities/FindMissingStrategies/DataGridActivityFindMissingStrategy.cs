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
using Dev2.Activities.WcfEndPoint;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;
// ReSharper disable ReturnTypeCanBeEnumerable.Local
// ReSharper disable LoopCanBeConvertedToQuery

namespace Dev2.FindMissingStrategies
{
    /// <summary>
    /// Responsible for the find missing logic that applys to all the activities that only have a collection property on them
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")] //This is loaded based on SpookyAction implementing IFindMissingStrategy
    public class DataGridActivityFindMissingStrategy : IFindMissingStrategy
    {

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
            else if (activityType == typeof(DsfMultiAssignObjectActivity))
            {
                DsfMultiAssignObjectActivity maAct = activity as DsfMultiAssignObjectActivity;
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
            else if (activityType == typeof(DsfSqlServerDatabaseActivity))
            {
                var maAct = activity as DsfSqlServerDatabaseActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }
                    if (maAct.Outputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Outputs));
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }

                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            else if (activityType == typeof(DsfMySqlDatabaseActivity))
            {
                var maAct = activity as DsfMySqlDatabaseActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }
                    if (maAct.Outputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Outputs));
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }

                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            else if (activityType == typeof(DsfPostgreSqlActivity))
            {
                var maAct = activity as DsfPostgreSqlActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }
                    if (maAct.Outputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Outputs));
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }

                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            else if (activityType == typeof(DsfOracleDatabaseActivity))
            {
                var maAct = activity as DsfOracleDatabaseActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }
                    if (maAct.Outputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Outputs));
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }

                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            else if (activityType == typeof(DsfODBCDatabaseActivity))
            {
                var maAct = activity as DsfODBCDatabaseActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }
                    if (maAct.CommandText != null)
                    {
                        results.Add(maAct.CommandText);
                    }
                    if (maAct.Outputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Outputs));
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }

                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            else if (activityType == typeof(DsfWebPostActivity))
            {
                var maAct = activity as DsfWebPostActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }

                    if (maAct.QueryString != null)
                    {
                        results.Add(maAct.QueryString);
                    }
                    if (maAct.PostData != null)
                    {
                        results.Add(maAct.PostData);
                    }
                    if (maAct.Headers != null)
                    {
                        foreach (var nameValue in maAct.Headers)
                        {
                            results.Add(nameValue.Name);
                            results.Add(nameValue.Value);
                        }
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                    if (maAct.IsObject)
                    {
                        if (!string.IsNullOrEmpty(maAct.ObjectName))
                        {
                            results.Add(maAct.ObjectName);
                        }
                    }
                    else
                    {

                        if (maAct.Outputs != null)
                        {
                            results.AddRange(InternalFindMissing(maAct.Outputs));
                        }
                    }
                }
            }
            else if (activityType == typeof(DsfWebDeleteActivity))
            {
                var maAct = activity as DsfWebDeleteActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }
                    if (maAct.QueryString != null)
                    {
                        results.Add(maAct.QueryString);
                    }
                    if (maAct.Headers != null)
                    {
                        foreach (var nameValue in maAct.Headers)
                        {
                            results.Add(nameValue.Name);
                            results.Add(nameValue.Value);
                        }
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }
                    if (maAct.IsObject)
                    {
                        if (!string.IsNullOrEmpty(maAct.ObjectName))
                        {
                            results.Add(maAct.ObjectName);
                        }
                    }
                    else
                    {

                        if (maAct.Outputs != null)
                        {
                            results.AddRange(InternalFindMissing(maAct.Outputs));
                        }
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            else if (activityType == typeof(DsfWebPutActivity))
            {
                var maAct = activity as DsfWebPutActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }
                    if (maAct.QueryString != null)
                    {
                        results.Add(maAct.QueryString);
                    }
                    if (maAct.PutData != null)
                    {
                        results.Add(maAct.PutData);
                    }
                    if (maAct.Headers != null)
                    {
                        foreach (var nameValue in maAct.Headers)
                        {
                            results.Add(nameValue.Name);
                            results.Add(nameValue.Value);
                        }
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }
                    if (maAct.IsObject)
                    {
                        if (!string.IsNullOrEmpty(maAct.ObjectName))
                        {
                            results.Add(maAct.ObjectName);
                        }
                    }
                    else
                    {

                        if (maAct.Outputs != null)
                        {
                            results.AddRange(InternalFindMissing(maAct.Outputs));
                        }
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            else if (activityType == typeof(DsfWebGetActivity))
            {
                var maAct = activity as DsfWebGetActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }
                    if (maAct.QueryString != null)
                    {
                        results.Add(maAct.QueryString);
                    }
                    if (maAct.Headers != null)
                    {
                        foreach (var nameValue in maAct.Headers)
                        {
                            results.Add(nameValue.Name);
                            results.Add(nameValue.Value);
                        }
                    }
                    if (!string.IsNullOrEmpty(maAct.ObjectName))
                    {
                        results.Add(maAct.ObjectName);
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }
                    if (maAct.IsObject)
                    {
                        if (!string.IsNullOrEmpty(maAct.ObjectName))
                        {
                            results.Add(maAct.ObjectName);
                        }
                    }
                    else
                    {

                        if (maAct.Outputs != null)
                        {
                            results.AddRange(InternalFindMissing(maAct.Outputs));
                        }
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            else if (activityType == typeof(DsfDotNetDllActivity))
            {
                var maAct = activity as DsfDotNetDllActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }
                    if (maAct.IsObject)
                    {
                        if (!string.IsNullOrEmpty(maAct.ObjectName))
                        {
                            results.Add(maAct.ObjectName);
                        }
                    }
                    else
                    {

                        if (maAct.Outputs != null)
                        {
                            results.AddRange(InternalFindMissing(maAct.Outputs));
                        }
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            else if (activityType == typeof(DsfEnhancedDotNetDllActivity))
            {
                var maAct = activity as DsfEnhancedDotNetDllActivity;
                if (maAct != null)
                {
                    if (maAct.ConstructorInputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.ConstructorInputs));
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }

                    if (maAct.MethodsToRun != null)
                    {
                        foreach (var pluginAction in maAct.MethodsToRun)
                        {
                            if(pluginAction?.Inputs != null)
                            {
                                results.AddRange(InternalFindMissing(pluginAction.Inputs));
                            }
                            if (!string.IsNullOrEmpty(pluginAction?.OutputVariable))
                                results.Add(pluginAction.OutputVariable);
                        }
                    }
                    if (maAct.IsObject)
                    {
                        if (!string.IsNullOrEmpty(maAct.ObjectName))
                        {
                            results.Add(maAct.ObjectName);
                        }
                    }
                    else
                    {

                        if (maAct.Outputs != null)
                        {
                            results.AddRange(InternalFindMissing(maAct.Outputs));
                        }
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            else if (activityType == typeof(DsfComDllActivity))
            {
                var maAct = activity as DsfComDllActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }
                    if (maAct.IsObject)
                    {
                        if (!string.IsNullOrEmpty(maAct.ObjectName))
                        {
                            results.Add(maAct.ObjectName);
                        }
                    }
                    else
                    {

                        if (maAct.Outputs != null)
                        {
                            results.AddRange(InternalFindMissing(maAct.Outputs));
                        }
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            else if (activityType == typeof(DsfWcfEndPointActivity))
            {
                var maAct = activity as DsfWcfEndPointActivity;
                if (maAct != null)
                {
                    if (maAct.Inputs != null)
                    {
                        results.AddRange(InternalFindMissing(maAct.Inputs));
                    }

                    if (!string.IsNullOrEmpty(maAct.OnErrorVariable))
                    {
                        results.Add(maAct.OnErrorVariable);
                    }
                    if (maAct.IsObject)
                    {
                        if (!string.IsNullOrEmpty(maAct.ObjectName))
                        {
                            results.Add(maAct.ObjectName);
                        }
                    }
                    else
                    {

                        if (maAct.Outputs != null)
                        {
                            results.AddRange(InternalFindMissing(maAct.Outputs));
                        }
                    }
                    if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                    {
                        results.Add(maAct.OnErrorWorkflow);
                    }
                }
            }
            return results;
        }


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
    }
}
