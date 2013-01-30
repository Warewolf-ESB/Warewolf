using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract.Network
{
    public interface INetworkDataListChannel
    {
        bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors);
        IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors);
        void DeleteDataList(Guid id, bool onlyIfNotPersisted);
        bool PersistChildChain(Guid id);
    }
}
