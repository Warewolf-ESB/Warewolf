using System.Text;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;

namespace Dev2.Data.Translators
{
    public class DataListKnockoutModelTranslator : IDataListTranslator
    { 
        public DataListFormat Format
        {
            get { throw new System.NotImplementedException(); }
        }

        public Encoding TextEncoding
        {
            get { throw new System.NotImplementedException(); }
        }

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList input, out ErrorResultTO errors)
        {
            throw new System.NotImplementedException();
        }

        public IBinaryDataList ConvertTo(byte[] input, string shape, out ErrorResultTO errors)
        {
            throw new System.NotImplementedException();
        }
    }
}
