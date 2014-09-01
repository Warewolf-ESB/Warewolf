using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.DataList.Contract.Translators;

// ReSharper disable once CheckNamespace
namespace Dev2.Server.DataList.Translators
{
    internal sealed class StudioDataListXMLTranslator : IDataListTranslator
    {
        private const string RootTag = "DataList";

        // Attribute Constants for the Target XML format
        private const string IsEditable = "IsEditable";
        private const string Description = "Description";

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
            if(payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            StringBuilder result = new StringBuilder("<" + RootTag + ">");
            errors = new ErrorResultTO();

            var entries = payload.FetchAllEntries();

            foreach(var entry in entries)
            {
                if(entry.IsRecordset)
                {
                    AddEntryToBuilder(result, entry);
                    result.Append(">");
                    foreach(var col in entry.Columns)
                    {
                        result.Append("<");
                        result.Append(col.ColumnName);
                        result.Append(" " + Description + "=\"");
                        result.Append(col.ColumnDescription);
                        result.Append("\" ");
                        result.Append(IsEditable + "=\"");
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
                    AddEntryToBuilder(result, entry);
                    result.Append("/>");
                }
            }

            result.Append("</" + RootTag + ">");

            DataListTranslatedPayloadTO tmp = new DataListTranslatedPayloadTO(result.ToString());

            return tmp;
        }

        static void AddEntryToBuilder(StringBuilder result, IBinaryDataListEntry entry)
        {
            result.Append("<");
            result.Append(entry.Namespace);
            result.Append(" " + Description + "=\"");
            result.Append(entry.Description);
            result.Append("\" ");
            result.Append(IsEditable + "=\"");
            result.Append(entry.IsEditable);
            result.Append("\" ");
            // Travis.Frisinger - Added Column direction
            result.Append(GlobalConstants.DataListIoColDirection + "=\"");
            result.Append(entry.ColumnIODirection);
            result.Append("\" ");

        }

        public IBinaryDataList ConvertTo(byte[] input, string targetShape, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var payload = Encoding.UTF8.GetString(input);

            IBinaryDataList result = null;

            // build shape
            if(String.IsNullOrEmpty(targetShape))
            {
                errors.AddError("Null payload shape");
            }
            else
            {
                string error;
                result = BuildTargetShape(targetShape, out error);
                if(!string.IsNullOrEmpty(error))
                {
                    errors.AddError(error);
                }

                // populate the shape 
                if(payload != string.Empty)
                {
                    try
                    {
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(payload);
                        if(xDoc.DocumentElement != null)
                        {
                            var children = xDoc.DocumentElement.ChildNodes;

                            IDictionary<string, int> indexCache = new Dictionary<string, int>();

                            {
                                // spin through each element in the XML
                                foreach(XmlNode c in children)
                                {
                                    if(!DataListUtil.IsSystemTag(c.Name))
                                    {
                                        // scalars and recordset fetch
                                        IBinaryDataListEntry entry;
                                        if(result.TryGetEntry(c.Name, out entry, out error))
                                        {
                                            if(entry.IsRecordset)
                                            {
                                                // fetch recordset index
                                                int fetchIdx;
                                                int idx; // recset index
                                                if(indexCache.TryGetValue(c.Name, out fetchIdx))
                                                {
                                                    idx = fetchIdx;
                                                }
                                                else
                                                {
                                                    // 28-02-2013 - Sashen.Naidoo
                                                    // BUG 9144
                                                    // A cache miss does not necessary mean there is nothing in the record set,
                                                    // it just means the value isn't in the record set.
                                                    idx = indexCache.Count == 0 ? 1 : indexCache.Count;

                                                }
                                                // process recordset
                                                var nl = c.ChildNodes;
                                                foreach(XmlNode subc in nl)
                                                {
                                                    entry.TryPutRecordItemAtIndex(Dev2BinaryDataListFactory.CreateBinaryItem(subc.InnerXml, c.Name, subc.Name, idx), idx, out error);

                                                    if(!string.IsNullOrEmpty(error))
                                                    {
                                                        errors.AddError(error);
                                                    }
                                                    // update this recordset index

                                                }
                                                indexCache[c.Name] = ++idx;
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
                                            errors.AddError(error);
                                        }
                                    }
                                }

                            }
                        }

                        // Transfer System Tags
                        for(var i = 0; i < TranslationConstants.systemTags.Length; i++)
                        {
                            var key = TranslationConstants.systemTags.GetValue(i).ToString();
                            var query = String.Concat("//", key);
                            var n = xDoc.SelectSingleNode(query);

                            if(n != null && !string.IsNullOrEmpty(n.InnerXml))
                            {
                                var bkey = GlobalConstants.SystemTagNamespace + "." + key;
                                IBinaryDataListEntry sysEntry;
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

        /// <summary>
        /// Converts the and only map inputs.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IBinaryDataList ConvertAndOnlyMapInputs(byte[] input, string shape, out ErrorResultTO errors)
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

        #region Private Methods

        /// <summary>
        /// Build the template based upon the sent shape
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="error"></param>
        private IBinaryDataList BuildTargetShape(string shape, out string error)
        {
            IBinaryDataList result = null;
            error = null;
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(shape);
                if(xDoc.DocumentElement != null)
                {
                    var children = xDoc.DocumentElement.ChildNodes;
                    error = string.Empty;

                    HashSet<string> procssesNamespaces = new HashSet<string>();

                    result = Dev2BinaryDataListFactory.CreateDataList();

                    foreach(XmlNode c in children)
                    {
                        if(!DataListUtil.IsSystemTag(c.Name))
                        {
                            if(c.HasChildNodes)
                            {
                                IList<Dev2Column> cols = new List<Dev2Column>();
                                // build template for Recordset
                                if(!procssesNamespaces.Contains(c.Name))
                                {
                                    // build columns
                                    foreach(XmlNode subc in c.ChildNodes)
                                    {
                                        // It is possible for the .Attributes property to be null, a check should be added
                                        if(subc.Attributes != null)
                                        {
                                            cols.Add(DataListFactory.CreateDev2Column(subc.Name,
                                                ParseDescription(subc.Attributes[Description]),
                                                ParseIsEditable(subc.Attributes[IsEditable]),
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

                                    if(c.Attributes != null)
                                    {
                                        if(!result.TryCreateRecordsetTemplate(c.Name,
                                            ParseDescription(c.Attributes[Description]),
                                            cols,
                                            true,
                                            ParseIsEditable(c.Attributes[IsEditable]),
                                            ParseColumnIODirection(c.Attributes[GlobalConstants.DataListIoColDirection]),
                                            out myError))
                                        {
                                            error = myError;
                                        }

                                    }
                                    else
                                    {
                                        if(!result.TryCreateRecordsetTemplate(c.Name,
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
                                if(c.Attributes != null)
                                {
                                    result.TryCreateScalarTemplate(string.Empty,
                                        c.Name,
                                        ParseDescription(c.Attributes[Description]),
                                        true,
                                        ParseIsEditable(c.Attributes[IsEditable]),
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
            catch(Exception e)
            {
                error = e.Message;
            }
            return result;
        }

        private string ParseDescription(XmlAttribute attr)
        {
            var result = string.Empty;
            if(attr != null)
            {
                result = attr.Value;
            }
            return result;
        }

        private bool ParseIsEditable(XmlAttribute attr)
        {
            var result = true;
            if(attr != null)
            {
                Boolean.TryParse(attr.Value, out result);
            }

            return result;
        }

        // ReSharper disable InconsistentNaming
        private enDev2ColumnArgumentDirection ParseColumnIODirection(XmlAttribute attr)
        // ReSharper restore InconsistentNaming
        {
            enDev2ColumnArgumentDirection result = enDev2ColumnArgumentDirection.None;

            if(attr == null)
            {
                return result;
            }
            if(!Enum.TryParse(attr.Value, true, out result))
            {
                result = enDev2ColumnArgumentDirection.None;
            }
            return result;
        }

        #endregion
    }
}
