using System.Collections.Generic;
using System.Text;
using System.Xml;
using Dev2.Data.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using Newtonsoft.Json.Linq;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IComplexObjectHandler
    {
        void RemoveBlankComplexObjects();
        void AddComplexObject(IDataListVerifyPart part);
        IEnumerable<string> RefreshJsonObjects(IEnumerable<IComplexObjectItemModel> complexObjectItemModels);
        void RemoveUnusedChildComplexObjects(IComplexObjectItemModel parentItemModel, IComplexObjectItemModel complexObjectItemModel);
        void SortComplexObjects(bool @ascending);
        void GenerateComplexObjectFromJson(string parentObjectName, string json);
        void ProcessObjectForComplexObjectCollection(IComplexObjectItemModel parentObj, JObject objToProcess);
        string GetNameForArrayComplexObject(XmlNode xmlNode, bool isArray);
        void AddComplexObjectFromXmlNode(XmlNode xmlNode, ComplexObjectItemModel parent);
        void AddComplexObjectsToBuilder(StringBuilder result, IComplexObjectItemModel complexObjectItemModel);
        void DetectUnusedComplexObjects(IEnumerable<IDataListVerifyPart> partsToVerify);
        void SetComplexObjectParentIsUsed(IComplexObjectItemModel complexObjectItemModel);
    }
}