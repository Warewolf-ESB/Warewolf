using Dev2.DataList.Contract;

namespace Dev2.Data.Interfaces
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
