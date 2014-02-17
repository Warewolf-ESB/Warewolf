using System;
using System.Text;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;

// ReSharper disable once CheckNamespace
namespace Dev2.Server.DataList.Translators
{
    internal sealed class StudioDebugTranslator : StudioTranslator, IDataListTranslator
    {
        public StudioDebugTranslator()
            : base(DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), Encoding.UTF8)
        {
        }

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList payload, out ErrorResultTO errors)
        {
            if(payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            StringBuilder result = new StringBuilder("<" + RootTag + ">");
            errors = new ErrorResultTO();
            var itemKeys = payload.FetchAllKeys();

            foreach(var key in itemKeys)
            {
                IBinaryDataListEntry entry;
                string error;
                if(payload.TryGetEntry(key, out entry, out error))
                {
                    if(entry.IsRecordset)
                    {
                        DoRecordSetAppending(errors, entry, result);
                    }
                    else
                    {
                        var fName = entry.Namespace;
                        var val = entry.FetchScalar();
                        if(val != null)
                        {
                            DoScalarAppending(result, fName, val);
                        }
                    }
                }

            }

            result.Append("</" + RootTag + ">");

            DataListTranslatedPayloadTO tmp = new DataListTranslatedPayloadTO(result.ToString());

            return tmp;
        }

        /// <summary>
        /// Converts from a binary representation in the specified <see cref="IDataListTranslator.Format" /> to the standard
        /// binary representation of a datalist.
        /// </summary>
        /// <param name="input">The binary representation in the specified <see cref="IDataListTranslator.Format" /></param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns>
        /// An array of bytes that represent the datalist in the standard format.
        /// </returns>
        public IBinaryDataList ConvertTo(byte[] input, string shape, out ErrorResultTO errors)
        {
            return base.ConvertTo(input, shape, out errors);
        }

        #region Private Methods

        #endregion



    }
}
