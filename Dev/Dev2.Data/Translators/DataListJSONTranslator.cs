
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
using System.Data;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Translators;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.Web;

// ReSharper disable CheckNamespace

namespace Dev2.Server.DataList.Translators
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Used to emit JSON data to the user ;)
    /// </summary>
    internal sealed class DataListJsonTranslator : IDataListTranslator
    {
        private readonly DataListFormat _format;
        private readonly Encoding _encoding;

        public DataListFormat Format
        {
            get { return _format; }
        }

        public Encoding TextEncoding
        {
            get { return _encoding; }
        }

        public DataListJsonTranslator()
        {
            _format = DataListFormat.CreateFormat(GlobalConstants._JSON, EmitionTypes.JSON, "application/json");
            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Converts from a binary representation in the standard format to the specified <see cref="Format" />.
        /// </summary>
        /// <param name="input">The binary representation of the datalist.</param>
        /// <param name="errors">The errors.</param>
        /// <returns>
        /// An array of bytes that represent the datalist in the specified <see cref="Format" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">input</exception>
        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList input, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if(input == null) throw new ArgumentNullException("input");

            IList<string> itemKeys = input.FetchAllUserKeys();
            errors = new ErrorResultTO();

            StringBuilder result = new StringBuilder("{");
            int keyCnt = 0;

            foreach(string key in itemKeys)
            {
                IBinaryDataListEntry entry;

                // This check was never here - this means this method has no testing and was never sane ;)

                string error;
                if(input.TryGetEntry(key, out entry, out error))
                {
                    if(entry.IsRecordset)
                    {
                        result.Append(ProcessRecordSet(entry, out error));
                        errors.AddError(error);
                    }
                    else
                    {
                        result.Append(ProcessScalar(entry));
                    }
                }

                errors.AddError(error);

                // wack in , for field separator ;)
                keyCnt++;
                if(keyCnt < itemKeys.Count)
                {
                    result.Append(",");
                }
            }

            result.Append("}");

            DataListTranslatedPayloadTO tmp = new DataListTranslatedPayloadTO(result.ToString());

            return tmp;
        }

        /// <summary>
        /// Handleses the type.
        /// </summary>
        /// <returns></returns>
        public DataListFormat HandlesType()
        {
            return _format;
        }

        /// <summary>
        /// Converts from a binary representation in the specified <see cref="Format" /> to the standard
        /// binary representation of a datalist.
        /// </summary>
        /// <param name="input">The binary representation in the specifeid <see cref="Format" /></param>
        /// <param name="shape"></param>
        /// <param name="errors"></param>
        /// <returns>
        /// An array of bytes that represent the datalist in the standard format.
        /// </returns>
        /// <exception cref="System.NotImplementedException">JSON ConvertTO does not exist. Please use XML ;)</exception>
        public IBinaryDataList ConvertTo(byte[] input, StringBuilder shape, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            throw new NotImplementedException("JSON ConvertTO does not exist. Please use XML ;)");
        }

        public IBinaryDataList ConvertTo(object input, StringBuilder shape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the and only map inputs.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IBinaryDataList ConvertAndOnlyMapInputs(byte[] input, StringBuilder shape, out ErrorResultTO errors)
        {
           throw new NotImplementedException();
        }

        public Guid Populate(object input, Guid targetDl, string outputDefs, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the and filter.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="filterShape">The filter shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">payload</exception>
        public StringBuilder ConvertAndFilter(IBinaryDataList payload, StringBuilder filterShape, out ErrorResultTO errors)
        {

            if(payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            int keyCnt = 0;
            errors = new ErrorResultTO();

            TranslatorUtils tu = new TranslatorUtils();
            ErrorResultTO invokeErrors;

            IBinaryDataList targetDl = tu.TranslateShapeToObject(filterShape, false, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            IList<string> itemKeys = targetDl.FetchAllUserKeys();
            StringBuilder result = new StringBuilder("{");

            foreach(string key in itemKeys)
            {
                IBinaryDataListEntry entry;
                IBinaryDataListEntry tmpEntry;
                string error;
                if(payload.TryGetEntry(key, out entry, out error) && targetDl.TryGetEntry(key, out tmpEntry, out error))
                {

                    if(entry.IsRecordset)
                    {
                        result.Append(ProcessRecordSet(entry, out error));
                        errors.AddError(error);
                    }
                    else
                    {
                        result.Append(ProcessScalar(entry));
                    }

                    // wack in , for field separator ;)
                    keyCnt++;
                    if(keyCnt < itemKeys.Count)
                    {
                        result.Append(",");
                    }
                }

                errors.AddError(error);
            }

            result.Append("}");

            return result;
        }

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions)
        {
            errors = null;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the scalar.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        private string ProcessScalar(IBinaryDataListEntry entry)
        {
            StringBuilder result = new StringBuilder();
            string fName = entry.Namespace;
            IBinaryDataListItem val = entry.FetchScalar();

            if(val != null)
            {
                result.Append("\"");
                result.Append(fName);
                result.Append("\":\"");
                result.Append(val.TheValue);
                result.Append("\"");
            }

            return result.ToString();
        }

        /// <summary>
        /// Processes the record set.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private string ProcessRecordSet(IBinaryDataListEntry entry, out string error)
        {
            StringBuilder result = new StringBuilder();
            error = string.Empty;

            // MAKE RS START ;)
            result.Append("\"");
            result.Append(entry.Namespace);
            result.Append("\" : [");

            IIndexIterator idxItr = entry.FetchRecordsetIndexes();

            int rsCnt = 0;

            while(idxItr.HasMore() && !entry.IsEmpty())
            {
                int idx = idxItr.FetchNextIndex();

                IList<IBinaryDataListItem> rowData = entry.FetchRecordAt(idx, out error);
                result.Append("{");

                int colIdx = 0;
                foreach(IBinaryDataListItem col in rowData)
                {
                    result.Append("\"");
                    result.Append(col.FieldName);
                    result.Append("\":\"");
                    result.Append(col.TheValue);
                    result.Append("\"");

                    // add , if need be ;)
                    colIdx++;
                    if(colIdx < rowData.Count)
                    {
                        result.Append(",");
                    }
                }

                result.Append("}");

                // append , for row data ;)
                rsCnt++;
                if(rsCnt < idxItr.Count)
                {
                    result.Append(", ");
                }
            }

            // END RS ;)
            result.Append("]");


            return result.ToString();
        }
    }
}
