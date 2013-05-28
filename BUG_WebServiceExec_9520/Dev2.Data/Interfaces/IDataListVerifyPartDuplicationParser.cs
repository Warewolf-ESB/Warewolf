using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IDataListVerifyPartDuplicationParser {
        bool Equals(IDataListVerifyPart Comparable, IDataListVerifyPart Comparor);
        int GetHashCode(IDataListVerifyPart DataListVerifyPart);
    }
}
