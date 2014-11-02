/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Runtime.ServiceModel.Data
{
    // BUG 9626 - 2013.06.11 - TWR : refactored
    public static class RecordsetListHelper
    {
        #region ToRecordsetList

        public static RecordsetList ToRecordsetList(this IOutputDescription outputDescription,
            RecordsetList currentList = null, string defaultFieldName = "")
        {
            if (outputDescription == null || outputDescription.DataSourceShapes == null ||
                outputDescription.DataSourceShapes.Count == 0)
            {
                throw new Exception("Error retrieving shape from service output.");
            }

            RecordsetList result = currentList ?? new RecordsetList();
            var currentFields = new List<RecordsetField>();

            #region Create a copy of the current list's fields so that we don't lose the user-defined aliases.

            foreach (Recordset rs in result)
            {
                currentFields.AddRange(rs.Fields);
                rs.Fields.Clear();
            }

            #endregion

            List<IPath> paths = outputDescription.DataSourceShapes[0].Paths;

            foreach (IPath path in paths)
            {
                Tuple<string, string> names = SplitRecordsetAndFieldNames(path);
                string rsName = names.Item1;
                string rsAlias = rsName;
                string fieldName = names.Item2;
                if (string.IsNullOrEmpty(fieldName) && string.IsNullOrEmpty(defaultFieldName))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(defaultFieldName))
                {
                    fieldName = defaultFieldName;
                }

                // Bug 10532 - Amend to remove : from the alias ;)
                string fieldAlias = fieldName.Replace(":", "");

                IPath pathLoop = path;
                RecordsetField rsField = currentFields.FirstOrDefault(f => f.Path == pathLoop) ??
                                         new RecordsetField {Path = path, Alias = fieldAlias, RecordsetAlias = rsAlias};
                rsField.Name = fieldName;

                Recordset rs = result.FirstOrDefault(r => r.Name == rsName);
                if (rs == null)
                {
                    rs = new Recordset {Name = rsName};
                    result.Add(rs);
                }
                int fieldIndex = rs.Fields.Count;
                rs.Fields.Add(rsField);

                string[] data = path.SampleData.Split(',');
                for (int recordIndex = 0; recordIndex < data.Length; recordIndex++)
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
            string rsName = string.Empty;
            string fieldName = path.ActualPath.Replace(".", "");

            int indexOf = path.ActualPath.LastIndexOf("()", StringComparison.InvariantCultureIgnoreCase);
            if (indexOf != -1)
            {
                int length = path.ActualPath.Length;
                if (indexOf + 2 == length) // This means we have a primitive array as property
                {
                    int upperRecsetName = path.ActualPath.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                    if (upperRecsetName == -1)
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