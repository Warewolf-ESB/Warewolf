/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Data;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;

namespace Dev2.Converters.Graph.DataTable
{
    public class DataTableMapper : IMapper
    {
        public IEnumerable<IPath> Map(object data)
        {
            VerifyArgument.IsNotNull("data", data);

            var tmp = data as System.Data.DataTable;

            if (tmp != null)
            {
                string tblName = tmp.TableName;

                DataColumnCollection cols = tmp.Columns;

                var result = new List<IPath>();

                // ReSharper disable LoopCanBeConvertedToQuery
                foreach (DataColumn col in cols)
                    // ReSharper restore LoopCanBeConvertedToQuery
                {
                    string colName = col.ColumnName;
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
        private void BuildSampleData(System.Data.DataTable tmp, int totalCols, ref List<IPath> result)
        {
            int totalRows = tmp.Rows.Count - 1;
            int rowCnt = 0;
            // now set sample data ;)

            foreach (DataRow row in tmp.Rows)
            {
                for (int i = 0; i < totalCols; i++)
                {
                    string itemData = row.ItemArray[i].ToString();
                    result[i].SampleData += itemData;
                    if (rowCnt < totalRows)
                    {
                        result[i].SampleData += GlobalConstants.AnytingToXmlCommaToken;
                    }
                }

                rowCnt++;

                // exit after 10 rows ;)
                if (rowCnt == 10)
                {
                    break;
                }
            }
        }
    }
}