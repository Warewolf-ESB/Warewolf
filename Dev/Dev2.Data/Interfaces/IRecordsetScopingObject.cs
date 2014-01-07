using System.Collections.Generic;
namespace Dev2.DataList.Contract
{
    public interface IRecordsetScopingObject
    {
        IRecordsetTO GetRecordset(string RecsetName);
        string Finalize(string DataListShape, string CurrentDataList);
        string PostingDataToDataList(IList<OutputTO> outputs, string CurrentDataList, string DataListShape);

        string DataList { get; }
    }
}
