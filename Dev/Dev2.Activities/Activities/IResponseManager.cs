using System.Collections.Generic;
using System.Xml;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Interfaces;

namespace Dev2.Activities
{
    public interface IResponseManager
    {
        bool IsObject { get; set; }
        string ObjectName { get; set; }
        IOutputDescription OutputDescription { get; set; }
        ICollection<IServiceOutputMapping> Outputs { get; set; }
        void PushResponseIntoEnvironment(string input, int update, IDSFDataObject dataObj);
        void PushResponseIntoEnvironment(string input, int update, IDSFDataObject dataObj, bool formatResult);
        string UnescapeRawXml(string innerXml);
        void TryConvert(XmlNodeList children, IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, IDSFDataObject dataObj);
        void TryConvert(XmlNodeList children, IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, IDSFDataObject dataObj, int level);
    }    
}