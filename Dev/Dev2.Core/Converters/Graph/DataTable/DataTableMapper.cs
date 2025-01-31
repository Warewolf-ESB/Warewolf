#pragma warning disable
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
using System.Data;
using System.Globalization;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;

namespace Dev2.Converters.Graph.DataTable
{
    public class DataTableMapper : IMapper
    {
        public IEnumerable<IPath> Map(object data)
        {
            VerifyArgument.IsNotNull("data", data);


            if (data is System.Data.DataTable tmp)
            {
                var tblName = tmp.TableName;

                var cols = tmp.Columns;

                var result = new List<IPath>();


                foreach (DataColumn col in cols)

                {
                    var colName = col.ColumnName;
                    result.Add(new DataTablePath(tblName, colName));
                }

                BuildSampleData(tmp, cols.Count, ref result);

                return result;
            }

            return null;
        }

        /// <summary>
        ///     Builds the sample data.
        /// </summary>
        /// <param name="tmp">The temporary.</param>
        /// <param name="totalCols">The total cols.</param>
        /// <param name="result">The result.</param>
        void BuildSampleData(System.Data.DataTable tmp, int totalCols, ref List<IPath> result)
        {
            var totalRows = tmp.Rows.Count - 1;
           
            // now set sample data ;)


            for (int i = 0; i < totalCols; i++)
            {
                var rowCnt = 0;
                for (int rowIdx = 0; rowIdx < totalRows; rowIdx++)
                {
                    var columnDataType = tmp.Columns[i].DataType;
                    var isNumericType = IsNumericType(columnDataType);
                    var itemData = isNumericType ? tmp.Rows[rowIdx].ItemArray[i] : tmp.Rows[rowIdx].ItemArray[i];

                    var newVals = ToInvariantString(itemData);

                    result[i].SampleData += isNumericType ? newVals : String.Concat("'", newVals, "'");

                    if (rowIdx < totalRows - 1)
                    {
                        result[i].SampleData += GlobalConstants.AnytingToXmlCommaToken;
                    }

                    rowCnt++;

                    // exit after 10 rows ;)
                    if (rowCnt == 10)
                    {
                        break;
                    }
                }
            }

            // exit after 10 columns ;)
            // Since the row count is handled inside the inner loop, we don't need the outer row loop exit condition anymore.



            //foreach (DataRow row in tmp.Rows)
            //{
            //    for (int i = 0; i < totalCols; i++)
            //    {
            //        var columnDataType = tmp.Columns[i].DataType;
            //        var isNumericType = IsNumericType(columnDataType);
            //        var itemData = row.ItemArray[i].ToString();
            //        result[i].SampleData += itemData;
            //        if (rowCnt < totalRows)
            //        {
            //            result[i].SampleData += GlobalConstants.AnytingToXmlCommaToken;
            //        }
            //    }

            //    rowCnt++;

            //    // exit after 10 rows ;)
            //    if (rowCnt == 10)
            //    {
            //        break;
            //    }
            //}
        }

        public static bool IsNumericType(Type type)
        {
            // Nullable types should be unwrapped
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // Check against all numeric types
            return underlyingType == typeof(byte) ||
                   underlyingType == typeof(sbyte) ||
                   underlyingType == typeof(short) ||
                   underlyingType == typeof(ushort) ||
                   underlyingType == typeof(int) ||
                   underlyingType == typeof(uint) ||
                   underlyingType == typeof(long) ||
                   underlyingType == typeof(ulong) ||
                   underlyingType == typeof(float) ||
                   underlyingType == typeof(double) ||
                   underlyingType == typeof(decimal);
        }

        public static string ToInvariantString(object input)
        {
            if (input == null)
                return "";

            // Handle DateTime and DateTimeOffset
            if (input is DateTime dateTime)
            {
                return dateTime.ToString("o", CultureInfo.InvariantCulture); // ISO 8601 format
            }
            if (input is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.ToString("o", CultureInfo.InvariantCulture); // ISO 8601 format
            }

            // Handle numeric types
            if (input is IConvertible convertible)
            {
                return convertible.ToString(CultureInfo.InvariantCulture);
            }

            // Handle strings (return as-is since strings are culture-independent)
            if (input is string str)
            {
                return str;
            }

            // For other types, use ToString with invariant culture
            return Convert.ToString(input, CultureInfo.InvariantCulture)
                   ?? "";
        }
    }
}