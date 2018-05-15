using System.Collections.Generic;
using Dev2.Data.Interfaces;

namespace Dev2.Studio.Interfaces.DataList
{
    public interface IMissingDataList
    {
        IEnumerable<IDataListVerifyPart> MissingRecordsets(IList<IDataListVerifyPart> partsToVerify);

        IEnumerable<IDataListVerifyPart> MissingScalars(IEnumerable<IDataListVerifyPart> partsToVerify);
    }
}