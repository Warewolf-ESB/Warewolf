using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.Web;

namespace Dev2.Data.Translators
{
    internal sealed class DataListXMLTranslatorWithOutSystemTags : IDataListTranslator
    {
        const string _rootTag = "DataList";
        private readonly DataListFormat _format;
        private readonly Encoding _encoding;
        readonly TranslatorUtils _tu;

        public DataListFormat Format { get { return _format; } }
        public Encoding TextEncoding { get { return _encoding; } }

        public DataListXMLTranslatorWithOutSystemTags()
        {
            _format = DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags, EmitionTypes.XML, "text/xml");
            _encoding = Encoding.UTF8;
            _tu = new TranslatorUtils();
        }

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList payload, out ErrorResultTO errors)
        {
            if(payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            TranslatorUtils tu = new TranslatorUtils();

            StringBuilder result = new StringBuilder("<" + _rootTag + ">");
            errors = new ErrorResultTO();

            IList<string> itemKeys = payload.FetchAllUserKeys();

            foreach(string key in itemKeys)
            {
                IBinaryDataListEntry entry;
                string error;
                if(payload.TryGetEntry(key, out entry, out error))
                {

                    if(entry.IsRecordset && !entry.IsEmpty())
                    {

                        var idxItr = entry.FetchRecordsetIndexes();

                        while(idxItr.HasMore())
                        {
                            var i = idxItr.FetchNextIndex();
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
                                result.Append(tu.FullCleanForEmit(col.TheValue));
                                result.Append("</");
                                result.Append(fName);
                                result.Append(">");
                            }

                            result.Append("</");
                            result.Append(entry.Namespace);
                            result.Append(">");
                        }

                    }
                    else if(!entry.IsRecordset)
                    {
                        string fName = entry.Namespace;
                        IBinaryDataListItem val = entry.FetchScalar();
                        if(val != null)
                        {
                            result.Append("<");
                            result.Append(fName);
                            result.Append(">");
                            result.Append(tu.FullCleanForEmit(val.TheValue));
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

            IBinaryDataList result = null;

            // build shape
            if(targetShape == null)
            {
                errors.AddError("Null payload or shape");
            }
            else
            {
                ErrorResultTO invokeErrors;
                result = _tu.TranslateShapeToObject(targetShape, false, out invokeErrors);
                errors.MergeErrors(invokeErrors);

                // populate the shape 
                if(payload != string.Empty)
                {
                    try
                    {
                        string toLoad = DataListUtil.StripCrap(payload); // clean up the rubbish ;)
                        XmlDocument xDoc = new XmlDocument();
                        try
                        {
                            xDoc.LoadXml(toLoad);
                        }
                        catch
                        {
                            // Append new root tags ;)
                            toLoad = "<root>" + toLoad + "</root>";
                            xDoc.LoadXml(toLoad);
                        }

                        if(!string.IsNullOrEmpty(toLoad))
                        {
                            if(xDoc.DocumentElement != null)
                            {
                                XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                                IDictionary<string, int> indexCache = new Dictionary<string, int>();
                                IBinaryDataListEntry entry;

                                string error;
                                if(children.Count > 0 && !DataListUtil.IsMSXmlBugNode(children[0].Name))
                                {
                                    #region Process children

                                    // spin through each element in the XML
                                    foreach(XmlNode c in children)
                                    {

                                        var hasCorrectIoDirection = true;
                                        if(c.Attributes != null)
                                        {
                                            var columnIoDirectionAttribute = c.Attributes["ColumnIODirection"];
                                            if(columnIoDirectionAttribute != null)
                                            {
                                                var columnIoDirectionValue = columnIoDirectionAttribute.Value;
                                                var hasCorrectIoDirectionFromAttribute = columnIoDirectionValue == enDev2ColumnArgumentDirection.Output.ToString() || columnIoDirectionValue == enDev2ColumnArgumentDirection.Both.ToString();
                                                hasCorrectIoDirection = hasCorrectIoDirectionFromAttribute;
                                            }
                                        }

                                        if(DataListUtil.IsSystemTag(c.Name) && !hasCorrectIoDirection)
                                        {
                                            continue;
                                        }
                                        // scalars and recordset fetch
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
                                                    idx = 1; //re-set idx on cache miss ;)
                                                }
                                                // process recordset
                                                XmlNodeList nl = c.ChildNodes;
                                                foreach(XmlNode subc in nl)
                                                {
                                                    entry.TryPutRecordItemAtIndex(Dev2BinaryDataListFactory.CreateBinaryItem(subc.InnerXml, c.Name, subc.Name, (idx + "")), idx, out error);

                                                    if(!string.IsNullOrEmpty(error))
                                                    {
                                                        errors.AddError(error);
                                                    }
                                                }
                                                // update this recordset index
                                                indexCache[c.Name] = ++idx;
                                            }
                                            else
                                            {
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

                                    #endregion
                                }
                                else
                                {
                                    var c = xDoc.DocumentElement;
                                    if(result.TryGetEntry(c.Name, out entry, out error))
                                    {
                                        entry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(c.InnerXml, c.Name), out error);
                                        if(!string.IsNullOrEmpty(error))
                                        {
                                            errors.AddError(error);
                                        }
                                    }
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

        // NOTE : This will be tested by the related WebServices and Plugin Integration Test
        public Guid Populate(object input, Guid targetDl, string outputDefs, out ErrorResultTO errors)
        {
            // input is a string of output mappings ;)
            var compiler = DataListFactory.CreateDataListCompiler();
            var outputDefinitions = (input as string);
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            // get sneeky and use the output shape operation for now,
            // as this should only every be called from external service containers all is good
            // if this is ever not the case be afraid, be very afraid!

            var targetDataList = compiler.FetchBinaryDataList(targetDl, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            var parentDataList = compiler.FetchBinaryDataList(targetDataList.ParentUID, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            var grandparentDL = compiler.FetchBinaryDataList(parentDataList.ParentUID, out invokeErrors);

            // as a result we need to re-set some alias operations that took place in the parent DataList where they happended ;)
            foreach(var entry in parentDataList.FetchRecordsetEntries())
            {
                entry.AdjustAliasOperationForExternalServicePopulate();
            }

            var parentID = parentDataList.UID;
            if(grandparentDL != null)
            {
                parentID = grandparentDL.UID;
            }

            compiler.SetParentID(targetDl, parentID);
            Guid result = compiler.Shape(targetDl, enDev2ArgumentType.Output, outputDefinitions, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            return result;
        }

        public string ConvertAndFilter(IBinaryDataList payload, string filterShape, out ErrorResultTO errors)
        {
            if(payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            StringBuilder result = new StringBuilder("<" + _rootTag + ">");
            errors = new ErrorResultTO();

            ErrorResultTO invokeErrors;
            TranslatorUtils tu = new TranslatorUtils();

            IBinaryDataList targetDL = _tu.TranslateShapeToObject(filterShape, false, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            IList<string> itemKeys = targetDL.FetchAllKeys();

            foreach(string key in itemKeys)
            {
                IBinaryDataListEntry entry;
                IBinaryDataListEntry tmpEntry;
                string error;
                if(payload.TryGetEntry(key, out entry, out error) && targetDL.TryGetEntry(key, out tmpEntry, out error))
                {

                    if(entry.IsRecordset)
                    {
                        var idxItr = entry.FetchRecordsetIndexes();

                        while(idxItr.HasMore())
                        {
                            var i = idxItr.FetchNextIndex();

                            IList<IBinaryDataListItem> rowData = entry.FetchRecordAt(i, out error);
                            if(error != string.Empty)
                            {
                                errors.AddError(error);
                            }
                            result.Append("<");
                            result.Append(entry.Namespace);
                            result.Append(">");

                            foreach(IBinaryDataListItem col in rowData)
                            {
                                IBinaryDataListItem col1 = col;
                                if(tmpEntry.Columns.Any((c => c.ColumnName == col1.FieldName)))
                                {
                                    string fName = col.FieldName;

                                    result.Append("<");
                                    result.Append(fName);
                                    result.Append(">");
                                    result.Append(tu.FullCleanForEmit(col.TheValue));
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

                        if(val != null)
                        {
                            result.Append("<");
                            result.Append(fName);
                            result.Append(">");
                            if(entry.Namespace != GlobalConstants.ManagementServicePayload)
                            {
                                result.Append(tu.FullCleanForEmit(val.TheValue));
                            }
                            else
                            {
                                result.Append(val.TheValue);
                            }
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

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions)
        {
            errors = null;
            throw new NotImplementedException();
        }

        public DataListFormat HandlesType()
        {
            return _format;
        }

    }
}
