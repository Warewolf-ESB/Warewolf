using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dev2.DataList.Contract
{
    public interface IDataListBinder {

        IEnumerable<string> EnumTextStreamLines(StreamReader stream);

        string RecursiveDescentScanner(string dataSource, string transformation);

        string RegionParser(string dataSource, string transformation);

        string TextAndJScriptRegionEvaluator(List<string> dataList, string transformation, string currentValue = "", bool dataBindRecursive = false, string rootService = "");

        string BindEnvironmentVariables(string transformation, string rootServiceName);

        string ParseDataRegionTokens(IList<string> ambientDataList, string transformation);

        string CleanStringForXmlName(string activityString);

        string NormalizeFieldValue(string value);

        //IList<UnlimitedObject> FindDataObjectByTagName(IList<string> dataList, string tagName);

        //UnlimitedObject DataListToUnlimitedObject(IList<string> dataList);

        //bool TagsExist(string dataTags, IList<string> dataList, out UnlimitedObject dataObject);

        bool ResultValidation(string result, string resultValidationRequiredTags, string resultValidationExpression);

        Guid InvokeDsfService(string requestXml, string uri, Guid dataListID); // Should be moved out ??

        //string ParseHTML(string html, string bindingData, IFrameworkDataChannel dsfChannel, IDSFDataObject parentRequest);

    }
}
