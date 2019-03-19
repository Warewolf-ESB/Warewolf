#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Data.Interfaces;


namespace Dev2.DataList.Contract
{
    public interface IDev2DataLanguageParser
    {
        IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList);
        IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList, bool addCompleteParts);
        IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList, bool addCompleteParts, IIntellisenseFilterOpsTO filterTo);
        IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList, bool addCompleteParts, IIntellisenseFilterOpsTO filterTo, bool isFromIntellisense);
        IList<IParseTO> MakeParts(string payload);
        IList<IParseTO> MakeParts(string payload, bool addCompleteParts);
        IList<IIntellisenseResult> ParseExpressionIntoParts(string expression, IList<IDev2DataLanguageIntellisensePart> parts);
    }
}
