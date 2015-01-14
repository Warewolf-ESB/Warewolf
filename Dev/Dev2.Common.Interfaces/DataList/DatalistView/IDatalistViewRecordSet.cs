using System.Collections.Generic;

namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IDatalistViewRecordSet : IDataListViewItem
    {
        IList<IDataListViewColumn> Columns { get; }
        string RecordsetName{get;set;}
       
    
 void AddColumn(IDataListViewColumn dataListViewColumn);
 void RemoveColumn(IDataListViewColumn dataListViewColumn);}
}