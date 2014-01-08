using System;
using Dev2.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Diagnostics
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
