using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide properties related to UnZip operations as per DotNetZip : http://dotnetzip.codeplex.com/
    /// </summary>
    public interface IUnZip {

        /// <summary>
        /// The password for UnZiping
        /// </summary>
        string ArchivePassword { get; set; }

        /// <summary>
        /// Indicates if the destination must be overwritten or not
        /// </summary>
        bool Overwrite { get; set; }
    }
}
