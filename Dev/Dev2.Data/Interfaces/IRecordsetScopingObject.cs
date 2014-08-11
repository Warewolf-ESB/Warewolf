using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Dev2.DataList.Contract
{
    public interface IRecordsetScopingObject
    {
        IRecordsetTO GetRecordset(string recsetName);
        string Finalize(string dataListShape, string currentDataList);
        string PostingDataToDataList(IList<OutputTO> outputs, string currentDataList, string dataListShape);

        string DataList { get; }
    }
}
