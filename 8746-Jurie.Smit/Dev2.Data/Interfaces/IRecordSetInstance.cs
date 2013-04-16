using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IRecordSetInstance : IRecordSetDefinition {

        #region Properties
        IList<IList<IDataValue>> RowData { get; }

        IRecordSetDefinition SetDefinition { get; }
        #endregion

        #region Methods
        void AddRow(IList<IDataValue> rowData);
        #endregion
    }
}
