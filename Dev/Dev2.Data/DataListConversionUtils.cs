#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
