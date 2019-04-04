#pragma warning disable
using System.Collections.Generic;
using System.Text;

namespace Dev2.Data.Interfaces
{
    public interface IParserHelper
    {
        bool ProcessOpenRegion(string payload, bool openRegion, int i, ref IParseTO currentNode, ref StringBuilder region, ref char cur);
        IParseTO CurrentNode(IParseTO currentNode, StringBuilder region, int i);
        bool ShouldAddToRegion(string payload, char cur, char prev, int i, bool shouldAddToRegion, char charToCheck);
        bool IsValidIndex(IParseTO to);
        IIntellisenseResult AddErrorToResults(bool isRs, string part, IDev2DataLangaugeParseError dev2DataLanguageParseError, bool isOpen);
        bool CheckValidIndex(IParseTO to, string part, int start, int end);
        bool CheckCurrentIndex(IParseTO to, int start, string raw, int end);
        IIntellisenseResult ValidateName(string name, string displayString);
        bool ProcessFieldsForRecordSet(IParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, out string search, out bool emptyOk, string display, IDev2DataLanguageIntellisensePart recordsetPart, string partName);
        void ProcessResults(IList<IIntellisenseResult> realResults, IIntellisenseResult intellisenseResult);
        bool ValidateName(string rawSearch, string displayString, IList<IIntellisenseResult> result, out IList<IIntellisenseResult> intellisenseResults);
        bool AddFieldResult(IParseTO payload, IList<IIntellisenseResult> result, string tmpString, string[] parts, bool isRs);
    }
}