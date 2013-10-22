using System.Data;
using Dev2.Common;
using Dev2.Data.Translators;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.DataList.Contract.Translators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

// ReSharper disable CheckNamespace
namespace Dev2.Server.DataList.Translators
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// The standard DataList translator used in the system
    /// </summary>
    internal sealed class DataListXMLTranslatorDebug : IDataListTranslator
    {
        private static readonly string _rootTag = "DataList";
        private DataListFormat _format;
        private Encoding _encoding;


        public DataListFormat Format { get { return _format; } }
        public Encoding TextEncoding { get { return _encoding; } }

        public DataListXMLTranslatorDebug()
        {
            _format = DataListFormat.CreateFormat(GlobalConstants._XML_DEBUG);
            _encoding = Encoding.UTF8;
        }

        public DataListFormat HandlesType()
        {
            return _format;
        }

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList payload, out ErrorResultTO errors)
        {
            if (payload == null)
            {
                throw new ArgumentNullException("input");
            }

            StringBuilder result = new StringBuilder("<" + _rootTag + ">");
            errors = new ErrorResultTO();
            string error = string.Empty;

            IList<string> itemKeys = payload.FetchAllKeys();

            ErrorResultTO invokeErrors;
            TranslatorUtils tu = new TranslatorUtils();

            foreach (string key in itemKeys)
            {
                IBinaryDataListEntry entry = null;
                if (payload.TryGetEntry(key, out entry, out error))
                {

                    if (entry.IsRecordset)
                    {
                        int cnt = entry.FetchLastRecordsetIndex();

                        if(entry.IsEmpty())
                        {
                            cnt = 0; // avoid emiting a blank record if it is ment to be empty ;)
                        }

                        for (int i = 1; i <= cnt; i++)
                        {
                            IList<IBinaryDataListItem> rowData = entry.FetchRecordAt(i, out error);
                            if (error != string.Empty) {
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

                                // Travis.Frisinger 04.02.2013
                                if (!col.IsDeferredRead)
                                {
                                    result.Append(tu.CleanForEmit(col.TheValue));
                                }
                                else
                                {
                                    // deferred read, just print the location
                                    result.Append(col.FetchDeferredLocation());
                                    
                                }
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
                            // Travis.Frisinger 04.02.2013
                            if(!val.IsDeferredRead)
                            {
                                if (!entry.IsManagmentServicePayload)
                                {
                                    result.Append(tu.CleanForEmit(val.TheValue));
                                }
                                else
                                {
                                    result.Append(val.TheValue); // avoid breaking WF Xaml
                                }
                            }
                            else
                            {
                                // deferred read, just print the location
                                result.Append(val.FetchDeferredLocation());
;                           }
                            result.Append("</");
                            result.Append(fName);
                            result.Append(">");
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
            string error;
            IBinaryDataList result = new BinaryDataList();

            ErrorResultTO invokeErrors;
            TranslatorUtils tu = new TranslatorUtils();

            // build shape
            if (targetShape == null)
            {
                errors.AddError("Null payload or shape");
            }
            else
            {
                result = tu.TranslateShapeToObject(targetShape, false, out invokeErrors);
                errors.MergeErrors(invokeErrors);

                // populate the shape 
                if (payload != string.Empty) {
                    try {
                        string toLoad = DataListUtil.StripCrap(payload); // clean up the rubish ;)
                        XmlDocument xDoc = new XmlDocument();
                        if (DataListUtil.IsXml(toLoad)) xDoc.LoadXml(toLoad);
                        else // Append new root tags ;)
                        {
                            toLoad = "<root>" + toLoad + "</root>";
                            xDoc.LoadXml(toLoad);
                        }
                        if(xDoc.DocumentElement != null)
                        {
                        XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                        IDictionary<string, int> indexCache = new Dictionary<string, int>();


                            IBinaryDataListEntry entry = null;
                            int idx = 1; // recset index

                            // spin through each element in the XML
                            foreach (XmlNode c in children) {
                                if (!DataListUtil.IsSystemTag(c.Name) && c.Name != GlobalConstants.NaughtyTextNode) {
                                    // scalars and recordset fetch
                                    if (result.TryGetEntry(c.Name, out entry, out error)) {
                                        if (entry.IsRecordset) {
                                            // fetch recordset index
                                            int fetchIdx = 0;
                                            if (indexCache.TryGetValue(c.Name, out fetchIdx)) {
                                                idx = fetchIdx;
                                            } else {
                                                idx = 1; //re-set idx on cache miss ;)
                                            }
                                            // process recordset
                                            XmlNodeList nl = c.ChildNodes;
                                            if (nl != null) {
                                                foreach (XmlNode subc in nl) {
                                                    entry.TryPutRecordItemAtIndex(Dev2BinaryDataListFactory.CreateBinaryItem(subc.InnerXml, c.Name, subc.Name, idx), idx, out error);

                                                    if (!string.IsNullOrEmpty(error)) {
                                                        errors.AddError(error);
                                                    }
                                                }
                                                // update this recordset index
                                                indexCache[c.Name] = ++idx;
                                            }

                                        } else {
                                            // process scalar
                                            entry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(c.InnerXml, c.Name), out error);

                                            if (!string.IsNullOrEmpty(error)) {
                                                errors.AddError(error);
                                            }
                                        }
                                    } else {
                                        errors.AddError(error);
                                        entry = null;
                                    }
                                }
                            }

                        }

                        // Transfer System Tags
                        IBinaryDataListEntry sysEntry;
                        for (int i = 0; i < TranslationConstants.systemTags.Length; i++) {
                            string key = TranslationConstants.systemTags.GetValue(i).ToString();
                            string query = String.Concat("//", key);
                            XmlNode n = xDoc.SelectSingleNode(query);

                            // try system namespace tags ;)
                            if (n == null)
                            {
                                query = String.Concat("//" + DataListUtil.BuildSystemTagForDataList(key, false));
                                n = xDoc.SelectSingleNode(query);
                            }

                            if (n != null && !string.IsNullOrEmpty(n.InnerXml)) {
                                string bkey = DataListUtil.BuildSystemTagForDataList(key, false);
                                if (result.TryGetEntry(bkey, out sysEntry, out error)) {
                                    sysEntry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(n.InnerXml, bkey), out error);
                                }
                            }
                        }
                    } catch (Exception e) {
                        // if use passed in empty input they only wanted the shape ;)
                        if (input.Length > 0) {
                            errors.AddError(e.Message);
                        }
                    }
                }
            }

            return result;
            
        }

        public IBinaryDataList ConvertTo(object input, string shape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public Guid Populate(object input, Guid targetDL, string outputDefs, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public string ConvertAndFilter(IBinaryDataList input, string filterShape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors)
        {
            errors = null;
            throw new NotImplementedException();
        }
    }
}
