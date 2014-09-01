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
