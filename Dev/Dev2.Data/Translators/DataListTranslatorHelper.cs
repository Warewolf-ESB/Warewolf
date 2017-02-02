
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
using System.Xml;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Translators
{
    /// <summary>
    /// Used to build the target emition shape ;)
    /// </summary>
    public static class DataListTranslatorHelper
    {

        /*
         * There are plenty of test in the DynamicServicesEndPointTest that appear to test this functionality ;)
         */

        /// <summary>
        /// Build the template based upon the sent shape
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="error"></param>
        public static IBinaryDataList BuildTargetShape(string shape, out string error)
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
                        XmlAttribute descAttribute = null;
                        XmlAttribute columnIoDirection = null;
                        if (!DataListUtil.IsSystemTag(c.Name))
                        {
                            if (c.HasChildNodes)
                            {
                                IList<Dev2Column> cols = new List<Dev2Column>();
                                //recordset
                                if (c.ChildNodes != null)
                                {
                                    // build template
                                    if (!procssesNamespaces.Contains(c.Name))
                                    {
                                        // build columns
                                        foreach (XmlNode subc in c.ChildNodes)
                                        {
                                            // It is possible for the .Attributes property to be null, a check should be added
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
                                        string myError = string.Empty;
                                        // It is possible for the .Attributes property to be null, a check should be added
                                        if (c.Attributes != null)
                                        {
                                            descAttribute = c.Attributes["Description"];
                                            columnIoDirection = c.Attributes["ColumnIODirection"];
                                        }
                                        if (descAttribute != null)
                                        {
                                            var columnDirection = enDev2ColumnArgumentDirection.None;
                                            if (columnIoDirection != null)
                                            {
                                                Enum.TryParse(columnIoDirection.Value, true, out columnDirection);
                                            }
                                            if (!result.TryCreateRecordsetTemplate(c.Name, descAttribute.Value, cols, true, false, columnDirection, out myError))
                                            {
                                                error = myError;
                                            }
                                        }
                                        else
                                        {
                                            if (!result.TryCreateRecordsetTemplate(c.Name, string.Empty, cols, true, out myError))
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
                                if (c.Attributes != null)
                                {
                                    descAttribute = c.Attributes["Description"];
                                    columnIoDirection = c.Attributes["ColumnIODirection"];
                                }
                                if (descAttribute != null)
                                {
                                    var columnDirection = enDev2ColumnArgumentDirection.None;
                                    if (columnIoDirection != null)
                                    {
                                        Enum.TryParse(columnIoDirection.Value, true, out columnDirection);
                                    }
                                    result.TryCreateScalarTemplate(string.Empty, c.Name, descAttribute.Value, true, false, columnDirection, out error);
                                }
                                else
                                {
                                    result.TryCreateScalarTemplate(string.Empty, c.Name, string.Empty, true, out error);
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

    }
}
