using System;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    // BUG 9626 - 2013.06.11 - TWR : refactored
    public static class RecordsetListHelper
    {
        #region ToRecordsetList

        public static RecordsetList ToRecordsetList(this IOutputDescription outputDescription, RecordsetList currentList = null, string defaultFieldName = "")
        {
            if(outputDescription == null || outputDescription.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
            {
                throw new Exception("Error retrieving shape from service output.");
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

                // Bug 10532 - Amend to remove : from the alias ;)
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
                int length = path.ActualPath.Length;
                if(indexOf + 2 == length) // This means we have a primitive array as property
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
