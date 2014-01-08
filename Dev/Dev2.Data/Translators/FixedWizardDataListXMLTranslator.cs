using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.DataList.Contract.Translators;

namespace Dev2.Server.DataList.Translators
{
    internal sealed class FixedWizardDataListXMLTranslator : IDataListTranslator
    {
        private static readonly string _rootTag = "DataList";
        private DataListFormat _format;
        private Encoding _encoding;
        private static readonly string _lockedAttribute = " IsEditable=\"False\" ";

        public DataListFormat Format { get { return _format; } }
        public Encoding TextEncoding { get { return _encoding; } }

        public FixedWizardDataListXMLTranslator()
        {
            _format = DataListFormat.CreateFormat(GlobalConstants._FIXED_WIZARD);
            _encoding = Encoding.UTF8;
        }

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList payload, out ErrorResultTO errors)
        {
            if(payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            StringBuilder result = new StringBuilder("<" + _rootTag + ">");
            errors = new ErrorResultTO();
            string error = string.Empty;

            IList<string> itemKeys = payload.FetchAllUserKeys();

            foreach(string key in itemKeys)
            {
                IBinaryDataListEntry entry = null;
                if(payload.TryGetEntry(key, out entry, out error))
                {

                    if(entry.IsRecordset)
                    {
                        int cnt = entry.FetchLastRecordsetIndex();
                        for(int i = 1; i <= cnt; i++)
                        {
                            IList<IBinaryDataListItem> rowData = entry.FetchRecordAt(i, out error);
                            if(error != string.Empty)
                            {
                                errors.AddError(error);
                            }
                            result.Append("<");

                            result.Append(entry.Namespace);
                            // Append attribute
                            result.Append(_lockedAttribute);
                            result.Append(">");

                            foreach(IBinaryDataListItem col in rowData)
                            {
                                string fName = col.FieldName;

                                result.Append("<");
                                result.Append(fName);
                                // Append attribute
                                result.Append(_lockedAttribute);
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
                        if(val != null)
                        {
                            result.Append("<");
                            result.Append(fName);
                            // Append attribute
                            result.Append(_lockedAttribute);
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

            DataListTranslatedPayloadTO tmp = new DataListTranslatedPayloadTO(result.ToString());

            return tmp;
        }

        public IBinaryDataList ConvertTo(byte[] input, string targetShape, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            string payload = Encoding.UTF8.GetString(input);
            string error = string.Empty;

            IBinaryDataList result = null;

            // build shape
            if(payload == null || targetShape == null)
            {
                errors.AddError("Null payload or shape");
            }
            else
            {
                result = BuildTargetShape(targetShape, out error);
                if(error != null && error != string.Empty)
                {
                    errors.AddError(error);
                }

                // populate the shape 
                if(payload != string.Empty)
                {
                    try
                    {
                        string toLoad = DataListUtil.StripCrap(payload); // clean up the rubish ;)
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(toLoad);
                        XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                        HashSet<string> procssesNamespaces = new HashSet<string>();
                        IDictionary<string, int> indexCache = new Dictionary<string, int>();

                        if(children != null)
                        {
                            IBinaryDataListEntry entry = null;
                            int idx = 1; // recset index

                            // spin through each element in the XML
                            foreach(XmlNode c in children)
                            {
                                if(!DataListUtil.IsSystemTag(c.Name))
                                {
                                    // scalars and recordset fetch
                                    if(result.TryGetEntry(c.Name, out entry, out error))
                                    {
                                        if(entry.IsRecordset)
                                        {
                                            // fetch recordset index
                                            int fetchIdx = 0;
                                            if(indexCache.TryGetValue(c.Name, out fetchIdx))
                                            {
                                                idx = fetchIdx;
                                            }
                                            else
                                            {
                                                idx = 1; //re-set idx on cache miss ;)
                                            }
                                            // process recordset
                                            XmlNodeList nl = c.ChildNodes;
                                            if(nl != null)
                                            {
                                                foreach(XmlNode subc in nl)
                                                {
                                                    entry.TryPutRecordItemAtIndex(Dev2BinaryDataListFactory.CreateBinaryItem(subc.InnerXml, c.Name, subc.Name, (idx + "")), idx, out error);

                                                    if(error != null && error != string.Empty)
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

                                            if(error != null && error != string.Empty)
                                            {
                                                errors.AddError(error);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        errors.AddError(error);
                                        entry = null;
                                    }
                                }
                            }

                        }

                        // Transfer System Tags
                        IBinaryDataListEntry sysEntry;
                        for(int i = 0; i < TranslationConstants.systemTags.Length; i++)
                        {
                            string key = TranslationConstants.systemTags.GetValue(i).ToString(); ;
                            string query = String.Concat("//", key);
                            XmlNode n = xDoc.SelectSingleNode(query);

                            if(n != null && !string.IsNullOrEmpty(n.InnerXml))
                            {
                                string bkey = GlobalConstants.SystemTagNamespace + "." + key;
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

        public Guid Populate(object input, Guid targetDl, string outputDefs, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public string ConvertAndFilter(IBinaryDataList input, string filterShape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions)
        {
            errors = null;
            throw new NotImplementedException();
        }

        public DataListFormat HandlesType()
        {
            return _format;
        }


        #region Private Methods

        /// <summary>
        /// Build the template based upon the sent shape
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="error"></param>
        private IBinaryDataList BuildTargetShape(string shape, out string error)
        {
            IBinaryDataList result = null;
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(shape);
                XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                error = string.Empty;

                HashSet<string> procssesNamespaces = new HashSet<string>();

                if(children != null)
                {
                    result = Dev2BinaryDataListFactory.CreateDataList();

                    foreach(XmlNode c in children)
                    {
                        XmlAttribute descAttribute = null;
                        if(!DataListUtil.IsSystemTag(c.Name))
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
                                            // It is possible for the .Attributes property to be null, a check should be added
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
                                        string myError = string.Empty;
                                        // It is possible for the .Attributes property to be null, a check should be added
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
                                // It is possible for the .Attributes property to be null, a check should be added
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
                    result.TryCreateScalarTemplate(GlobalConstants.SystemTagNamespace,
                                                    TranslationConstants.systemTags.GetValue(i).ToString(),
                                                    string.Empty,
                                                    true,
                                                    out error);
                }
            }
            catch(Exception e)
            {
                error = e.Message;
            }

            return result;
        }
        #endregion



    }
}
