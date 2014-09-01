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

            if(tmp != null)
            {
                var tblName = tmp.TableName;

                DataColumnCollection cols = tmp.Columns;

                List<IPath> result = new List<IPath>();

                // ReSharper disable LoopCanBeConvertedToQuery
                foreach(DataColumn col in cols)
                // ReSharper restore LoopCanBeConvertedToQuery
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
        /// Builds the sample data.
        /// </summary>
        /// <param name="tmp">The temporary.</param>
        /// <param name="totalCols">The total cols.</param>
        /// <param name="result">The result.</param>
        private void BuildSampleData(System.Data.DataTable tmp, int totalCols, ref List<IPath> result)
        {
            int totalRows = tmp.Rows.Count - 1;
            int rowCnt = 0;
            // now set sample data ;)

            foreach(DataRow row in tmp.Rows)
            {
                for(int i = 0; i < totalCols; i++)
                {
                    var itemData = row.ItemArray[i].ToString();
                    result[i].SampleData += itemData;
                    if(rowCnt < totalRows)
                    {
                        result[i].SampleData += GlobalConstants.AnytingToXmlCommaToken;
                    }
                }

                rowCnt++;

                // exit after 10 rows ;)
                if(rowCnt == 10)
                {
                    break;
                }
            }

        }
    }
}
