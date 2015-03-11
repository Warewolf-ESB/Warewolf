/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Convertors.Base;

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
            if (_from.IsType(payload))
            {
                byte[] rawBytes = _from.NeutralizeToCommon(payload);

                // convert to expected type
                result = _to.ConvertToBase(rawBytes);
            }
            else
            {
                //throw new ConversionException - wrong base format
                throw new BaseTypeException("Base Conversion Broker was expecting [ " + _from.HandlesType() +
                                            " ] but the data was not in this format");
            }

            return result;
        }
    }
}