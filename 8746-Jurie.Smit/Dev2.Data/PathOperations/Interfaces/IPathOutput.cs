using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide properties related to File output destinations
    /// </summary>
    public interface IPathOutput {

        /// <summary>
        /// Output path location
        /// </summary>
        string OutputPath { get; set; }
    }
}
