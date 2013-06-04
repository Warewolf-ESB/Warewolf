using System.Collections.Generic;


namespace Dev2.DataList.Contract
{
    public interface IDev2DataLanguageParser {

        /// <summary>
        /// Parses the data language for intellisense.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="dataList">The data list.</param>
        /// <param name="addCompleteParts">if set to <c>true</c> [add complete parts].</param>
        /// <param name="fiterTO">The fiter TO.</param>
        /// <returns></returns>
        IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList, bool addCompleteParts = false, IntellisenseFilterOpsTO fiterTO = null);

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
        /// <returns></returns>
        IList<ParseTO> MakeParts(string payload);

        /// <summary>
        /// Parses the expression into parts.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        IList<IIntellisenseResult> ParseExpressionIntoParts(string expression, IList<IDev2DataLanguageIntellisensePart> parts);

    }
}
