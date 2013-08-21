using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
