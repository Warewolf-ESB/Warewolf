using System;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract.Network
{
    public interface INetworkDataListChannel
    {
        bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors);
        IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors);
        void DeleteDataList(Guid id, bool onlyIfNotPersisted);
        Guid ServerID { get; }
    }
}
