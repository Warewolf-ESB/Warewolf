using System;
using Dev2.Common;

namespace Dev2.Converters {
    public class Dev2BaseConversionFactory : SpookyAction<IBaseConverter, Enum> {
        /// <summary>
        /// Used to create the conversion broker between types
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public IBaseConversionBroker CreateBroker(IBaseConverter from, IBaseConverter to) {
            return new Dev2BaseConversionBroker(from, to);
        }

        /// <summary>
        /// Create a base converter 
        /// </summary>
        /// <param name="typeOf"></param>
        /// <returns></returns>
        public IBaseConverter CreateConverter(enDev2BaseConvertType typeOf) {
            return FindMatch(typeOf);
        }
    }
}
