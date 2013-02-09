using System;
using System.Text;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;

// ReSharper disable CheckNamespace
namespace Dev2.Server.DataList.Translators
// ReSharper restore CheckNamespace
{
    internal sealed class DataListJSONTranslator : IDataListTranslator
    {
        private DataListFormat _format;
        private Encoding _encoding;

        public DataListFormat Format { get { return _format; } }
        public Encoding TextEncoding { get { return _encoding; } }

        public DataListJSONTranslator()
        {
            _format = DataListFormat.CreateFormat(GlobalConstants._JSON);
            _encoding = Encoding.UTF8;
        }

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList input, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if (input == null) throw new ArgumentNullException("input");
            throw new NotImplementedException();
        }

        public IBinaryDataList ConvertTo(byte[] input, string shape, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if (input == null) throw new ArgumentNullException("input");
            throw new NotImplementedException();
        }

     
    }
}
