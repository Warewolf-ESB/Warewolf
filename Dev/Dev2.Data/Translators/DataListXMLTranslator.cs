using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.DataList.Contract.Translators;

// ReSharper disable CheckNamespace
namespace Dev2.Server.DataList.Translators
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// The standard DataList translator used in the system
    /// </summary>
    internal sealed class DataListXMLTranslator : IDataListTranslator
    {
        private static readonly string _rootTag = "DataList";
        private DataListFormat _format;
        private Encoding _encoding;


        public DataListFormat Format { get { return _format; } }
        public Encoding TextEncoding { get { return _encoding; } }

        public DataListXMLTranslator()
        {
            _format = DataListFormat.CreateFormat(GlobalConstants._XML);
            _encoding = Encoding.UTF8;
        }

        public DataListFormat HandlesType()
        {
            return _format;
        }

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList payload, out ErrorResultTO errors)
        {
            if(payload == null)
            {
                throw new ArgumentNullException("input");
            }

            StringBuilder result = new StringBuilder("<" + _rootTag + ">");
            errors = new ErrorResultTO();
            string error = string.Empty;

            IList<string> itemKeys = payload.FetchAllKeys();

            foreach(string key in itemKeys)
            {
                IBinaryDataListEntry entry = null;
                if(payload.TryGetEntry(key, out entry, out error))
                {

                    if(entry.IsRecordset)
                    {
                        IIndexIterator idxItr = entry.FetchRecordsetIndexes();

                        // &amp;amp;
                        int i;

                        while(idxItr.HasMore() && !entry.IsEmpty())
                        {

                            while(idxItr.HasMore())
                            {

                                i = idxItr.FetchNextIndex();

                                IList<IBinaryDataListItem> rowData = entry.FetchRecordAt(i, out error);
                                errors.AddError(error);
                                result.Append("<");
                                result.Append(entry.Namespace);
                                result.Append(">");

                                foreach(IBinaryDataListItem col in rowData)
                                {
                                    string fName = col.FieldName;

                                    result.Append("<");
                                    result.Append(fName);
                                    result.Append(">");

                                    // Travis.Frisinger 04.02.2013
                                    if(!col.IsDeferredRead)
                                    {
                                        result.Append(CleanForEmit(col.TheValue));
                                    }
                                    else
                                    {
                                        // deferred read, just print the location
                                        if(!string.IsNullOrEmpty(col.TheValue))
                                        {
                                            result.Append(col.FetchDeferredLocation());
                                        }
                                        else
                                        {
                                            result.Append(string.Empty);
                                        }


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
                    }
                    else
                    {
                        string fName = entry.Namespace;
                        IBinaryDataListItem val = entry.FetchScalar();
                        if(val != null)
                        {
                            result.Append("<");
                            result.Append(fName);
                            result.Append(">");
                            // Travis.Frisinger 04.02.2013
                            if(!val.IsDeferredRead)
                            {
                                // Dev2System.FormView is our html region, pass it by ;)
                                if(!entry.IsManagmentServicePayload && !entry.Namespace.Equals("Dev2System.FormView"))
                                {
                                    result.Append(CleanForEmit(val.TheValue));
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
                                ;
                            }
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

            // build shape
            if(targetShape == null)
            {
                errors.AddError("Null payload or shape");
            }
            else
            {
                result = BuildTargetShape(targetShape, out error);
                errors.AddError(error);

                // populate the shape 
                if(payload != string.Empty)
                {
                    try
                    {
                        string toLoad = DataListUtil.StripCrap(payload); // clean up the rubish ;)
                        XmlDocument xDoc = new XmlDocument();

                        // BUG 9626 - 2013.06.11 - TWR: ensure our DocumentElement
                        toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
                        xDoc.LoadXml(toLoad);

                        if(xDoc.DocumentElement != null)
                        {
                            XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                            IDictionary<string, int> indexCache = new Dictionary<string, int>();

                            // BUG 9626 - 2013.06.11 - TWR: refactored for recursion
                            TryConvert(children, result, indexCache, errors);
                        }

                        // Transfer System Tags
                        IBinaryDataListEntry sysEntry;
                        for(int i = 0; i < TranslationConstants.systemTags.Length; i++)
                        {
                            string key = TranslationConstants.systemTags.GetValue(i).ToString();
                            string query = String.Concat("//", key);
                            XmlNode n = xDoc.SelectSingleNode(query);

                            // try system namespace tags ;)
                            if(n == null)
                            {
                                query = String.Concat("//" + DataListUtil.BuildSystemTagForDataList(key, false));
                                n = xDoc.SelectSingleNode(query);
                            }

                            if(n != null)
                            {
                                string bkey = DataListUtil.BuildSystemTagForDataList(key, false);
                                if(result.TryGetEntry(bkey, out sysEntry, out error))
                                {
                                    sysEntry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(n.InnerXml, bkey), out error);
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        // if use passed in empty input they only wanted the shape ;)
                        if(input.Length > 0)
                        {
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

        // BUG 9626 - 2013.06.11 - TWR: refectored for recursion
        static void TryConvert(XmlNodeList children, IBinaryDataList result, IDictionary<string, int> indexCache, ErrorResultTO errors, int level = 0)
        {
            // spin through each element in the XML
            foreach(XmlNode c in children)
            {
                if(!DataListUtil.isSystemTag(c.Name) && c.Name != GlobalConstants.NaughtyTextNode)
                {
                    // scalars and recordset fetch
                    IBinaryDataListEntry entry;
                    string error;
                    if(result.TryGetEntry(c.Name, out entry, out error))
                    {
                        if(entry.IsRecordset)
                        {
                            // fetch recordset index
                            int fetchIdx;
                            var idx = indexCache.TryGetValue(c.Name, out fetchIdx) ? fetchIdx : 1;
                            // process recordset
                            var nl = c.ChildNodes;
                            if(nl != null)
                            {
                                foreach(XmlNode subc in nl)
                                {
                                    entry.TryPutRecordItemAtIndex(Dev2BinaryDataListFactory.CreateBinaryItem(subc.InnerXml, c.Name, subc.Name, idx), idx, out error);

                                    if(!string.IsNullOrEmpty(error))
                                    {
                                        errors.AddError(error);
                                    }
                                }
                                // update this recordset index
                                indexCache[c.Name] = ++idx;
                            }
                        }
                        else
                        {
                            // process scalar
                            entry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(c.InnerXml, c.Name), out error);

                            if(!string.IsNullOrEmpty(error))
                            {
                                errors.AddError(error);
                            }
                        }
                    }
                    else
                    {
                        if(level == 0)
                        {
                            // Only recurse if we're at the first level!!
                            TryConvert(c.ChildNodes, result, indexCache, errors, ++level);
                        }
                        else
                        {
                            errors.AddError(error);
                        }
                    }
                }
            }
        }

        #region Private Methods


        private string CleanForEmit(string val)
        {
            // Escape all of the following 
            // '      &apos;
            // "      &quot;
            // <      &lt;
            // >      &gt;
            // &      &amp;

            return val.Replace("&", "&amp;"); 
        }

        /// <summary>
        /// Build the template based upon the sent shape
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="error"></param>
        private IBinaryDataList BuildTargetShape(string shape, out string error)
        {
            IBinaryDataList result = null;
            error = string.Empty;
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(shape);
                if(xDoc.DocumentElement != null)
                {
                    XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                    error = string.Empty;

                    HashSet<string> procssesNamespaces = new HashSet<string>();

                    result = Dev2BinaryDataListFactory.CreateDataList();

                    foreach(XmlNode c in children)
                    {
                        XmlAttribute descAttribute = null;
                        if(!DataListUtil.isSystemTag(c.Name))
                        {
                            if(c.HasChildNodes)
                            {
                                IList<Dev2Column> cols = new List<Dev2Column>();
                                //recordset
                                if(c.ChildNodes != null)
                                {
                                    // build template
                                    if(!procssesNamespaces.Contains(c.Name))
                                    {
                                        // build columns
                                        foreach(XmlNode subc in c.ChildNodes)
                                        {
                                            if(subc.Attributes != null)
                                            {
                                                descAttribute = subc.Attributes["Description"];
                                            }

                                            if(descAttribute != null)
                                            {
                                                cols.Add(DataListFactory.CreateDev2Column(subc.Name, descAttribute.Value));
                                            }
                                            else
                                            {
                                                cols.Add(DataListFactory.CreateDev2Column(subc.Name, string.Empty));
                                            }
                                        }
                                        string myError;

                                        if(c.Attributes != null)
                                        {
                                            descAttribute = c.Attributes["Description"];
                                        }

                                        if(descAttribute != null)
                                        {
                                            if(!result.TryCreateRecordsetTemplate(c.Name, descAttribute.Value, cols, true, out myError))
                                            {
                                                error = myError;
                                            }
                                        }
                                        else
                                        {
                                            if(!result.TryCreateRecordsetTemplate(c.Name, string.Empty, cols, true, out myError))
                                            {
                                                error = myError;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //scalar
                                if(c.Attributes != null)
                                {
                                    descAttribute = c.Attributes["Description"];
                                }

                                if(descAttribute != null)
                                {
                                    result.TryCreateScalarTemplate(string.Empty, c.Name, descAttribute.Value, true, out error);
                                }
                                else
                                {
                                    result.TryCreateScalarTemplate(string.Empty, c.Name, string.Empty, true, out error);
                                }
                            }
                        }
                    }

                }

                // Build System Tag Shape ;)
                for(int i = 0; i < TranslationConstants.systemTags.Length; i++)
                {
                    if(result != null)
                    {
                        result.TryCreateScalarTemplate(GlobalConstants.SystemTagNamespace,
                                                    TranslationConstants.systemTags.GetValue(i).ToString(),
                                                    string.Empty,
                                                    true,
                                                    out error);
                    }
                }
            }
            catch(Exception e)
            {
                error = e.Message;
            }

            return result;
        }
        #endregion

        public string ConvertAndFilter(IBinaryDataList input, string filterShape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

    }
}
