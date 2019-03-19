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

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Common;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data
{
    public class DataListConversionUtils
    {
        public OptomizedObservableCollection<IDataListItem> CreateListToBindTo(IDataListModel dataList) => GetInputs(dataList);

        OptomizedObservableCollection<IDataListItem> Get(IDataListModel dataList, enDev2ColumnArgumentDirection directionToGet)
        {
            var result = new OptomizedObservableCollection<IDataListItem>();

            if (dataList != null)
            {
                var listOfScalars = dataList.Scalars;

                foreach (var entry in listOfScalars
                    .Where(e => e.IODirection == directionToGet || e.IODirection == enDev2ColumnArgumentDirection.Both))
                {
                    result.AddRange(ConvertToIDataListItem(entry));
                }

                var listOfRecordSets = dataList.RecordSets;
                foreach (var entry in listOfRecordSets)
                {
                    result.AddRange(ConvertToIDataListItem(entry, directionToGet));
                }

                var listOfComplexObject = dataList.ComplexObjects;
                foreach (var complexObject in listOfComplexObject)
                {
                    if (complexObject.IODirection == enDev2ColumnArgumentDirection.Both || complexObject.IODirection == directionToGet)
                    {
                        result.Add(ConvertToIDataListItem(complexObject));
                    }
                }
            }

            return result;
        }

        public OptomizedObservableCollection<IDataListItem> GetInputs(IDataListModel dataList) => Get(dataList, enDev2ColumnArgumentDirection.Input);

        public OptomizedObservableCollection<IDataListItem> GetOutputs(IDataListModel dataList) => Get(dataList, enDev2ColumnArgumentDirection.Output);

        IList<IDataListItem> ConvertToIDataListItem(IScalar scalar)
        {
            IList<IDataListItem> result = new List<IDataListItem>();
            var item = scalar;
            if (item != null)
            {
                IDataListItem singleRes = new DataListItem
                {
                    CanHaveMutipleRows = false,
                    Field = item.Name,
                    DisplayValue = item.Name
                };
                try
                {
                    singleRes.Value = item.Value.UnescapeString();
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

        IList<IDataListItem> ConvertToIDataListItem(IRecordSet recordSet, enDev2ColumnArgumentDirection directionToGet)
        {
            IList<IDataListItem> result = new List<IDataListItem>();
            var dataListEntry = recordSet;
            
            foreach(var column in dataListEntry.Columns)
            {
                var fields = column.Value.Where(c => c.IODirection == enDev2ColumnArgumentDirection.Both || c.IODirection == directionToGet).ToList();
                foreach (var col in fields)
                {
                    IDataListItem singleRes = new DataListItem
                    {
                        CanHaveMutipleRows = true,
                        Recordset = recordSet.Name,
                        Field = col.Name,
                        Index = column.Key.ToString(),
                        Value = col.Value.UnescapeString(),
                        DisplayValue = DataListUtil.CreateRecordsetDisplayValue(recordSet.Name, col.Name, column.Key.ToString()),
                        Description = col.Description
                    };
                    result.Add(singleRes);
                }
            }
                                 
            return result;
        }

        IDataListItem ConvertToIDataListItem(IComplexObject complexObject)
        {
            IDataListItem singleRes = new DataListItem
            {
                IsObject = true,
                DisplayValue = complexObject.Name,
                Value = complexObject.Value,
                Field = complexObject.Name.TrimStart('@')
            };
            return singleRes;
        }
    }
}
