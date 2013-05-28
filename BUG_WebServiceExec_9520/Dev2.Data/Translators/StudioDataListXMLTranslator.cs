using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.DataList.Contract.Translators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Dev2.Server.DataList.Translators
{
    internal sealed class StudioDataListXMLTranslator : IDataListTranslator
    {
        private const string _rootTag = "DataList";

        // Attribute Constants for the Target XML format
        private const string _isEditable = "IsEditable";
        private const string _description = "Description";

        private readonly DataListFormat _format;
        private readonly Encoding _encoding;


        public DataListFormat Format { get { return _format; } }
        public Encoding TextEncoding { get { return _encoding; } }

        public StudioDataListXMLTranslator()
        {
            _format = DataListFormat.CreateFormat(GlobalConstants._Studio_XML);
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
                throw new ArgumentNullException("payload");
            }

            StringBuilder result = new StringBuilder("<" + _rootTag + ">");
            errors = new ErrorResultTO();
            string error = string.Empty;

            IList<IBinaryDataListEntry> entries = payload.FetchAllEntries();

            foreach (IBinaryDataListEntry entry in entries)
            {
                if (entry.IsRecordset)
                {
                    result.Append("<");
                    result.Append(entry.Namespace);
                    result.Append(" " + _description + "=\"");
                    result.Append(entry.Description);
                    result.Append("\" ");
                    result.Append(_isEditable + "=\"");
                    result.Append(entry.IsEditable);
                    result.Append("\" ");
                    // Travis.Frisinger - Added Column direction
                    result.Append(GlobalConstants.DataListIoColDirection + "=\"");
                    result.Append(entry.ColumnIODirection);
                    result.Append("\" ");
                    result.Append(">");

                    foreach (Dev2Column col in entry.Columns)
                    {
                        result.Append("<");
                        result.Append(col.ColumnName);
                        result.Append(" " + _description + "=\"");
                        result.Append(col.ColumnDescription);
                        result.Append("\" ");
                        result.Append(_isEditable + "=\"");
                        result.Append(col.IsEditable);
                        result.Append("\" ");
                        // Travis.Frisinger - Added Column direction
                        result.Append(GlobalConstants.DataListIoColDirection + "=\"");
                        result.Append(col.ColumnIODirection);
                        result.Append("\" ");
                        result.Append("/>");
                    }

                    result.Append("</");
                    result.Append(entry.Namespace);
                    result.Append(">");
                }
                else
                {
                    result.Append("<");
                    result.Append(entry.Namespace);
                    result.Append(" " + _description + "=\"");
                    result.Append(entry.Description);
                    result.Append("\" ");
                    result.Append(_isEditable + "=\"");
                    result.Append(entry.IsEditable);
                    result.Append("\" ");
                    // Travis.Frisinger - Added Column direction
                    result.Append(GlobalConstants.DataListIoColDirection + "=\"");
                    result.Append(entry.ColumnIODirection);
                    result.Append("\" ");
                    result.Append("/>");
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
            if (String.IsNullOrEmpty(targetShape))
            {
                errors.AddError("Null payload shape");
            }
            else
            {
                string error;
                result = BuildTargetShape(targetShape, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    errors.AddError(error);
                }

                // populate the shape 
                if (payload != string.Empty)
                {
                    try
                    {
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(payload);
                        XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                        IDictionary<string, int> indexCache = new Dictionary<string, int>();

                        if (children != null)
                        {
                            IBinaryDataListEntry entry = null;
                            int idx = 1; // recset index

                            // spin through each element in the XML
                            foreach (XmlNode c in children)
                            {
                                if (!DataListUtil.isSystemTag(c.Name))
                                {
                                    // scalars and recordset fetch
                                    if (result.TryGetEntry(c.Name, out entry, out error))
                                    {
                                        if (entry.IsRecordset)
                                        {
                                            // fetch recordset index
                                            int fetchIdx = 0;
                                            if (indexCache.TryGetValue(c.Name, out fetchIdx))
                                            {
                                                idx = fetchIdx;
                                            }
                                            else
                                            {
                                                // 28-02-2013 - Sashen.Naidoo
                                                // BUG 9144
                                                // A cache miss does not necessary mean there is nothing in the record set,
                                                // it just means the value isn't in the record set.
                                                if (indexCache.Count == 0)
                                                {
                                                    idx = 1;
                                                }
                                                else
                                                {
                                                    idx = indexCache.Count;
                                                }
                                                
                                                //idx = 1; //re-set idx on cache miss ;)
                                            }
                                            // process recordset
                                            XmlNodeList nl = c.ChildNodes;
                                            if (nl != null)
                                            {
                                                foreach (XmlNode subc in nl)
                                                {
                                                    entry.TryPutRecordItemAtIndex(Dev2BinaryDataListFactory.CreateBinaryItem(subc.InnerXml, c.Name, subc.Name, idx), idx, out error);

                                                    if (!string.IsNullOrEmpty(error))
                                                    {
                                                        errors.AddError(error);
                                                    }
                                                    // update this recordset index
                                                    
                                                }
                                                indexCache[c.Name] = ++idx;

                                            }

                                        }
                                        else
                                        {
                                            // process scalar
                                            entry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(c.InnerXml, c.Name), out error);

                                            if (!string.IsNullOrEmpty(error))
                                            {
                                                errors.AddError(error);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        errors.AddError(error);
                                    }
                                }
                            }

                        }

                        // Transfer System Tags
                        IBinaryDataListEntry sysEntry;
                        for (int i = 0; i < TranslationConstants.systemTags.Length; i++)
                        {
                            string key = TranslationConstants.systemTags.GetValue(i).ToString(); ;
                            string query = String.Concat("//", key);
                            XmlNode n = xDoc.SelectSingleNode(query);

                            if (n != null)
                            {
                                string bkey = GlobalConstants.SystemTagNamespace + "." + key;
                                if (result.TryGetEntry(bkey, out sysEntry, out error))
                                {
                                    sysEntry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(n.InnerXml, bkey), out error);
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

                if (children != null)
                {
                    result = Dev2BinaryDataListFactory.CreateDataList();

                    foreach (XmlNode c in children)
                    {
                        if (!DataListUtil.isSystemTag(c.Name))
                        {
                            if (c.HasChildNodes)
                            {
                                IList<Dev2Column> cols = new List<Dev2Column>();
                                // build template for Recordset
                                if (!procssesNamespaces.Contains(c.Name))
                                {
                                    // build columns
                                    foreach (XmlNode subc in c.ChildNodes)
                                    {
                                        // It is possible for the .Attributes property to be null, a check should be added
                                        if (subc.Attributes != null)
                                        {
                                            cols.Add(DataListFactory.CreateDev2Column(subc.Name,
                                                                                        ParseDescription(subc.Attributes[_description]),
                                                                                        ParseIsEditable(subc.Attributes[_isEditable]),
                                                                                        ParseColumnIODirection(subc.Attributes[GlobalConstants.DataListIoColDirection])));
                                        }
                                        else
                                        {
                                            cols.Add(DataListFactory.CreateDev2Column(subc.Name,
                                                                                        ParseDescription(null),
                                                                                        ParseIsEditable(null),
                                                                                        ParseColumnIODirection(null)));
                                        }
                                    }

                                    string myError;

                                    if (c.Attributes != null)
                                    {
                                        if (!result.TryCreateRecordsetTemplate(c.Name,
                                                                                ParseDescription(c.Attributes[_description]),
                                                                                cols,
                                                                                true,
                                                                                ParseIsEditable(c.Attributes[_isEditable]),
                                                                                ParseColumnIODirection(c.Attributes[GlobalConstants.DataListIoColDirection]),
                                                                                out myError))
                                        {
                                            error = myError;
                                        }

                                    }
                                    else
                                    {
                                        if (!result.TryCreateRecordsetTemplate(c.Name,
                                                                                ParseDescription(null),
                                                                                cols,
                                                                                true,
                                                                                ParseIsEditable(null),
                                                                                ParseColumnIODirection(null),
                                                                                out myError))
                                        {
                                            error = myError;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //scalar
                                if (c.Attributes != null)
                                {
                                    result.TryCreateScalarTemplate(string.Empty,
                                                                   c.Name,
                                                                   ParseDescription(c.Attributes[_description]),
                                                                   true,
                                                                   ParseIsEditable(c.Attributes[_isEditable]),
                                                                   ParseColumnIODirection(c.Attributes[GlobalConstants.DataListIoColDirection]),
                                                                   out error);
                                }
                                else
                                {
                                    result.TryCreateScalarTemplate(string.Empty,
                                                                   c.Name,
                                                                   ParseDescription(null),
                                                                   true,
                                                                   ParseIsEditable(null),
                                                                   ParseColumnIODirection(null),
                                                                   out error);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                error = e.Message;
            }

            return result;
        }

        private string ParseDescription(XmlAttribute attr)
        {
            string result = string.Empty;

            if (attr != null)
            {
                result = attr.Value;
            }

            return result;
        }

        private bool ParseIsEditable(XmlAttribute attr)
        {
            bool result = true;
            if (attr != null)
            {
                Boolean.TryParse(attr.Value, out result);
            }

            return result;
        }

        private enDev2ColumnArgumentDirection ParseColumnIODirection(XmlAttribute attr)
        {
            enDev2ColumnArgumentDirection result = enDev2ColumnArgumentDirection.None;

            if (attr != null)
            {
                if (!Enum.TryParse(attr.Value, true, out result))
                {
                    result = enDev2ColumnArgumentDirection.None;
                }
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
