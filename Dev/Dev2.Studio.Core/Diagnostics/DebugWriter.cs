/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Newtonsoft.Json;

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

        public void Write(string serializeObject)
        {
            if(!string.IsNullOrEmpty(serializeObject))
            {
                var debugState = JsonConvert.DeserializeObject<IDebugState>(serializeObject);
                if(debugState != null)
                {
                    Write(debugState);
                }
            }
        }

        #endregion
    }
}
