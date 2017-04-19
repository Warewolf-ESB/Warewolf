using System.Collections.Generic;
using Dev2.Data.Interfaces;

namespace Dev2.Studio.Interfaces.DataList
{
    public interface IMissingDataList
    {
        IEnumerable<IDataListVerifyPart> MissingRecordsets(IList<IDataListVerifyPart> partsToVerify, bool excludeUnusedItems);

        IEnumerable<IDataListVerifyPart> MissingScalars(IEnumerable<IDataListVerifyPart> partsToVerify, bool excludeUnusedItems);
    }
}