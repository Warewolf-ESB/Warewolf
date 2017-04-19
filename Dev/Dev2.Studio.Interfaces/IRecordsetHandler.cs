using System.Collections.Generic;
using System.Xml;
using Dev2.Data.Interfaces;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Interfaces
{
    public interface IRecordsetHandler
    {
        void AddRecordsetNamesIfMissing();
        void RemoveBlankRecordsets();
        void RemoveBlankRecordsetFields();
        void ValidateRecordsetChildren(IRecordSetItemModel recset);
        void ValidateRecordset();
        void AddRowToRecordsets();
        void AddRecordSet();
        void SortRecset(bool @ascending);
        void AddRecordSets(XmlNode xmlNode);
        void SetRecordSetItemsAsUsed();
        void FindMissingPartsForRecordset(IDataListVerifyPart part, List<IDataListVerifyPart> missingDataParts);
        bool BuildRecordSetErrorMessages(IRecordSetItemModel model, out string errorMessage);
        void AddMissingTempRecordSetList(IEnumerable<IRecordSetItemModel> tmpRecsetList);
        void AddMissingTempRecordSet(IDataListVerifyPart part, IRecordSetItemModel tmpRecset);
        void AddMissingRecordSetPart(IRecordSetItemModel recsetToAddTo, IDataListVerifyPart part);
        void RemoveUnusedRecordSets();
    }
}