/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Dev2.Data
{
    public class DataListModel : IDataListModel
    {
        public List<IScalar> Scalars { get; set; }
        public List<IScalar> ShapeScalars { get; set; }
        public List<IRecordSet> RecordSets { get; set; }
        public List<IRecordSet> ShapeRecordSets { get; set; }
        public List<IComplexObject> ShapeComplexObjects { get; set; }
        public List<IComplexObject> ComplexObjects { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DataListModel()
        {
            Scalars = new List<IScalar>();
            RecordSets = new List<IRecordSet>();
            ComplexObjects = new List<IComplexObject>();
            ShapeScalars = new List<IScalar>();
            ShapeRecordSets = new List<IRecordSet>();
            ShapeComplexObjects = new List<IComplexObject>();
        }

        public void Create(string data, string shape)
        {
            CreateShape(shape);
            PopulateWithData(data);
        }


        public void PopulateWithData(string data)
        {
            var toLoad = data;
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

            if (!String.IsNullOrEmpty(toLoad))
            {
                if (xDoc.DocumentElement != null)
                {
                    XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                    IDictionary<string, int> indexCache = new Dictionary<string, int>();

                    // spin through each element in the XML
                    foreach (XmlNode c in children)
                    {
                        var hasCorrectIoDirection = true;
                        var columnIoDirectionAttribute = c.Attributes?["ColumnIODirection"];
                        if (columnIoDirectionAttribute != null)
                        {
                            var columnIoDirectionValue = columnIoDirectionAttribute.Value;
                            var hasCorrectIoDirectionFromAttribute = columnIoDirectionValue == enDev2ColumnArgumentDirection.Output.ToString() || columnIoDirectionValue == enDev2ColumnArgumentDirection.Both.ToString();
                            hasCorrectIoDirection = hasCorrectIoDirectionFromAttribute;
                        }

                        if (DataListUtil.IsSystemTag(c.Name) && !hasCorrectIoDirection)
                        {
                            continue;
                        }
                        var recSet = RecordSets.FirstOrDefault(set => set.Name == c.Name);
                        var shapeRecSet = ShapeRecordSets.FirstOrDefault(set => set.Name == c.Name);
                        var scalar = Scalars.FirstOrDefault(scalar1 => scalar1.Name == c.Name);
                        var complexObject = ComplexObjects.FirstOrDefault(o => o.Name == "@" + c.Name);
                        if (complexObject != null)
                        {
                            if (!string.IsNullOrEmpty(c.OuterXml))
                            {
                                var jsonData = JsonConvert.SerializeXNode(XDocument.Parse(c.OuterXml),Newtonsoft.Json.Formatting.None,true);
                                var obj = JsonConvert.DeserializeObject(jsonData.Replace("@", "")) as JObject;
                                if(obj != null)
                                {
                                    var value = obj.ToString();
                                    complexObject.Value = value;
                                }
                            }
                        }
                        else
                        {
                            if (recSet != null && shapeRecSet != null)
                            {
                                // fetch recordset index
                                int fetchIdx;
                                int idx = indexCache.TryGetValue(c.Name, out fetchIdx) ? fetchIdx : 1; // recset index
                                // process recordset
                                var scalars = shapeRecSet.Columns[1];
                                var colToIoDirection = scalars.ToDictionary(scalar1 => scalar1.Name, scalar1 => scalar1.IODirection);
                                XmlNodeList nl = c.ChildNodes;
                                if (!recSet.Columns.ContainsKey(idx))
                                {
                                    recSet.Columns.Add(idx, new List<IScalar>());
                                }
                                else
                                {
                                    recSet.Columns[idx] = new List<IScalar>();
                                }
                                foreach (XmlNode subc in nl)
                                {
                                    if (colToIoDirection.ContainsKey(subc.Name))
                                    {
                                        var column = new Scalar { Name = subc.Name, Value = subc.InnerText, IODirection = colToIoDirection[subc.Name] };
                                        recSet.Columns[idx].Add(column);
                                    }
                                }
                                // update this recordset index
                                indexCache[c.Name] = ++idx;
                            }
                            else
                            {
                                if (scalar != null)
                                {
                                    scalar.Value = c.InnerXml;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CreateShape(string shape)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(shape);
            if (xDoc.DocumentElement != null)
            {
                XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                var columnDirection = enDev2ColumnArgumentDirection.None;
                foreach (XmlNode c in children)
                {
                    XmlAttribute descAttribute = null;
                    XmlAttribute columnIoDirection = null;
                    if (!DataListUtil.IsSystemTag(c.Name))
                    {
                        if (c.HasChildNodes)
                        {
                            var jsonAttribute = false;
                            var xmlAttribute = c.Attributes?["IsJson"];
                            if (xmlAttribute != null)
                            {
                                bool.TryParse(xmlAttribute.Value, out jsonAttribute);
                            }
                            if (jsonAttribute)
                            {
                                AddComplexObjectFromXmlNode(c);
                            }
                            else
                            {
                                var cols = new List<IScalar>();
                                foreach (XmlNode subc in c.ChildNodes)
                                {
                                    // It is possible for the .Attributes property to be null, a check should be added
                                    if (subc.Attributes != null)
                                    {
                                        descAttribute = subc.Attributes["Description"];
                                        columnIoDirection = subc.Attributes["ColumnIODirection"];
                                        if (columnIoDirection != null)
                                        {
                                            Enum.TryParse(columnIoDirection.Value, true, out columnDirection);
                                        }
                                    }
                                    var scalar = new Scalar { Name = subc.Name, IsEditable = true, IODirection = columnDirection };
                                    if (descAttribute != null)
                                    {
                                        scalar.Description = descAttribute.Value;
                                    }
                                    cols.Add(scalar);
                                }
                                if (c.Attributes != null)
                                {
                                    descAttribute = c.Attributes["Description"];
                                    columnIoDirection = c.Attributes["ColumnIODirection"];
                                }

                                var descriptionValue = "";
                                columnDirection = enDev2ColumnArgumentDirection.None;
                                if (descAttribute != null)
                                {
                                    descriptionValue = descAttribute.Value;
                                }
                                if (columnIoDirection != null)
                                {
                                    Enum.TryParse(columnIoDirection.Value, true, out columnDirection);
                                }
                                var recSet = new RecordSet { Columns = new Dictionary<int, List<IScalar>> { { 1, cols } }, Description = descriptionValue, IODirection = columnDirection, IsEditable = false, Name = c.Name };
                                RecordSets.Add(recSet);
                                var shapeRecSet = new RecordSet { Columns = new Dictionary<int, List<IScalar>> { { 1, cols } }, Description = descriptionValue, IODirection = columnDirection, IsEditable = false, Name = c.Name };
                                ShapeRecordSets.Add(shapeRecSet);
                            }
                        }
                        else
                        {
                            if (c.Attributes != null)
                            {
                                descAttribute = c.Attributes["Description"];
                                columnIoDirection = c.Attributes["ColumnIODirection"];
                            }
                            string descriptionValue = "";
                            columnDirection = enDev2ColumnArgumentDirection.None;
                            if (descAttribute != null)
                            {
                                descriptionValue = descAttribute.Value;
                            }
                            if (columnIoDirection != null)
                            {
                                Enum.TryParse(columnIoDirection.Value, true, out columnDirection);
                            }
                            var scalar = new Scalar { Name = c.Name, Description = descriptionValue, IODirection = columnDirection, IsEditable = true };
                            Scalars.Add(scalar);
                            ShapeScalars.Add(scalar);
                        }
                    }
                }
            }
        }

        private void AddComplexObjectFromXmlNode(XmlNode c)
        {
            var isArray = false;
            var ioDirection = enDev2ColumnArgumentDirection.None;
            XmlAttribute descAttribute = null;
            if (c.Attributes != null)
            {
                isArray = ParseBoolAttribute(c.Attributes["IsArray"]);
                ioDirection = ParseColumnIODirection(c.Attributes[GlobalConstants.DataListIoColDirection]);
                descAttribute = c.Attributes["Description"];
            }
            var name = GetNameForArrayComplexObject(c, isArray);
            var complexObjectItemModel = new ComplexObject { Name = "@" + name, IsArray = isArray, IODirection = ioDirection, Children = new Dictionary<int, List<IComplexObject>>() };
            if (descAttribute != null)
            {
                complexObjectItemModel.Description = descAttribute.Value;
            }
            ComplexObjects.Add(complexObjectItemModel);
            ShapeComplexObjects.Add(complexObjectItemModel);
        }

        private bool ParseBoolAttribute(XmlAttribute attr)
        {
            var result = false;
            if (attr != null)
            {
                bool.TryParse(attr.Value, out result);
            }
            return result;
        }

        private enDev2ColumnArgumentDirection ParseColumnIODirection(XmlAttribute attr)
        // ReSharper restore InconsistentNaming
        {
            enDev2ColumnArgumentDirection result = enDev2ColumnArgumentDirection.None;

            if (attr == null)
            {
                return result;
            }
            if (!Enum.TryParse(attr.Value, true, out result))
            {
                result = enDev2ColumnArgumentDirection.None;
            }
            return result;
        }

        private static string GetNameForArrayComplexObject(XmlNode xmlNode, bool isArray)
        {
            var name = isArray ? xmlNode.Name + "()" : xmlNode.Name;
            return name;
        }
    }
}