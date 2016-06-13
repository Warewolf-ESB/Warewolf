
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Studio.Core;

namespace Dev2.ViewModels.Workflow
{
    public class DataListConversionUtils
    {
        public OptomizedObservableCollection<IDataListItem> CreateListToBindTo(IDataListModel dataList)
        {
            var result = new OptomizedObservableCollection<IDataListItem>();

            if(dataList != null)
            {
                var listOfScalars = dataList.Scalars;

                // process scalars ;)
                foreach (var entry in listOfScalars
                    .Where(e => e.IODirection == enDev2ColumnArgumentDirection.Input ||
                                e.IODirection == enDev2ColumnArgumentDirection.Both))
                {
                    result.AddRange(ConvertToIDataListItem(entry));
                }

                // now process recordsets ;)
                var listOfRecordSets = dataList.RecordSets;
                foreach (var entry in listOfRecordSets)
                {
                    result.AddRange(ConvertToIDataListItem(entry));
                }

                var listOfComplexObject = dataList.ComplexObjects;
                foreach(var complexObject in listOfComplexObject)
                {
                    if (complexObject.IODirection == enDev2ColumnArgumentDirection.Both || complexObject.IODirection == enDev2ColumnArgumentDirection.Input)
                    {
                        result.AddRange(ConvertToIDataListItem(complexObject,null));
                    }
                }
            }

            return result;
        }

        IList<IDataListItem> ConvertToIDataListItem(IScalar scalar)
        {
            IList<IDataListItem> result = new List<IDataListItem>();
            var item = scalar;
            if (item != null)
            {
                IDataListItem singleRes = new DataListItem();
                singleRes.CanHaveMutipleRows = false;
                singleRes.Field = item.Name;
                singleRes.DisplayValue = item.Name;
                try
                {
                    singleRes.Value = item.Value;
                }
                catch (Exception)
                {
                    singleRes.Value = null;
                }
                var desc = item.Description;
                singleRes.Description = string.IsNullOrWhiteSpace(desc) ? null : desc;
                result.Add(singleRes);
            }
            return result;
        }

        IList<IDataListItem> ConvertToIDataListItem(IRecordSet recordSet)
        {
            IList<IDataListItem> result = new List<IDataListItem>();
            var dataListEntry = recordSet;
            
            foreach(var column in dataListEntry.Columns)
            {
                var fields = column.Value.Where(c => c.IODirection == enDev2ColumnArgumentDirection.Both || c.IODirection == enDev2ColumnArgumentDirection.Input).ToList();
                foreach (var col in fields)
                {
                    IDataListItem singleRes = new DataListItem();
                    singleRes.CanHaveMutipleRows = true;
                    singleRes.Recordset = recordSet.Name;
                    singleRes.Field = col.Name;
                    singleRes.Index = column.Key.ToString();
                    singleRes.Value = col.Value;
                    singleRes.DisplayValue = DataListUtil.CreateRecordsetDisplayValue(recordSet.Name, col.Name, column.Key.ToString());
                    singleRes.Description = col.Description;
                    result.Add(singleRes);

                }      
            }
                                 
            return result;
        }

        IList<IDataListItem> ConvertToIDataListItem(IComplexObject complexObject, IDataListItem parentItem)
        {
            List<IDataListItem> result = new List<IDataListItem>();
            var dataListEntry = complexObject;
            foreach (var column in dataListEntry.Children)
            {
                var fields = column.Value;
                foreach (var col in fields)
                {
                    IDataListItem singleRes = new DataListItem();
                    singleRes.IsObject = true;
                    singleRes.CanHaveMutipleRows = complexObject.IsArray;
                    if (complexObject.IsArray)
                    {
                        singleRes.Recordset = complexObject.Name;
                        singleRes.Field = col.Name;
                        singleRes.Index = column.Key.ToString();
                        var displayValue = complexObject.Name + "." + col.Name;
                        singleRes.DisplayValue = DataListUtil.ReplaceRecordsetBlankWithIndex(displayValue, column.Key);
                    }
                    else
                    {
                        //if (col.Children == null || col.Children.Count == 0)
                        {
                            if (parentItem == null)
                            {
                                singleRes.Index = column.Key.ToString();
                                singleRes.Field = col.Name;
                                singleRes.DisplayValue = complexObject.Name + "." + col.Name;
                            }
                            else
                            {
                                singleRes.Index = column.Key.ToString();
                                singleRes.Field = col.Name;
                                singleRes.DisplayValue = parentItem.DisplayValue + "." + col.Name;
                            }
                        }
                    }
                    singleRes.Value = col.Value;
                    singleRes.Description = col.Description;
                    if (col.Children != null && col.Children.Count > 0)
                    {
                        foreach(var child in col.Children)
                        {
                            foreach(var obj in child.Value)
                            {
                                var childRes = ConvertToIDataListItem(obj,singleRes);
                                result.AddRange(childRes);
                            }
                        }
                    }
                    if (col.Children == null || col.Children.Count == 0)
                    {
                        result.Add(singleRes);
                    }
                }
            }
            if(complexObject.Children.Count==0)
            {
                IDataListItem singleRes = new DataListItem();
                singleRes.IsObject = true;
                if (parentItem!=null && parentItem.Field.Contains("()"))
                {
                    singleRes.Recordset = parentItem.Field;
                    singleRes.Field = complexObject.Name;
                    singleRes.CanHaveMutipleRows = true;
                    singleRes.Index = "1";
                    var displayValue = parentItem.DisplayValue + "." + complexObject.Name;
                    singleRes.DisplayValue = DataListUtil.ReplaceRecordsetBlankWithIndex(displayValue,1);
                }
                else
                {
                    if (parentItem == null)
                    {
                        singleRes.Index = "1";
                        singleRes.Field =  complexObject.Name;
                        singleRes.DisplayValue = complexObject.Name + "." + complexObject.Name;
                    }
                    else
                    {
                        singleRes.Index = "1";
                        singleRes.Field = complexObject.Name;
                        singleRes.DisplayValue = parentItem.DisplayValue + "." + complexObject.Name;
                    }
                }
                singleRes.Value = complexObject.Value;
                singleRes.Description = complexObject.Description;
                if (complexObject.Children != null && complexObject.Children.Count > 0)
                {
                    foreach (var child in complexObject.Children)
                    {
                        foreach (var obj in child.Value)
                        {
                            var childRes = ConvertToIDataListItem(obj, singleRes);
                            result.AddRange(childRes);
                        }
                    }
                }
                result.Add(singleRes);
            }       
            return result;
        }
    }
}
