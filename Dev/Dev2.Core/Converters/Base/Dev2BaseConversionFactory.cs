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
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Convertors.Base;

namespace Dev2.Converters
{
    public class Dev2BaseConversionFactory : SpookyAction<IBaseConverter, Enum>
    {
        /// <summary>
        ///     Used to create the conversion broker between types
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public IBaseConversionBroker CreateBroker(IBaseConverter from, IBaseConverter to)
        {
            return new Dev2BaseConversionBroker(from, to);
        }

        /// <summary>
        ///     Create a base converter
        /// </summary>
        /// <param name="typeOf"></param>
        /// <returns></returns>
        public IBaseConverter CreateConverter(enDev2BaseConvertType typeOf)
        {
            switch (typeOf)
            {
                case enDev2BaseConvertType.Base64:
                    return new Dev2Base64Converter();
                case enDev2BaseConvertType.Binary:
                    return new Dev2BinaryConverter();
                case enDev2BaseConvertType.Hex:
                    return new Dev2HexConverter();
                case enDev2BaseConvertType.Text:
                    return new Dev2TextConverter();
                default:
                    throw new Exception("");
            }
        }
    }
}