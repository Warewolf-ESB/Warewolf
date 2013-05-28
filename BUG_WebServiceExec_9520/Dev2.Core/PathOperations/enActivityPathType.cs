using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.PathOperations {

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To abstract IO endpoint types
    /// </summary>
    public enum enActivityIOPathType {

        FileSystem,
        FTP,
        FTPS,
        FTPES,
        Invalid
    }
}
