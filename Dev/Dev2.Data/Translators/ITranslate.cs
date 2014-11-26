
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Enums;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Translators
{

    public interface ITranslate
    {

        /// <summary>
        /// Convert string to binary object
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="targetShape">The target shape.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IBinaryDataList DeSerialize(string data, string targetShape, enTranslationTypes typeOf, out string error);
    }
}
