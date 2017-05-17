using System.Collections.Generic;
using System.Text;
using System.Xml;
using Dev2.Data.Interfaces;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Interfaces
{
    public interface IComplexObjectHandler
    {
        void RemoveBlankComplexObjects();
        void AddComplexObject(IDataListVerifyPart part);
        IEnumerable<string> RefreshJsonObjects(IEnumerable<IComplexObjectItemModel> complexObjectItemModels);
        void RemoveUnusedChildComplexObjects(IComplexObjectItemModel parentItemModel, IComplexObjectItemModel complexObjectItemModel);
        void SortComplexObjects(bool @ascending);
        void GenerateComplexObjectFromJson(string parentObjectName, string json);
        void AddComplexObjectFromXmlNode(XmlNode xmlNode, IComplexObjectItemModel parent);
        void AddComplexObjectsToBuilder(StringBuilder result, IComplexObjectItemModel complexObjectItemModel);
        void DetectUnusedComplexObjects(IEnumerable<IDataListVerifyPart> partsToVerify);
        void RemoveUnusedComplexObjects();
        void ValidateComplexObject();
    }
}