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

        #region Private Methods

        #endregion



    }
}
