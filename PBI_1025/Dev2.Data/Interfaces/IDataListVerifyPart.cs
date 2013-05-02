using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Data.Binary_Objects;

namespace Dev2.DataList.Contract {
    public interface IDataListVerifyPart {

        string DisplayValue { get; }

        string Recordset { get; }

        string Field { get; }

        string Description { get; }

        bool IsScalar { get; }

        string RecordsetIndex { get; }

        bool HasRecordsetIndex { get; }
    }
}
