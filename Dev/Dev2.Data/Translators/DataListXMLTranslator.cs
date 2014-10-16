
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
using Dev2.Data.Translators;
using Dev2.Data.Util;
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
        const string RootTag = "DataList";
        private readonly DataListFormat _format;
        private readonly Encoding _encoding;


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
                throw new ArgumentNullException("payload");
            }

            TranslatorUtils tu = new TranslatorUtils();

            StringBuilder result = new StringBuilder("<" + RootTag + ">");
            errors = new ErrorResultTO();

            var itemKeys = payload.FetchAllKeys();

            foreach(string key in itemKeys)
            {
                IBinaryDataListEntry entry;
                string error;
                if(payload.TryGetEntry(key, out entry, out error))
                {

                    if(entry.IsRecordset)
                    {
                        var idxItr = entry.FetchRecordsetIndexes();

                        while(idxItr.HasMore() && !entry.IsEmpty())
                        {

                            while(idxItr.HasMore())
                            {

                                int i = idxItr.FetchNextIndex();

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
                                        try
                                        {
                                            result.Append(tu.CleanForEmit(col.TheValue));
                                        }
                                        catch(Exception e)
                                        {
                                            Dev2Logger.Log.Error(e);
                                        }
                                    }
                                    else
                                    {
                                        // deferred read, just print the location
                                        result.Append(!string.IsNullOrEmpty(col.TheValue) ? col.FetchDeferredLocation() : string.Empty);
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
                                try
                                {
                                    result.Append(!entry.IsManagmentServicePayload ? tu.CleanForEmit(val.TheValue) : val.TheValue);
                                }
                                catch(Exception e)
                                {
                                    Dev2Logger.Log.Error(e);
                                }

                            }
                            else
                            {
                                // deferred read, just print the location
                                result.Append(val.FetchDeferredLocation());
                            }
                            result.Append("</");
                            result.Append(fName);
                            result.Append(">");
                        }
                    }
                }

            }

            result.Append("</" + RootTag + ">");

            DataListTranslatedPayloadTO tmp = new DataListTranslatedPayloadTO(result.ToString());

            return tmp;
        }

        // api level
        public IBinaryDataList ConvertTo(byte[] input, StringBuilder targetShape, out ErrorResultTO errors)
        {
            return ConvertTo(input, targetShape, out errors, false);
        }


        /// <summary>
        /// Converts the and only map inputs.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IBinaryDataList ConvertAndOnlyMapInputs(byte[] input, StringBuilder shape, out ErrorResultTO errors)
        {
            return ConvertTo(input, shape, out errors, true);
        }

        // internal to impl api methods
        private IBinaryDataList ConvertTo(byte[] input, StringBuilder targetShape, out ErrorResultTO errors, bool onlyMapInputs)
        {
            errors = new ErrorResultTO();
            string payload = Encoding.UTF8.GetString(input);

            IBinaryDataList result = new BinaryDataList();
            TranslatorUtils tu = new TranslatorUtils();

            // build shape
            if(targetShape == null)
            {
                errors.AddError("Null payload or shape");
            }
            else
            {
                ErrorResultTO invokeErrors;
                result = tu.TranslateShapeToObject(targetShape, true, out invokeErrors);
                errors.MergeErrors(invokeErrors);

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
                            TryConvert(children, result, indexCache, errors, onlyMapInputs);
                        }

                        // Transfer System Tags
                        for(int i = 0; i < TranslationConstants.systemTags.Length; i++)
                        {
                            string key = TranslationConstants.systemTags.GetValue(i).ToString();
                            string query = String.Concat("//", key);
                            XmlNode n = xDoc.SelectSingleNode(query);

                            // try system namespace tags ;)
                            if(n == null)
                            {
                                var values = "//" + DataListUtil.BuildSystemTagForDataList(key, false);
                                query = values;
                                n = xDoc.SelectSingleNode(query);
                            }

                            if(n != null && !string.IsNullOrEmpty(n.InnerXml))
                            {
                                string bkey = DataListUtil.BuildSystemTagForDataList(key, false);
                                string error;
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

        public IBinaryDataList ConvertTo(object input, StringBuilder shape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }


        public Guid Populate(object input, Guid targetDl, string outputDefs, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        static bool CanMapValue(bool onlyMapInputs, enDev2ColumnArgumentDirection direction)
        {
            if(!onlyMapInputs)
            {
                return true;
            }

            return (direction == enDev2ColumnArgumentDirection.Input || direction == enDev2ColumnArgumentDirection.Both);
        }

        // BUG 9626 - 2013.06.11 - TWR: refactored for recursion
        static void TryConvert(XmlNodeList children, IBinaryDataList result, IDictionary<string, int> indexCache, ErrorResultTO errors, bool onlyMapInputs, int level = 0)
        {
            // spin through each element in the XML
            foreach(XmlNode c in children)
            {
                if(!DataListUtil.IsSystemTag(c.Name) && c.Name != GlobalConstants.NaughtyTextNode)
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
                            foreach(XmlNode subc in nl)
                            {
                                // Extract column being mapped to ;)
                                var theCol = entry.Columns.FirstOrDefault(col => col.ColumnName == subc.Name);
                                var dir = enDev2ColumnArgumentDirection.None;
                                if(theCol != null)
                                {
                                    dir = theCol.ColumnIODirection;
                                }

                                if(CanMapValue(onlyMapInputs, dir))
                                {
                                    entry.TryPutRecordItemAtIndex(Dev2BinaryDataListFactory.CreateBinaryItem(subc.InnerXml, c.Name, subc.Name, idx), idx, out error);
                                }

                                errors.AddError(error);
                            }
                            // update this recordset index
                            indexCache[c.Name] = ++idx;
                        }
                        else if(CanMapValue(onlyMapInputs, entry.ColumnIODirection))
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
                            TryConvert(c.ChildNodes, result, indexCache, errors, onlyMapInputs, ++level);
                        }
                        else
                        {
                            errors.AddError(error);
                        }
                    }
                }
            }
        }

        public StringBuilder ConvertAndFilter(IBinaryDataList input, StringBuilder filterShape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions)
        {
            errors = null;
            throw new NotImplementedException();
        }
    }
}
