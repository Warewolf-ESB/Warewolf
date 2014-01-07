using System.Collections.Generic;

namespace Dev2.DataList.Contract
{
    public interface IRecordSetCollection {

        #region Properties
        IList<IRecordSetDefinition> RecordSets { get; }

        IList<string> RecordSetNames { get; }
        #endregion
    }
}
