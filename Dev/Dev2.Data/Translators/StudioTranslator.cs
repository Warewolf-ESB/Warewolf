using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

// ReSharper disable CheckNamespace
namespace Dev2.Server.DataList.Translators
// ReSharper restore CheckNamespace
{
    internal abstract class StudioTranslator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        protected StudioTranslator(DataListFormat format, Encoding encoding)
        {
            Format = format;
            TextEncoding = encoding;
        }

        protected const string RootTag = "DataList";
        public DataListFormat Format { get; private set; }
        public Encoding TextEncoding { get; private set; }

        public DataListFormat HandlesType()
        {
            return Format;
        }

        internal static void DoScalarAppending(StringBuilder result, string fName, IBinaryDataListItem val)
        {


            result.Append("<");
            result.Append(fName);
            result.Append(">");
            try
            {
                result.Append(val.TheValue);
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
            }
            result.Append("</");
            result.Append(fName);
            result.Append(">");
        }

        internal static void DoRecordSetAppending(ErrorResultTO errors, IBinaryDataListEntry entry, StringBuilder result)
        {
            var cnt = entry.FetchLastRecordsetIndex();
            var cols = entry.Columns;
            if(!cols.Any(c => c.ColumnIODirection == enDev2ColumnArgumentDirection.Both || c.ColumnIODirection == enDev2ColumnArgumentDirection.Input))
            {
                return;
            }
            for(var i = 1; i <= cnt; i++)
            {
                string error;
                var rowData = entry.FetchRecordAt(i, out error);
                errors.AddError(error);

                result.Append("<");
                result.Append(entry.Namespace);
                result.Append(">");

                foreach(var col in rowData)
                {

                    var fName = col.FieldName;

                    if(cols.Any(c => c.ColumnName == fName &&
                        (c.ColumnIODirection == enDev2ColumnArgumentDirection.Both || c.ColumnIODirection == enDev2ColumnArgumentDirection.Input)))
                    {

                        result.Append("<");
                        result.Append(fName);
                        result.Append(">");
                        try
                        {
                            result.Append(col.TheValue);
                        }
                        catch(Exception e)
                        {
                            Dev2Logger.Log.Error(e);
                        }
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

        internal static IBinaryDataList BuildBinaryDataList(byte[] input, string targetShape, ErrorResultTO errors, string payload)
        {
            IBinaryDataList result = null;
            // build shape
            if(targetShape == null)
            {
                errors.AddError("Null payload or shape");
            }
            else
            {
                string error;
                result = BuildTargetShape(targetShape, out error);
                errors.AddError(error);

                // populate the shape
                if(payload != String.Empty)
                {
                    try
                    {
                        var toLoad = payload;
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

                        if(!String.IsNullOrEmpty(toLoad))
                        {
                            if(xDoc.DocumentElement != null)
                            {
                                XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                                IDictionary<string, int> indexCache = new Dictionary<string, int>();

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
                                    IBinaryDataListEntry entry;
                                    if(result.TryGetEntry(c.Name, out entry, out error))
                                    {
                                        if(entry.IsRecordset)
                                        {
                                            // fetch recordset index
                                            int fetchIdx;
                                            int idx = indexCache.TryGetValue(c.Name, out fetchIdx) ? fetchIdx : 1; // recset index
                                            // process recordset
                                            XmlNodeList nl = c.ChildNodes;
                                            foreach(XmlNode subc in nl)
                                            {
                                                entry.TryPutRecordItemAtIndex(Dev2BinaryDataListFactory.CreateBinaryItem(subc.InnerXml, c.Name, subc.Name, (idx + "")), idx, out error);
                                                errors.AddError(error);
                                            }
                                            // update this recordset index
                                            indexCache[c.Name] = ++idx;
                                        }
                                        else
                                        {
                                            // process scalar
                                            entry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(c.InnerXml, c.Name), out error);
                                            errors.AddError(error);
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

        /// <summary>
        /// Build the template based upon the sent shape
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        internal static IBinaryDataList BuildTargetShape(string shape, out string error)
        {
            IBinaryDataList result = null;
            error = null;
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(shape);
                if(xDoc.DocumentElement != null)
                {
                    XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                    error = String.Empty;

                    HashSet<string> procssesNamespaces = new HashSet<string>();

                    result = Dev2BinaryDataListFactory.CreateDataList();

                    foreach(XmlNode c in children)
                    {
                        XmlAttribute descAttribute = null;
                        XmlAttribute columnIoDirection = null;
                        if(!DataListUtil.IsSystemTag(c.Name))
                        {
                            if(c.HasChildNodes)
                            {
                                IList<Dev2Column> cols = new List<Dev2Column>();
                                //recordset
                                // build template
                                if(!procssesNamespaces.Contains(c.Name))
                                {
                                    // build columns
                                    var columnDirection = enDev2ColumnArgumentDirection.None;
                                    foreach(XmlNode subc in c.ChildNodes)
                                    {
                                        // It is possible for the .Attributes property to be null, a check should be added
                                        if(subc.Attributes != null)
                                        {
                                            descAttribute = subc.Attributes["Description"];
                                            columnIoDirection = subc.Attributes["ColumnIODirection"];
                                            if(columnIoDirection != null)
                                            {
                                                Enum.TryParse(columnIoDirection.Value, true, out columnDirection);
                                            }
                                        }

                                        cols.Add(descAttribute != null ? DataListFactory.CreateDev2Column(subc.Name, descAttribute.Value, true, columnDirection) : DataListFactory.CreateDev2Column(subc.Name, String.Empty, true, columnDirection));

                                    }
                                    string myError;
                                    // It is possible for the .Attributes property to be null, a check should be added
                                    if(c.Attributes != null)
                                    {
                                        descAttribute = c.Attributes["Description"];
                                        columnIoDirection = c.Attributes["ColumnIODirection"];
                                    }
                                    var descriptionValue = "";
                                    columnDirection = enDev2ColumnArgumentDirection.None;
                                    if(descAttribute != null)
                                    {
                                        descriptionValue = descAttribute.Value;
                                    }
                                    if(columnIoDirection != null)
                                    {
                                        Enum.TryParse(columnIoDirection.Value, true, out columnDirection);
                                    }
                                    if(!result.TryCreateRecordsetTemplate(c.Name, descriptionValue, cols, true, false, columnDirection, out myError))
                                    {
                                        error = myError;
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
                                    columnIoDirection = c.Attributes["ColumnIODirection"];
                                }
                                string descriptionValue = "";
                                var columnDirection = enDev2ColumnArgumentDirection.None;
                                if(descAttribute != null)
                                {
                                    descriptionValue = descAttribute.Value;
                                }
                                if(columnIoDirection != null)
                                {
                                    Enum.TryParse(columnIoDirection.Value, true, out columnDirection);
                                }
                                result.TryCreateScalarTemplate(String.Empty, c.Name, descriptionValue, true, true, columnDirection, out error);
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

        public string ConvertAndFilter(IBinaryDataList payload, string filterShape, out ErrorResultTO errors)
        {
            if(payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            StringBuilder result = new StringBuilder("<" + RootTag + ">");
            errors = new ErrorResultTO();
            string error;

            IBinaryDataList targetDl = BuildTargetShape(filterShape, out error);

            IList<string> itemKeys = targetDl.FetchAllKeys();

            foreach(string key in itemKeys)
            {
                IBinaryDataListEntry entry;
                IBinaryDataListEntry tmpEntry;
                if(payload.TryGetEntry(key, out entry, out error) && targetDl.TryGetEntry(key, out tmpEntry, out error))
                {

                    if(entry.IsRecordset)
                    {
                        DoRecordSetAppending(errors, entry, result);
                    }
                    else
                    {
                        string fName = entry.Namespace;
                        IBinaryDataListItem val = entry.FetchScalar();
                        if(val != null)
                        {
                            DoScalarAppending(result, fName, val);
                        }
                    }
                }

            }

            result.Append("</" + RootTag + ">");

            return result.ToString();
        }

        public IBinaryDataList ConvertTo(byte[] input, string targetShape, out ErrorResultTO errors, bool onlyMatchInputs = false)
        {
            errors = new ErrorResultTO();
            var payload = Encoding.UTF8.GetString(input);

            var result = BuildBinaryDataList(input, targetShape, errors, payload);

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

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions)
        {
            errors = null;
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
    }
}