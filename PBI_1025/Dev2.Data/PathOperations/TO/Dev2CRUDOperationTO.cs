using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.PathOperations
{

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide overwrite value to CRUD path operations
    /// </summary>
    public class Dev2CRUDOperationTO : IPathOverwrite
    {

        public Dev2CRUDOperationTO(bool overwrite)
        {
            Overwrite = overwrite;
        }

        public bool Overwrite
        {
            get;
            set;
        }
    }
}
