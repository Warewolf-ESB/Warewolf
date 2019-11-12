#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Data.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Dev2.Data.Interfaces.Enums;

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
            var xDoc = new XmlDocument();
            try
            {
                xDoc.PreserveWhitespace = true;
                xDoc.LoadXml(toLoad);
            }
            catch (Exception ex)
            {
                toLoad = "<root>" + toLoad + "</root>";
                xDoc.LoadXml(toLoad);
            }

            if (!string.IsNullOrEmpty(toLoad) && xDoc.DocumentElement != null)
            {
                var children = xDoc.DocumentElement.ChildNodes;

                IDictionary<string, int> indexCache = new Dictionary<string, int>();
                
                PopulateForEachXmlElement(children, indexCache);
            }

        }

        void PopulateForEachXmlElement(XmlNodeList children, IDictionary<string, int> indexCache)
        {
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
                    SetComplexObjectValue(c, complexObject);
                }
                else
                {
                    SetScalarOrEcordsetValue(indexCache, c, recSet, shapeRecSet, scalar);
                }
            }
        }

        private static void SetScalarOrEcordsetValue(IDictionary<string, int> indexCache, XmlNode c, IRecordSet recSet, IRecordSet shapeRecSet, IScalar scalar)
        {
            if (recSet != null && shapeRecSet != null)
            {
                // fetch recordset index
                var idx = indexCache.TryGetValue(c.Name, out int fetchIdx) ? fetchIdx : 1; // recset index
                                                                                           // process recordset
                var scalars = shapeRecSet.Columns[1];
                var colToIoDirection = scalars.ToDictionary(scalar1 => scalar1.Name, scalar1 => scalar1.IODirection);
                var nl = c.ChildNodes;
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

        private static void SetComplexObjectValue(XmlNode c, IComplexObject complexObject)
        {
            if (!string.IsNullOrEmpty(c.OuterXml))
            {
                var jsonData = JsonConvert.SerializeXNode(XDocument.Parse(c.OuterXml), Newtonsoft.Json.Formatting.None, true);
                if (JsonConvert.DeserializeObject(jsonData.Replace("@", "")) is JObject obj)
                {
                    var value = obj.ToString();
                    complexObject.Value = value;
                }
            }
        }

        public void CreateShape(string shape)
        {
            var xDoc = new XmlDocument();
            xDoc.LoadXml(shape);
            if (xDoc.DocumentElement != null)
            {
                var children = xDoc.DocumentElement.ChildNodes;

                var columnDirection = enDev2ColumnArgumentDirection.None;
                foreach (XmlNode c in children)
                {
                    columnDirection = AddNonSystemTags(columnDirection, c);
                }
            }
        }

        private enDev2ColumnArgumentDirection AddNonSystemTags(enDev2ColumnArgumentDirection columnDirection, XmlNode c)
        {
            XmlAttribute descAttribute = null;
            XmlAttribute columnIoDirection = null;
            if (!DataListUtil.IsSystemTag(c.Name))
            {
                if (c.HasChildNodes)
                {
                    var jsonAttribute = IsJsonAttribute(c);
                    if (jsonAttribute)
                    {
                        AddComplexObjectFromXmlNode(c);
                    }
                    else
                    {
                        AddRecsetShape(ref columnDirection, c, ref descAttribute, ref columnIoDirection);
                    }
                }
                else
                {
                    var jsonAttribute = IsJsonAttribute(c);
                    if (jsonAttribute)
                    {
                        AddComplexObjectFromXmlNode(c);
                    }
                    else
                    {
                        columnDirection = AddScalarsShape(c, ref descAttribute, ref columnIoDirection);
                    }
                }
            }

            return columnDirection;
        }

        private enDev2ColumnArgumentDirection AddScalarsShape(XmlNode c, ref XmlAttribute descAttribute, ref XmlAttribute columnIoDirection)
        {
            enDev2ColumnArgumentDirection columnDirection;
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
            var scalar = new Scalar { Name = c.Name, Description = descriptionValue, IODirection = columnDirection, IsEditable = true };
            Scalars.Add(scalar);
            ShapeScalars.Add(scalar);
            return columnDirection;
        }

        private void AddRecsetShape(ref enDev2ColumnArgumentDirection columnDirection, XmlNode c, ref XmlAttribute descAttribute, ref XmlAttribute columnIoDirection)
        {
            var cols = new List<IScalar>();
            foreach (XmlNode subc in c.ChildNodes)
            {
                // It is possible for the .Attributes property to be null, a check should be added
                AddAttributes(ref columnDirection, ref descAttribute, ref columnIoDirection, cols, subc);
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

        private static void AddAttributes(ref enDev2ColumnArgumentDirection columnDirection, ref XmlAttribute descAttribute, ref XmlAttribute columnIoDirection, List<IScalar> cols, XmlNode subc)
        {
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

        static bool IsJsonAttribute(XmlNode c)
        {
            var jsonAttribute = false;
            var xmlAttribute = c.Attributes?["IsJson"];
            if (xmlAttribute != null)
            {
                bool.TryParse(xmlAttribute.Value, out jsonAttribute);
            }
            return jsonAttribute;
        }

        void AddComplexObjectFromXmlNode(XmlNode c)
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

        bool ParseBoolAttribute(XmlAttribute attr)
        {
            var result = false;
            if (attr != null)
            {
                bool.TryParse(attr.Value, out result);
            }
            return result;
        }

        enDev2ColumnArgumentDirection ParseColumnIODirection(XmlAttribute attr)
        {
            var result = enDev2ColumnArgumentDirection.None;

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

        static string GetNameForArrayComplexObject(XmlNode xmlNode, bool isArray)
        {
            var name = isArray ? xmlNode.Name + "()" : xmlNode.Name;
            return name;
        }
    }
}