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
using System.Reflection;
using Dev2.Activities;
using Dev2.Activities.WcfEndPoint;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;



namespace Dev2.FindMissingStrategies
{
    /// <summary>
    /// Responsible for the find missing logic that applys to all the activities that only have a collection property on them
    /// </summary>
 //This is loaded based on SpookyAction implementing IFindMissingStrategy
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
                if (activity is DsfBaseConvertActivity bcAct)
                {
                    results.AddRange(InternalFindMissing(bcAct.ConvertCollection));
                }
            }
            else if (activityType == typeof(DsfCaseConvertActivity))
            {
                if (activity is DsfCaseConvertActivity ccAct)
                {
                    results.AddRange(InternalFindMissing(ccAct.ConvertCollection));
                }
            }
            else if (activityType == typeof(DsfMultiAssignActivity))
            {
                if (activity is DsfMultiAssignActivity maAct)
                {
                    results.AddRange(InternalFindMissing(maAct.FieldsCollection));
                }
            }
            else if (activityType == typeof(DsfMultiAssignObjectActivity))
            {
                if (activity is DsfMultiAssignObjectActivity maAct)
                {
                    results.AddRange(InternalFindMissing(maAct.FieldsCollection));
                }
            }
            else if (activityType == typeof(DsfGatherSystemInformationActivity))
            {
                if (activity is DsfGatherSystemInformationActivity maAct)
                {
                    results.AddRange(InternalFindMissing(maAct.SystemInformationCollection));
                }
            }
            else if (activityType == typeof(DsfSqlServerDatabaseActivity))
            {
                if (activity is DsfSqlServerDatabaseActivity maAct)
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
                if (activity is DsfMySqlDatabaseActivity maAct)
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
                if (activity is DsfPostgreSqlActivity maAct)
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
                if (activity is DsfOracleDatabaseActivity maAct)
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
                if (activity is DsfODBCDatabaseActivity maAct)
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
                if (activity is DsfWebPostActivity maAct)
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
                if (activity is DsfWebDeleteActivity maAct)
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
                if (activity is DsfWebPutActivity maAct)
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
                if (activity is DsfWebGetActivity maAct)
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
                if (activity is DsfDotNetDllActivity maAct)
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
                if (activity is DsfEnhancedDotNetDllActivity maAct)
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
                            if (pluginAction?.Inputs != null)
                            {
                                results.AddRange(InternalFindMissing(pluginAction.Inputs));
                            }
                            if (!string.IsNullOrEmpty(pluginAction?.OutputVariable))
                            {
                                results.Add(pluginAction.OutputVariable);
                            }
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
                if (activity is DsfComDllActivity maAct)
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
                if (activity is DsfWcfEndPointActivity maAct)
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
