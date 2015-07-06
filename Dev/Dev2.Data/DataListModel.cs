using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;

namespace Dev2.Data
{
    public class DataListModel : IDataListModel
    {
        #region Implementation of IDataListModel

        public List<IScalar> Scalars { get; set; }
        public List<IRecordSet> RecordSets { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DataListModel()
        {
            Scalars = new List<IScalar>();
            RecordSets = new List<IRecordSet>();
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
                        if (c.Attributes != null)
                        {
                            var columnIoDirectionAttribute = c.Attributes["ColumnIODirection"];
                            if (columnIoDirectionAttribute != null)
                            {
                                var columnIoDirectionValue = columnIoDirectionAttribute.Value;
                                var hasCorrectIoDirectionFromAttribute = columnIoDirectionValue == enDev2ColumnArgumentDirection.Output.ToString() || columnIoDirectionValue == enDev2ColumnArgumentDirection.Both.ToString();
                                hasCorrectIoDirection = hasCorrectIoDirectionFromAttribute;
                            }
                        }

                        if (DataListUtil.IsSystemTag(c.Name) && !hasCorrectIoDirection)
                        {
                            continue;
                        }
                        var recSet = RecordSets.FirstOrDefault(set => set.Name == c.Name);
                        var scalar = Scalars.FirstOrDefault(scalar1 => scalar1.Name == c.Name);
                        // scalars and recordset fetch
                        {
                            if (recSet!=null)
                            {
                                // fetch recordset index
                                int fetchIdx;
                                int idx = indexCache.TryGetValue(c.Name, out fetchIdx) ? fetchIdx : 1; // recset index
                                // process recordset
                                var scalars = recSet.Columns[1];
                                var colToIoDirection = scalars.ToDictionary(scalar1 => scalar1.Name, scalar1 => scalar1.IODirection);
                                XmlNodeList nl = c.ChildNodes;
                                if (!recSet.Columns.ContainsKey(idx))
                                {
                                    recSet.Columns.Add(idx,new List<IScalar>());
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
                                if(scalar!=null)
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
            if(xDoc.DocumentElement != null)
            {
                XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                var columnDirection = enDev2ColumnArgumentDirection.None;
                foreach(XmlNode c in children)
                {
                    XmlAttribute descAttribute = null;
                    XmlAttribute columnIoDirection = null;
                    if(!DataListUtil.IsSystemTag(c.Name))
                    {
                        if(c.HasChildNodes)
                        {
                            var cols = new List<IScalar>();
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
                                var scalar = new Scalar { Name = subc.Name, IsEditable = true, IODirection = columnDirection };
                                if(descAttribute != null)
                                {
                                    scalar.Description = descAttribute.Value;
                                }
                                cols.Add(scalar);
                            }
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
                            var recSet = new RecordSet { Columns = new Dictionary<int, List<IScalar>> { { 1, cols } }, Description = descriptionValue, IODirection = columnDirection, IsEditable = false, Name = c.Name };
                            RecordSets.Add(recSet);
                        }
                        else
                        {
                            if(c.Attributes != null)
                            {
                                descAttribute = c.Attributes["Description"];
                                columnIoDirection = c.Attributes["ColumnIODirection"];
                            }
                            string descriptionValue = "";
                            columnDirection = enDev2ColumnArgumentDirection.None;
                            if(descAttribute != null)
                            {
                                descriptionValue = descAttribute.Value;
                            }
                            if(columnIoDirection != null)
                            {
                                Enum.TryParse(columnIoDirection.Value, true, out columnDirection);
                            }
                            var scalar = new Scalar { Name = c.Name, Description = descriptionValue, IODirection = columnDirection, IsEditable = true };
                            Scalars.Add(scalar);
                        }
                    }
                }
            }
        }
    }
}