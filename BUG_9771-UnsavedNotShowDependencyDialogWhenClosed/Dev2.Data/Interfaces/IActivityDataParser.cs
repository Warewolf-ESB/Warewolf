using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IActivityDataParser {

        #region Properties
        ActivityMappingTO ParsedData { get; }
        #endregion

        #region Methods
        void ParseDataStream(string xmlData, IList<IDev2Definition> defs);
        #endregion

    }
}
