using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.Web;

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


        /// <summary>
        /// Converts from a binary representation in the specified <see cref="Format" /> to the standard
        /// binary representation of a datalist.
        /// 
        /// THERE SHOULD ONLY EVER BE A SINGLE RECORDSET TO PROCESS ;)
        /// </summary>
        /// <param name="input">The binary representation in the specifeid <see cref="Format" /></param>
        /// <param name="shape"></param>
        /// <param name="errors"></param>
        /// <returns>
        /// An array of bytes that represent the datalist in the standard format.
        /// </returns>
        public IBinaryDataList ConvertTo(byte[] input, string shape, out ErrorResultTO errors)
        {
            string error;
            errors = new ErrorResultTO();
            IBinaryDataList targetDL = DataListTranslatorHelper.BuildTargetShape(shape, out error);
            errors.AddError(error);

           
            var rs = targetDL.FetchAllUserKeys().FirstOrDefault();

            DataTable dtbl = new DataTable(rs);


            if (rs != null)
            {
                IBinaryDataListEntry entry;
                
                // build up the columns ;)
                if (targetDL.TryGetEntry(rs, out entry, out error))
                {
                    var cols = entry.Columns;
                    foreach (var c in cols)
                    {
                        dtbl.Columns.Add(c.ColumnName, typeof(string));
                    }

                    string payload = Encoding.UTF8.GetString(input);

                    // now process data ;)
                    dtbl.ReadXml(new StringReader(payload));

                    // now convert to binary datalist ;)
                    int rowIdx = 1;
                    foreach (DataRow row in dtbl.Rows)
                    {
                        IList<IBinaryDataListItem> items = new List<IBinaryDataListItem>(cols.Count);
                        // build up the row
                        int idx = 0;
                        foreach (var item in row.ItemArray)
                        {
                            items.Add(new BinaryDataListItem(item.ToString(),rs, dtbl.Columns[idx].ColumnName, rowIdx));
                            idx++;
                        }

                        rowIdx++;
                        // add the row ;)
                        entry.TryPutRecordRowAt(items, 1, out error);
                        errors.AddError(error);
                    }

                }
                else
                {
                    errors.AddError(error);
                }   
            }
            return targetDL;
        }

        public string ConvertAndFilter(IBinaryDataList input, string filterShape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }
    }
}
