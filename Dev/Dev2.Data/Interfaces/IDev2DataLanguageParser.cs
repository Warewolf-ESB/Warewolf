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
        /// <param name="fiterTO">The filter TO.</param>
        /// <param name="isFromIntellisense"></param>
        /// <returns></returns>
        IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList, bool addCompleteParts = false, IntellisenseFilterOpsTO fiterTO = null, bool isFromIntellisense = false);

        /// <summary>
        /// Parses for missing data list items.
        /// </summary>
        /// <param name="parts">The parts.</param>
        /// <param name="dataList">The data list.</param>
        /// <returns></returns>
        IList<IIntellisenseResult> ParseForMissingDataListItems(IList<IDataListVerifyPart> parts, string dataList);

        /// <summary>
        /// Makes the parts.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="addCompleteParts">Allows open regions for intellisense</param>
        /// <returns></returns>
        IList<ParseTO> MakeParts(string payload, bool addCompleteParts = false);

        /// <summary>
        /// Makes the parts excluding recordset index with is added back to the results later.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        IList<ParseTO> MakePartsWithOutRecsetIndex(string payload);

        /// <summary>
        /// Parses the expression into parts.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parts"></param>
        /// <returns></returns>
        IList<IIntellisenseResult> ParseExpressionIntoParts(string expression, IList<IDev2DataLanguageIntellisensePart> parts);

    }
}
