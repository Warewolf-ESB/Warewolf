/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Convertors.Case;

namespace Dev2
{
    public static class CaseConverterFactory
    {
        /// <summary>
        ///     Instantiates a concreate implementation of the ICaseConvertTO
        /// </summary>
        public static ICaseConvertTO CreateCaseConverterTO(string stringToConvert, string convertType, string result,
            int indexNumber)
        {
            return new CaseConvertTO(stringToConvert, convertType, result, indexNumber);
        }

        /// <summary>
        ///     Instantiates a concreate implementation of the ICaseConverter
        /// </summary>
        public static ICaseConverter CreateCaseConverter()
        {
            return new CaseConverter();
        }
    }
}