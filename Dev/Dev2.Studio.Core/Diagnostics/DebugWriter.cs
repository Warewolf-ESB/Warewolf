
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Diagnostics
{
    public class DebugWriter : IDebugWriter
    {
        readonly Action<IDebugState> _write;

        #region Initialization

        public DebugWriter(Action<IDebugState> write)
        {
            if(write == null)
            {
                throw new ArgumentNullException("write");
            }
            ID = Guid.NewGuid();
            _write = write;
        }

        #endregion

        public Guid ID { get; private set; }

        #region Write

        public void Write(IDebugState debugState)
        {
            if(debugState != null)
            {
                _write(debugState);
            }
        }

        #endregion
    }
}
