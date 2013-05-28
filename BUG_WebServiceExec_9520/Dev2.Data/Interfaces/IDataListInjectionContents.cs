using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IDataListInjectionContents {

        bool IsSystemRegion { get; }

        #region Methods
        string ToInjectableState();
        #endregion
    }
}
