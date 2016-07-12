using Dev2.DataList.Contract;

namespace Dev2.Data.Interfaces
{
    internal interface ICommonRecordSetUtil
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
    }
}