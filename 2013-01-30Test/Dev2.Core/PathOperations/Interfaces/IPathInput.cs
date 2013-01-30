using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide properties related to File input paths
    /// </summary>
    public interface IPathInput {

        /// <summary>
        /// Input path location
        /// </summary>
        string InputPath { get; set; }
    }
}
