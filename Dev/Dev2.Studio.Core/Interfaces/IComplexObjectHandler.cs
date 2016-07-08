using System.Collections.Generic;
using System.Text;
using System.Xml;
using Dev2.Data.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;

namespace Dev2.Studio.Core.Interfaces
{
    internal interface IComplexObjectHandler
    {
        void RemoveBlankComplexObjects();
        void AddComplexObject(IDataListVerifyPart part);
        IEnumerable<string> RefreshJsonObjects(IEnumerable<IComplexObjectItemModel> complexObjectItemModels);
        void RemoveUnusedChildComplexObjects(IComplexObjectItemModel parentItemModel, IComplexObjectItemModel complexObjectItemModel);
        void SortComplexObjects(bool @ascending);
        void GenerateComplexObjectFromJson(string parentObjectName, string json);
        void AddComplexObjectFromXmlNode(XmlNode xmlNode, ComplexObjectItemModel parent);
        void AddComplexObjectsToBuilder(StringBuilder result, IComplexObjectItemModel complexObjectItemModel);
        void DetectUnusedComplexObjects(IEnumerable<IDataListVerifyPart> partsToVerify);
        void RemoveUnusedComplexObjects();
    }
}