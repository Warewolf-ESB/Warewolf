using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList input,out ErrorResultTO errors)
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
        
        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors)
        {
            var dbData = new DataTable();
            IBinaryDataListEntry entry;
            errors = null;
            string error;
            if(input.TryGetEntry(recsetName, out entry, out error))
            {
                if(entry.IsRecordset)
                {
                    var cols = entry.Columns;
                    var dataColumns = cols.ToList().ConvertAll(column => new DataColumn(column.ColumnName));
                    dbData.Columns.AddRange(dataColumns.ToArray());
                    var fetchRecordsetIndexes = entry.FetchRecordsetIndexes();
                    while(fetchRecordsetIndexes.HasMore())
                    {
                        var binaryDataListItems = entry.FetchRowAt(fetchRecordsetIndexes.FetchNextIndex(), out error);
                        dbData.LoadDataRow(binaryDataListItems.Select(item => item.TheValue as object).ToArray(), LoadOption.OverwriteChanges);
                    }
                }
            }
            return dbData;
        }

        public Guid Populate(object input, Guid targetDLID, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO invokeErrors;
            IBinaryDataList targetDL = compiler.FetchBinaryDataList(targetDLID, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            DataTable dbData = (input as DataTable);

            var tmp = targetDL.FetchAllEntries();

            string rsName = string.Empty;

            // pick the first recordset to deal with ;)
            int rsCnt = 0;
            foreach (var tt in tmp)
            {
                
                if (tt.IsRecordset)
                {
                    if (rsCnt == 0)
                    {
                        rsName = tt.Namespace;
                    }

                    rsCnt++;
                }
            }

            // ensure we only have a single recordset to map too ;)
            if(string.IsNullOrEmpty(rsName) || rsCnt > 1)
            {
                throw new Exception("DataTable translator can only map to a single recordset!");
            }

            if(dbData != null)
            {
                IBinaryDataListEntry entry;

                // build up the columns ;)
                string error;
                if(targetDL.TryGetEntry(rsName, out entry, out error))
                {

                    if(entry.IsRecordset)
                    {
                        var cols = entry.Columns;
                        IDictionary<int, string> colMapping = BuildColumnNameToIndexMap(entry.Columns, dbData.Columns);

                        // now convert to binary datalist ;)
                        int rowIdx = entry.FetchAppendRecordsetIndex();

                        foreach(DataRow row in dbData.Rows)
                        {
                            IList<IBinaryDataListItem> items = new List<IBinaryDataListItem>(cols.Count);
                            // build up the row
                            int idx = 0;

                            foreach(var item in row.ItemArray)
                            {
                                string colName;

                                if(colMapping.TryGetValue(idx, out colName))
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
                        if(dbData.Rows != null && dbData.Rows.Count == 1)
                        {
                            var row = dbData.Rows[0].ItemArray;
                            // Look up the correct index from the columns ;)

                            int pos = 0;
                            var cols = dbData.Columns;
                            int idx = 0;

                            while(pos < cols.Count && idx == -1)
                            {
                                if(cols[pos].ColumnName == entry.Namespace)
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

            compiler.PushBinaryDataList(targetDL.UID, targetDL, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            return targetDL.UID;
        }


        public IBinaryDataList ConvertTo(object input, string shape, out ErrorResultTO errors)
        {
            string error;
            errors = new ErrorResultTO();

            TranslatorUtils tu = new TranslatorUtils();
            ErrorResultTO invokeErrors;
            var targetDL = tu.TranslateShapeToObject(shape, false, out invokeErrors);

            errors.MergeErrors(invokeErrors);

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
                            int idx = 0;
                            
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
