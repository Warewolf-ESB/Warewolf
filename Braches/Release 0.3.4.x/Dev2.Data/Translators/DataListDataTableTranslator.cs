using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;

namespace Dev2.Data.Translators
{
    public class DataListDataTableTranslator : IDataListTranslator
    {

        public DataListDataTableTranslator()
        {
            Format = DataListFormat.CreateFormat(GlobalConstants._DATATABLE);
            TextEncoding = Encoding.UTF8;
        }

        public DataListFormat HandlesType()
        {
            return Format;
        }

        public DataListFormat Format { get; private set; }
        public Encoding TextEncoding { get; private set; }

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList input, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public IBinaryDataList ConvertTo(byte[] input, string shape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public string ConvertAndFilter(IBinaryDataList input, string filterShape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }


        public IBinaryDataList ConvertTo(object input, string shape, out ErrorResultTO errors)
        {
            string error;
            errors = new ErrorResultTO();
            IBinaryDataList targetDL = DataListTranslatorHelper.BuildTargetShape(shape, out error);
            errors.AddError(error);

            DataTable dbData = (input as DataTable);

            var rs = targetDL.FetchAllUserKeys();

            // ensure we only have a single recordset to map too ;)
            if (rs != null && rs.Count != 1)
            {
                throw new Exception("DataTable translator can only map to a single recordset!");    
            }

            var rsName = rs.FirstOrDefault();

            if (dbData != null)
            {
                IBinaryDataListEntry entry;

                // build up the columns ;)
                if (targetDL.TryGetEntry(rsName, out entry, out error))
                {
                    
                    //if (!entry.IsRecordset)
                    //{
                    //    throw new Exception("DataTable translator cannot map to scalar values!");    
                    //}

                    if (entry.IsRecordset)
                    {
                        var cols = entry.Columns;
                        IDictionary<int, string> colMapping = BuildColumnNameToIndexMap(entry.Columns, dbData.Columns);

                        // now convert to binary datalist ;)
                        int rowIdx = 1;
                        foreach (DataRow row in dbData.Rows)
                        {
                            IList<IBinaryDataListItem> items = new List<IBinaryDataListItem>(cols.Count);
                            // build up the row
                            int idx = 0;

                            foreach (var item in row.ItemArray)
                            {
                                string colName;

                                if (colMapping.TryGetValue(idx, out colName))
                                {
                                    items.Add(new BinaryDataListItem(item.ToString(), rsName, colName, rowIdx));
                                }

                                idx++;
                            }

                            // add the row ;)
                            entry.TryPutRecordRowAt(items, rowIdx, out error);
                            errors.AddError(error);
                            rowIdx++;
                        }
                    }
                    else
                    {
                        // handle a scalar coming out ;)
                        if (dbData.Rows != null && dbData.Rows.Count == 1)
                        {
                            var row = dbData.Rows[0].ItemArray;
                            // Look up the correct index from the columns ;)

                            int pos = 0;
                            var cols = dbData.Columns;
                            int idx = -1;
                            
                            while (pos < cols.Count && idx == -1)
                            {
                                if (cols[pos].ColumnName == entry.Namespace)
                                {
                                    idx = pos;
                                }
                                pos++;
                            }

                            entry.TryPutScalar(new BinaryDataListItem(row[idx].ToString(), entry.Namespace), out error);
                            errors.AddError(error);

                        }
                    }

                }
                else
                {
                    errors.AddError(error);
                }
            }
            return targetDL;
        }


        /// <summary>
        /// Builds the column name to index map.
        /// </summary>
        /// <param name="dlCols">The dl cols.</param>
        /// <param name="dtCols">The dt cols.</param>
        /// <returns></returns>
        private IDictionary<int, string> BuildColumnNameToIndexMap(IEnumerable<Dev2Column> dlCols,  DataColumnCollection dtCols)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();


            foreach (var dlC in dlCols)
            {
                var idx = dtCols.IndexOf(dlC.ColumnName);

                if (idx != -1)
                {
                    result.Add(idx,dlC.ColumnName);
                }
            }

            return result;
        }
    }
}
