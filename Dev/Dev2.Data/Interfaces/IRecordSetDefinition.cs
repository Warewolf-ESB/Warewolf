using System.Collections.Generic;

namespace Dev2.DataList.Contract
{
    public interface IRecordSetDefinition {

        #region Properties
        string SetName { get; }

        string XmlSetName { get; }

        IList<IDev2Definition> Columns { get; }
        #endregion
    }
}
