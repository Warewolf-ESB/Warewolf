using System;
using System.Text;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;

namespace Dev2.Data.Translators
{
    public class Dev2DecisionStackTranslator : IDataListTranslator
    {

        private DataListFormat _format;
        private Encoding _encoding;

        public Dev2DecisionStackTranslator()
        {
            _format = DataListFormat.CreateFormat(GlobalConstants._DECISION_STACK);
            _encoding = Encoding.UTF8;    
        }

        public DataListFormat Format
        {
            get { return _format; }
        }

        public Encoding TextEncoding
        {
            get { return _encoding; }
        }

        public DataListFormat HandlesType()
        {
            return _format;
        }

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
    }
}
