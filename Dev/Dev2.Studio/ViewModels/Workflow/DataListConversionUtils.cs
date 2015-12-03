
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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
                singleRes.IsRecordset = false;
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
                    singleRes.IsRecordset = true;
                    singleRes.Recordset = recordSet.Name;
                    singleRes.Field = col.Name;
                    singleRes.RecordsetIndex = column.Key.ToString();
                    singleRes.Value = col.Value;
                    singleRes.DisplayValue = string.Concat(recordSet.Name, "(", column.Key, ").", col.Name);
                    singleRes.Description = col.Description;
                    result.Add(singleRes);

                }      
            }
                                 
            return result;
        }
    }
}
