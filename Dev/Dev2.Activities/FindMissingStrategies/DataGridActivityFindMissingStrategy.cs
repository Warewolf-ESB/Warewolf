#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        public List<string> GetActivityFields(object activity)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
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

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        List<string> GetDsfEnhancedDotNetDllActivityFields(object activity)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
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

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        List<string> GetDsfWebGetActivityFields(object activity)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
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

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        List<string> GetDsfWebPutActivityFields(object activity)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
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

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        List<string> GetDsfWebDeleteActivityFields(object activity)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
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

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        private List<string> GetDsfWebPostActivityFields(object activity)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
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
