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
using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ServiceModel.Data
{
    public static class RecordsetListHelper
    {
        #region ToRecordsetList

        public static RecordsetList ToRecordsetList(this IOutputDescription outputDescription) => outputDescription.ToRecordsetList(null, "");

        public static RecordsetList ToRecordsetList(this IOutputDescription outputDescription, RecordsetList currentList) => outputDescription.ToRecordsetList(currentList, "");

        public static RecordsetList ToRecordsetList(this IOutputDescription outputDescription, RecordsetList currentList, string defaultFieldName)
        {
            if(outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
            {
                throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
            }

            var result = currentList ?? new RecordsetList();
            var currentFields = new List<RecordsetField>();

            #region Create a copy of the current list's fields so that we don't lose the user-defined aliases.

            foreach(var rs in result)
            {
                currentFields.AddRange(rs.Fields);
                rs.Fields.Clear();
            }

            #endregion

            var paths = outputDescription.DataSourceShapes[0].Paths;

            foreach(var path in paths)
            {
                var names = SplitRecordsetAndFieldNames(path);
                var rsName = names.Item1;
                var rsAlias = rsName;
                var fieldName = names.Item2;
                if(string.IsNullOrEmpty(fieldName) && string.IsNullOrEmpty(defaultFieldName))
                {
                    continue;
                }

                if(string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(defaultFieldName))
                {
                    fieldName = defaultFieldName;
                }

                if(fieldName != null)
                {
                    var fieldAlias = fieldName.Replace(":", "");

                    var pathLoop = path;
                    var rsField = currentFields.FirstOrDefault(f => f.Path == pathLoop) ?? new RecordsetField { Path = path, Alias = fieldAlias, RecordsetAlias = rsAlias };
                    rsField.Name = fieldName;

                    var rs = result.FirstOrDefault(r => r.Name == rsName);
                    if(rs == null)
                    {
                        rs = new Recordset { Name = rsName };
                        result.Add(rs);
                    }
                    var fieldIndex = rs.Fields.Count;
                    rs.Fields.Add(rsField);

                    var data = path.SampleData.Split(',');
                    for(var recordIndex = 0; recordIndex < data.Length; recordIndex++)
                    {
                        rs.SetValue(recordIndex, fieldIndex, data[recordIndex]);
                    }
                }
            }
            result.Description = outputDescription;
            return result;
        }

        #endregion

        #region ParseRecordsetAndFieldNames

        public static Tuple<string, string> SplitRecordsetAndFieldNames(IPath path)
        {
            var rsName = string.Empty;
            var fieldName = path.ActualPath.Replace(".", "");

            var indexOf = path.ActualPath.LastIndexOf("()", StringComparison.InvariantCultureIgnoreCase);
            if(indexOf != -1)
            {
                var length = path.ActualPath.Length;
                if (indexOf + 2 == length) // This means we have a primitive array as property
                {
                    var upperRecsetName = path.ActualPath.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                    if(upperRecsetName == -1)
                    {
                        rsName = path.ActualPath.Substring(0, indexOf + 2).Replace("()", "").Replace(".", "_");
                    }
                    else
                    {
                        rsName = path.ActualPath.Substring(0, upperRecsetName).Replace("()", "").Replace(".", "_");
                        fieldName = path.ActualPath.Substring(upperRecsetName).Replace(".", "").Replace("()", "");
                    }
                }
                else
                {
                    rsName = path.ActualPath.Substring(0, indexOf + 2).Replace("()", "").Replace(".", "_");
                    fieldName = path.ActualPath.Substring(indexOf + 2).Replace(".", "");
                }
            }
            return new Tuple<string, string>(rsName, fieldName);
        }

        #endregion

    }
}
