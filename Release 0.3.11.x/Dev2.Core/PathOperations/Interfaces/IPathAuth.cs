using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a common abstraction as it related to file system ( FTP, FTPS, Disk ) operations
    /// </summary>
    public interface IPathAuth {

        /// <summary>
        /// Username for auth
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// Password for auth
        /// </summary>
        string Password { get; set; }
    }
}
