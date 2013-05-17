using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IRecordSetCollection {

        #region Properties
        IList<IRecordSetDefinition> RecordSets { get; }

        IList<string> RecordSetNames { get; }
        #endregion
    }
}
