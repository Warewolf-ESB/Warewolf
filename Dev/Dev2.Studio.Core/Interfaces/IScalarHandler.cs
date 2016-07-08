using System.Collections.Generic;
using System.Xml;
using Dev2.Data.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Interfaces
{
    internal interface IScalarHandler
    {
        void FindMissingForScalar(IDataListVerifyPart part, List<IDataListVerifyPart> missingDataParts);
        void SetScalarItemsAsUsed();
        void AddScalars(XmlNode xmlNode);
        void SortScalars(bool @ascending);
        void FixNamingForScalar(IDataListItemModel scalar);
        void AddRowToScalars();
        void RemoveBlankScalars();
        void RemoveUnusedScalars();
        void AddMissingScalarParts(IDataListVerifyPart part);
    }
}