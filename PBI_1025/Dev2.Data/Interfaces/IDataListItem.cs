using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IDataListItem
    {
        string Field { get; set; }
        string Recordset { get; set; }
        string RecordsetIndex { get; set; }        
        enRecordsetIndexType RecordsetIndexType { get; set; }
        bool IsRecordset { get; set; }
        string Value { get; set; }
        string DisplayValue { get; set; }
        string Description { get; set; }
    }
}
