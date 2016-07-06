using System.Collections.Generic;
using System.Xml;
using Dev2.Data.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IRecordsetHandler
    {
        IEnumerable<string> RefreshRecordSets(IEnumerable<IRecordSetItemModel> toList, IList<string> accList);
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
    }
}