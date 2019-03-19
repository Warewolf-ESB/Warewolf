#pragma warning disable
using System.Collections.Generic;
using Dev2.Data.Interfaces.Enums;

namespace Dev2.Data.Interfaces
{
    public interface ICommonRecordSetUtil
    {
        string ReplaceRecordBlankWithStar(string fullRecSetName);
        string ReplaceRecordsetBlankWithStar(string fullRecSetName);
        string ReplaceRecordsetBlankWithIndex(string fullRecSetName, int length);
        string CreateRecordsetDisplayValue(string recsetName, string colName, string indexNum);
        string RemoveRecordsetBracketsFromValue(string value);
        enRecordsetIndexType GetRecordsetIndexType(string expression);
        bool IsStarIndex(string rs);
        string ExtractIndexRegionFromRecordset(string rs);
        string MakeValueIntoHighLevelRecordset(string value, bool starNotation);
        string ExtractFieldNameOnlyFromValue(string value);
        string ExtractFieldNameFromValue(string value);
        string ExtractRecordsetNameFromValue(string value);
        bool IsValueRecordsetWithFields(string value);
        bool IsValueRecordset(string value);
        string ReplaceRecordsetIndexWithStar(string expression);
        string ReplaceRecordsetIndexWithBlank(string expression);
        string RemoveRecordSetBraces(string search, ref bool isRs);
        void ProcessRecordSetFields(IParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1);
        void ProcessNonRecordsetFields(IParseTO payload, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1);
        bool RecordsetMatch(IParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string rawSearch, string search, bool emptyOk, string[] parts, IDev2DataLanguageIntellisensePart t1);
        void OpenRecordsetItem(IParseTO payload, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1);
        string ReplaceObjectBlankWithIndex(string fullRecSetName, int length);
    }
}