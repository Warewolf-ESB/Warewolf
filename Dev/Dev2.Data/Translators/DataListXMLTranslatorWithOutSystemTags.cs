using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Translators;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.Web;

namespace Dev2.Server.DataList.Translators
{
    internal sealed class DataListXMLTranslatorWithOutSystemTags : IDataListTranslator
    {
        private const string _rootTag = "DataList";
        private readonly DataListFormat _format;
        private readonly Encoding _encoding;


        public DataListFormat Format { get { return _format; } }
        public Encoding TextEncoding { get { return _encoding; } }

        public DataListXMLTranslatorWithOutSystemTags()
        {
            _format = DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags, EmitionTypes.XML, "text/xml");
            _encoding = Encoding.UTF8;
        }

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList payload, out ErrorResultTO errors)
        {
            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            StringBuilder result = new StringBuilder("<" + _rootTag + ">");
            errors = new ErrorResultTO();
            string error = string.Empty;

            IList<string> itemKeys = payload.FetchAllKeys();

            foreach (string key in itemKeys)
            {
                IBinaryDataListEntry entry = null;
                
                // This check was never here - this means this method has no testing and was never sane ;)

                if(!DataListUtil.isSystemTag(key.Replace("Dev2System.","")))
                {
                    if (payload.TryGetEntry(key, out entry, out error))
                    {
                        if (entry.IsRecordset)
                        {
                            int cnt = entry.FetchLastRecordsetIndex();
                            for (int i = 1; i <= cnt; i++)
                            {
                                IList<IBinaryDataListItem> rowData = entry.FetchRecordAt(i, out error);
                                if (error != string.Empty)
                                {
                                    errors.AddError(error);
                                }
                                result.Append("<");
                                result.Append(entry.Namespace);
                                result.Append(">");

                                foreach (IBinaryDataListItem col in rowData)
                                {
                                    string fName = col.FieldName;

                                    result.Append("<");
                                    result.Append(fName);
                                    result.Append(">");
                                    result.Append(col.TheValue);
                                    result.Append("</");
                                    result.Append(fName);
                                    result.Append(">");
                                }

                                result.Append("</");
                                result.Append(entry.Namespace);
                                result.Append(">");
                            }
                        }
                        else
                        {
                            string fName = entry.Namespace;
                            IBinaryDataListItem val = entry.FetchScalar();
                            if (val != null)
                            {
                                result.Append("<");
                                result.Append(fName);
                                result.Append(">");
                                result.Append(val.TheValue);
                                result.Append("</");
                                result.Append(fName);
                                result.Append(">");
                            }
                        }
                    }
                }
            }

            result.Append("</" + _rootTag + ">");

            DataListTranslatedPayloadTO tmp = new DataListTranslatedPayloadTO(result.ToString());

            return tmp;
        }

        public IBinaryDataList ConvertTo(byte[] input, string targetShape, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            string payload = Encoding.UTF8.GetString(input);

            IBinaryDataList result = new BinaryDataList();

            

            // build shape
            if (targetShape == null)
            {
                errors.AddError("Null payload or shape");
            }
            else
            {
                string error;
                result = DataListTranslatorHelper.BuildTargetShape(targetShape, out error);
                errors.AddError(error);
                
                

                // populate the shape 
                if (payload != string.Empty)
                {
                    try
                    {
                        string toLoad = DataListUtil.StripCrap(payload); // clean up the rubish ;)
                        XmlDocument xDoc = new XmlDocument();
                        try
                        {
                            xDoc.LoadXml(toLoad);

                            if (xDoc.DocumentElement != null)
                            {
                                var tmp = xDoc.DocumentElement.ChildNodes;

                                if (tmp.Count == 1)
                                {
                                    // funny single scalar issue ;)
                                    throw new Exception("Single scalar issue");
                                }
                            }
                        }
                        catch
                        {
                            // Append new root tags ;)
                            toLoad = "<root>" + toLoad + "</root>";
                            xDoc.LoadXml(toLoad);
                        }

                        if (!string.IsNullOrEmpty(toLoad))
                        {
                            if (xDoc.DocumentElement != null)
                            {
                                XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                                IDictionary<string, int> indexCache = new Dictionary<string, int>();

                                if (children != null)
                                {
                                    IBinaryDataListEntry entry = null;

                                    // spin through each element in the XML
                                    var loopCnt = children.Count;
                                    ErrorResultTO invokeErrors = new ErrorResultTO();
                                    foreach(XmlNode c in children){
                                        var hasCorrectIoDirection = true;
                                        
                                        if (c.Attributes != null)
                                        {
                                            var columnIoDirectionAttribute = c.Attributes["ColumnIODirection"];
                                            if (columnIoDirectionAttribute != null)
                                            {
                                                var columnIoDirectionValue = columnIoDirectionAttribute.Value;
                                                var hasCorrectIoDirectionFromAttribute = columnIoDirectionValue == enDev2ColumnArgumentDirection.Output.ToString() || columnIoDirectionValue == enDev2ColumnArgumentDirection.Both.ToString();
                                                hasCorrectIoDirection = hasCorrectIoDirectionFromAttribute;
                                            }
                                        }

                                        if (!DataListUtil.isSystemTag(c.Name) && hasCorrectIoDirection)
                                        {

                                            // scalars and recordset fetch
                                            if (result.TryGetEntry(c.Name, out entry, out error))
                                            {
                                                if (entry.IsRecordset)
                                                {
                                                    // fetch recordset index
                                                    int fetchIdx = 0;
                                                    int idx = 1; // recset index
                                                    if (indexCache.TryGetValue(c.Name, out fetchIdx))
                                                    {
                                                        idx = fetchIdx;
                                                    }
                                                    else
                                                    {
                                                        idx = 1; //re-set idx on cache miss ;)
                                                    }
                                                    // process recordset
                                                    XmlNodeList nl = c.ChildNodes;
                                                    int rowLen = c.InnerText.Length;

                                                    if (nl != null)
                                                    {
                                                        // TODO : We need to lock on row storage here?!
                                                        foreach (XmlNode subc in nl)
                                                        {
                                                            entry.TryPutRecordItemAtIndex(Dev2BinaryDataListFactory.CreateBinaryItem(subc.InnerXml, c.Name, subc.Name, idx), idx, out error);
                                                            invokeErrors.AddError(error);
                                                        }

                                                        // update this recordset index
                                                        indexCache[c.Name] = ++idx;
                                                    }

                                                }
                                                else
                                                {
                                                    // process scalar
                                                    entry.TryPutScalar(
                                                        Dev2BinaryDataListFactory.CreateBinaryItem(c.InnerXml, c.Name),
                                                        out error);

                                                    invokeErrors.AddError(error);
                                                }
                                            }
                                            else
                                            {
                                                invokeErrors.AddError(error);
                                                entry = null;
                                            }
                                        }
                                    };
                                }

                            }
                        }

                    }
                    catch (Exception e)
                    {
                        // if use passed in empty input they only wanted the shape ;)
                        if (input.Length > 0)
                        {
                            errors.AddError(e.Message);
                        }
                    }
                 
                }
            }
              
             
            return result;

        }

        public string ConvertAndFilter(IBinaryDataList payload, string filterShape, out ErrorResultTO errors)
        {
            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            StringBuilder result = new StringBuilder("<" + _rootTag + ">");
            errors = new ErrorResultTO();
            string error;

            IBinaryDataList targetDL = DataListTranslatorHelper.BuildTargetShape(filterShape, out error);

            IList<string> itemKeys = targetDL.FetchAllKeys();

            foreach (string key in itemKeys)
            {
                IBinaryDataListEntry entry = null;
                IBinaryDataListEntry tmpEntry = null;
                if (payload.TryGetEntry(key, out entry, out error) && targetDL.TryGetEntry(key, out tmpEntry, out error))
                {

                    if (entry.IsRecordset)
                    {
                        int cnt = entry.FetchLastRecordsetIndex();
                        for (int i = 1; i <= cnt; i++)
                        {
                            IList<IBinaryDataListItem> rowData = entry.FetchRecordAt(i, out error);
                            if (error != string.Empty)
                            {
                                errors.AddError(error);
                            }
                            result.Append("<");
                            result.Append(entry.Namespace);
                            result.Append(">");
                                                           
                                foreach (IBinaryDataListItem col in rowData)
                                {
                                    if(tmpEntry.Columns.Any((c=>c.ColumnName == col.FieldName)))
                                    {
                                        string fName = col.FieldName;

                                        result.Append("<");
                                        result.Append(fName);
                                        result.Append(">");
                                        result.Append(col.TheValue);
                                        result.Append("</");
                                        result.Append(fName);
                                        result.Append(">");
                                    }                                                                     
                                }
                                                       

                            result.Append("</");
                            result.Append(entry.Namespace);
                            result.Append(">");
                        }
                    }
                    else
                    {
                        string fName = entry.Namespace;
                        IBinaryDataListItem val = entry.FetchScalar();
                        if (val != null)
                        {
                            result.Append("<");
                            result.Append(fName);
                            result.Append(">");
                            result.Append(val.TheValue);
                            result.Append("</");
                            result.Append(fName);
                            result.Append(">");
                        }
                    }
                }

            }

            result.Append("</" + _rootTag + ">");

            return result.ToString();
        }

        public DataListFormat HandlesType()
        {
            return _format;
        }
    }
}
