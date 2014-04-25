namespace Dev2.Converters
{
    internal class Dev2BaseConversionBroker : IBaseConversionBroker
    {

        private readonly IBaseConverter _from;
        private readonly IBaseConverter _to;

        internal Dev2BaseConversionBroker(IBaseConverter from, IBaseConverter to)
        {
            _to = to;
            _from = from;
        }

        public string Convert(string payload)
        {
            string result;

            // convert from to base type
            if(_from.IsType(payload))
            {

                byte[] rawBytes = _from.NeutralizeToCommon(payload);

                // convert to expected type
                result = _to.ConvertToBase(rawBytes);


            }
            else
            {
                //throw new ConversionException - wrong base format
                throw new BaseTypeException("Base Conversion Broker was expecting [ " + _from.HandlesType() + " ] but the data was not in this format");
            }

            return result;
        }
    }
}
