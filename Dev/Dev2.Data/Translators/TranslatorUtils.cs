using System;
using System.Collections.Generic;
using System.Xml;
using Dev2.Common;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Translators;

namespace Dev2.Data.Translators
{
    /// <summary>
    /// Core utils for Data List Translators ;)
    /// </summary>
    public class TranslatorUtils
    {

        /// <summary>
        /// Cleans & for emit
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public string CleanForEmit(string val)
        {
            return val.Replace("&", "&amp;");
        }

        /// <summary>
        /// Full suite [ &, angle brackets, ', and " clean for emit.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public string FullCleanForEmit(string val)
        {
            return
                val.Replace("&", "&amp;")
                   .Replace("<", "&lt;")
                   .Replace(">", "&gt;")
                   .Replace("'", "&apos;")
                   .Replace("\"", "&quot;");
        }

        /// <summary>
        /// Translates the shape automatic object.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IBinaryDataList TranslateShapeToObject(string shape, bool includeSysTags, out ErrorResultTO errors)
        {
            IBinaryDataList result = null;
            errors = new ErrorResultTO(); 

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(shape);
                string error;
                if(xDoc.DocumentElement != null)
                {
                    XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                    HashSet<string> procssesNamespaces = new HashSet<string>();

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
                                    if (!procssesNamespaces.Contains(c.Name))
                                    {
                                        procssesNamespaces.Add(c.Name);

                                        // build columns
                                        foreach (XmlNode subc in c.ChildNodes)
                                        {
                                            if (subc.Attributes != null)
                                            {
                                                descAttribute = subc.Attributes["Description"];
                                            }

                                            if (descAttribute != null)
                                            {
                                                cols.Add(DataListFactory.CreateDev2Column(subc.Name, descAttribute.Value));
                                            }
                                            else
                                            {
                                                cols.Add(DataListFactory.CreateDev2Column(subc.Name, string.Empty));
                                            }
                                        }
                                        string myError;

                                        if (c.Attributes != null)
                                        {
                                            descAttribute = c.Attributes["Description"];
                                        }

                                        if (descAttribute != null)
                                        {
                                            if (!result.TryCreateRecordsetTemplate(c.Name, descAttribute.Value, cols, true, out myError))
                                            {
                                                errors.AddError(myError);
                                            }
                                        }
                                        else
                                        {
                                            if (!result.TryCreateRecordsetTemplate(c.Name, string.Empty, cols, true, out myError))
                                            {
                                                errors.AddError(myError);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // it is a duplicate we need to handle this correctly ;)
                                        // build columns

                                        IBinaryDataListEntry amendEntry;
                                        result.TryGetEntry(c.Name, out amendEntry, out error);
                                        errors.AddError(error);

                                        if (amendEntry != null)
                                        {
                                            cols = amendEntry.Columns;

                                            foreach (XmlNode subc in c.ChildNodes)
                                            {
                                                if (subc.Attributes != null)
                                                {
                                                    descAttribute = subc.Attributes["Description"];
                                                }

                                                if (descAttribute != null)
                                                {
                                                    cols.Add(DataListFactory.CreateDev2Column(subc.Name,
                                                                                              descAttribute.Value));
                                                }
                                                else
                                                {
                                                    cols.Add(DataListFactory.CreateDev2Column(subc.Name, string.Empty));
                                                }
                                            }

                                            // now re-insert the entry ;)
                                            if(!result.TryCreateRecordsetTemplate(c.Name, string.Empty, cols, true, out error))
                                            {
                                                errors.AddError(error);
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


                if (includeSysTags)
                {
                    // Build System Tag Shape ;)
                    for (int i = 0; i < TranslationConstants.systemTags.Length; i++)
                    {
                        if (result != null)
                        {
                            result.TryCreateScalarTemplate(GlobalConstants.SystemTagNamespace,
                                                           TranslationConstants.systemTags.GetValue(i).ToString(),
                                                           string.Empty,
                                                           true,
                                                           out error);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                errors.AddError(e.Message);
            }

            return result;   
        }
    }
}
