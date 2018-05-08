/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
    public class DataGridActivityFindMissingStrategy : IFindMissingStrategy
    {
        public Enum HandlesType() => enFindMissingType.DataGridActivity;
        
        public List<string> GetActivityFields(object activity)
        {
            // TODO: refactor into all activities havnig a GetActivityFields() method
            var activityType = activity.GetType();
            if (activityType == typeof(DsfBaseConvertActivity))
            {
                return GetDsfBaseConvertActivityFields(activity);
            }
            else if (activityType == typeof(DsfCaseConvertActivity))
            {
                return GetDsfCaseConvertActivityFields(activity);
            }
            else if (activityType == typeof(DsfMultiAssignActivity))
            {
                return GetDsfMultiAssignActivityFields(activity);
            }
            else if (activityType == typeof(DsfMultiAssignObjectActivity))
            {
                return GetDsfMultiAssignObjectActivityFields(activity);
            }
            else if (activityType == typeof(DsfDotNetMultiAssignActivity))
            {
                return GetDsfDotNetMultiAssignActivityFields(activity);
            }
            else if (activityType == typeof(DsfDotNetMultiAssignObjectActivity))
            {
                return GetDsfDotNetMultiAssignObjectActivityFields(activity);
            }
            else if (activityType == typeof(DsfGatherSystemInformationActivity))
            {
                return GetDsfGatherSystemInformationActivityFields(activity);
            }
            else if (activityType == typeof(DsfDotNetGatherSystemInformationActivity))
            {
                return GetDsfDotNetGatherSystemInformationActivityFields(activity);
            }
            else if (activityType == typeof(DsfSqlServerDatabaseActivity))
            {
                return GetDsfSqlServerDatabaseActivityFields(activity);
            }
            else if (activityType == typeof(AdvancedRecordsetActivity))
            {
                return GetDsfAdvancedRecordsetActivity(activity);
            }
            else if (activityType == typeof(DsfMySqlDatabaseActivity))
            {
                return GetDsfMySqlDatabaseActivityFields(activity);
            }
            else if (activityType == typeof(DsfPostgreSqlActivity))
            {
                return GetDsfPostgreSqlActivityFields(activity);
            }
            else if (activityType == typeof(DsfOracleDatabaseActivity))
            {
                return GetDsfOracleDatabaseActivityFields(activity);
            }
            else if (activityType == typeof(DsfODBCDatabaseActivity))
            {
                return GetDsfODBCDatabaseActivityFields(activity);
            }
            else if (activityType == typeof(DsfWebPostActivity))
            {
                return GetDsfWebPostActivityFields(activity);
            }
            else if (activityType == typeof(DsfWebDeleteActivity))
            {
                return GetDsfWebDeleteActivityFields(activity);
            }
            else if (activityType == typeof(DsfWebPutActivity))
            {
                return GetDsfWebPutActivityFields(activity);
            }
            else if (activityType == typeof(DsfWebGetActivity))
            {
                return GetDsfWebGetActivityFields(activity);
            }
            else if (activityType == typeof(DsfDotNetDllActivity))
            {
                return GetDsfDotNetDllActivityFields(activity);
            }
            else if (activityType == typeof(DsfEnhancedDotNetDllActivity))
            {
                return GetDsfEnhancedDotNetDllActivityFields(activity);
            }
            else if (activityType == typeof(DsfComDllActivity))
            {
                return GetDsfComDllActivityFields(activity);
            }
            else if (activityType == typeof(DsfWcfEndPointActivity))
            {
                return GetDsfWcfEndPointActivityFields(activity);
            }
            else
            {
                return new List<string>();
            }
        }

        List<string> GetDsfWcfEndPointActivityFields(object activity)
        {
            var results = new List<string>();
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
            return results;
        }

        List<string> GetDsfComDllActivityFields(object activity)
        {
            var results = new List<string>();
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
            return results;
        }

        List<string> GetDsfEnhancedDotNetDllActivityFields(object activity)
        {
            var results = new List<string>();
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
                        AddMethodsToRun(results, pluginAction);
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
            return results;
        }

        private void AddMethodsToRun(List<string> results, Common.Interfaces.IPluginAction pluginAction)
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

        List<string> GetDsfDotNetDllActivityFields(object activity)
        {
            var results = new List<string>();
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
            return results;
        }

        List<string> GetDsfWebGetActivityFields(object activity)
        {
            var results = new List<string>();
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
            return results;
        }

        List<string> GetDsfWebPutActivityFields(object activity)
        {
            var results = new List<string>();
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
            return results;
        }

        List<string> GetDsfWebDeleteActivityFields(object activity)
        {
            var results = new List<string>();
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
            return results;
        }

        private List<string> GetDsfWebPostActivityFields(object activity)
        {
            var results = new List<string>();
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
                    results.AddRange(AddAllHeaders(maAct));
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
            return results;
        }

        List<string> GetDsfODBCDatabaseActivityFields(object activity)
        {
            var results = new List<string>();
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
            return results;
        }

        List<string> GetDsfOracleDatabaseActivityFields(object activity)
        {
            var results = new List<string>();
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
            return results;
        }

        List<string> GetDsfPostgreSqlActivityFields(object activity)
        {
            var results = new List<string>();
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
            return results;
        }

        List<string> GetDsfAdvancedRecordsetActivity(object activity) {
            var results = new List<string>();
            if (activity is AdvancedRecordsetActivity maAct)
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
                if(maAct.DeclareVariables != null)
                {
                    foreach(var item in maAct.DeclareVariables)
                    {
                        results.Add(item.Value);
                    }
                }
                if (!string.IsNullOrEmpty(maAct.OnErrorWorkflow))
                {
                    results.Add(maAct.OnErrorWorkflow);
                }
            }
            return results;
        }

        List<string> GetDsfMySqlDatabaseActivityFields(object activity)
        {
            var results = new List<string>();
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
            return results;
        }

        List<string> GetDsfSqlServerDatabaseActivityFields(object activity)
        {
            var results = new List<string>();
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
            return results;
        }

        List<string> GetDsfDotNetGatherSystemInformationActivityFields(object activity)
        {
            var results = new List<string>();
            if (activity is DsfDotNetGatherSystemInformationActivity maAct)
            {
                results.AddRange(InternalFindMissing(maAct.SystemInformationCollection));
            }
            return results;
        }

        List<string> GetDsfGatherSystemInformationActivityFields(object activity)
        {
            var results = new List<string>();
            if (activity is DsfGatherSystemInformationActivity maAct)
            {
                results.AddRange(InternalFindMissing(maAct.SystemInformationCollection));
            }
            return results;
        }

        List<string> GetDsfDotNetMultiAssignObjectActivityFields(object activity)
        {
            var results = new List<string>();
            if (activity is DsfDotNetMultiAssignObjectActivity maAct)
            {
                results.AddRange(InternalFindMissing(maAct.FieldsCollection));
            }
            return results;
        }

        List<string> GetDsfDotNetMultiAssignActivityFields(object activity)
        {
            var results = new List<string>();
            if (activity is DsfDotNetMultiAssignActivity maAct)
            {
                results.AddRange(InternalFindMissing(maAct.FieldsCollection));
            }
            return results;
        }

        List<string> GetDsfMultiAssignObjectActivityFields(object activity)
        {
            var results = new List<string>();
            if (activity is DsfMultiAssignObjectActivity maAct)
            {
                results.AddRange(InternalFindMissing(maAct.FieldsCollection));
            }
            return results;
        }

        List<string> GetDsfMultiAssignActivityFields(object activity)
        {
            var results = new List<string>();
            if (activity is DsfMultiAssignActivity maAct)
            {
                results.AddRange(InternalFindMissing(maAct.FieldsCollection));
            }
            return results;
        }

        List<string> GetDsfCaseConvertActivityFields(object activity)
        {
            var results = new List<string>();
            if (activity is DsfCaseConvertActivity ccAct)
            {
                results.AddRange(InternalFindMissing(ccAct.ConvertCollection));
            }
            return results;
        }

        List<string> GetDsfBaseConvertActivityFields(object activity)
        {
            var results = new List<string>();
            if (activity is DsfBaseConvertActivity bcAct)
            {
                results.AddRange(InternalFindMissing(bcAct.ConvertCollection));
            }
            return results;
        }

        static List<string> AddAllHeaders(DsfWebPostActivity maAct)
        {
            var results = new List<string>();
            foreach (var nameValue in maAct.Headers)
            {
                results.Add(nameValue.Name);
                results.Add(nameValue.Value);
            }
            return results;
        }

        IList<string> InternalFindMissing<T>(IEnumerable<T> data)
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
    }
}
