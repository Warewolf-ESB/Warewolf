/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;

// ReSharper disable CheckNamespace
namespace Dev2.DataList.Contract
// ReSharper restore CheckNamespace
{
    public interface IDev2DataLanguageParser
    {
        /// <summary>
        /// Parses the data language for intellisense.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="dataList">The data list.</param>
        /// <param name="addCompleteParts">if set to <c>true</c> [add complete parts].</param>
        /// <param name="filterTo">The filter TO.</param>
        /// <param name="isFromIntellisense"></param>
        /// <returns></returns>
        IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList, bool addCompleteParts = false, IIntellisenseFilterOpsTO filterTo = null, bool isFromIntellisense = false);

        /// <summary>
        /// Makes the parts.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="addCompleteParts">Allows open regions for intellisense</param>
        /// <returns></returns>
        IList<ParseTO> MakeParts(string payload, bool addCompleteParts = false);

        /// <summary>
        /// Parses the expression into parts.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parts"></param>
        /// <returns></returns>
        IList<IIntellisenseResult> ParseExpressionIntoParts(string expression, IList<IDev2DataLanguageIntellisensePart> parts);

    }
}
