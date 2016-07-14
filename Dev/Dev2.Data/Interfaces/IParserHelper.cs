using System.Collections.Generic;
using System.Text;
using Dev2.Data.Exceptions;
using Dev2.Data.TO;
using Dev2.DataList.Contract;

namespace Dev2.Data.Interfaces
{
    internal interface IParserHelper
    {
        bool ProcessOpenRegion(string payload, bool openRegion, int i, ref ParseTO currentNode, ref StringBuilder region, ref char cur);
        ParseTO CurrentNode(ParseTO currentNode, StringBuilder region, int i);
        bool ShouldAddToRegion(string payload, char cur, char prev, int i, bool shouldAddToRegion, char charToCheck);
        bool IsValidIndex(ParseTO to);
        IIntellisenseResult AddErrorToResults(bool isRs, string part, Dev2DataLanguageParseError dev2DataLanguageParseError, bool isOpen);
        bool CheckValidIndex(ParseTO to, string part, int start, int end);
        bool CheckCurrentIndex(ParseTO to, int start, string raw, int end);
        IIntellisenseResult ValidateName(string name, string displayString);
        bool ProcessFieldsForRecordSet(ParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, out string search, out bool emptyOk, string display, IDev2DataLanguageIntellisensePart recordsetPart, string partName);
        void ProcessResults(IList<IIntellisenseResult> realResults, IIntellisenseResult intellisenseResult);
        bool ValidateName(string rawSearch, string displayString, IList<IIntellisenseResult> result, out IList<IIntellisenseResult> intellisenseResults);
        bool AddFieldResult(ParseTO payload, IList<IIntellisenseResult> result, string tmpString, string[] parts, bool isRs);
    }
}