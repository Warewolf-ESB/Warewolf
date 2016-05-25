using System.Collections.Generic;
using System.Xml;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Activities
{
    interface IWebXmlConvert
    {
        void TryConvert(XmlNodeList children, IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, IDSFDataObject dataObj, int level = 0);
        void PushXmlIntoEnvironment(string input, int update, IDSFDataObject dataObj);
        string UnescapeRawXml(string innerXml);
    }
}