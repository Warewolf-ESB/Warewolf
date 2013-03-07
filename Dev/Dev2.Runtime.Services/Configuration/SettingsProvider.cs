using System;
using Dev2.Network.Messaging.Messages;

namespace Dev2.Runtime.Configuration
{
    /// <summary>
    /// Do NOT instantiate directly - use static <see cref="Instance"/> property instead; use for testing only!
    /// </summary>
    public class SettingsProvider : SettingsProviderBase
    {
        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile SettingsProvider _instance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static SettingsProvider Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
                        {
                            _instance = new SettingsProvider();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region CTOR

        /// <summary>
        /// Do NOT instantiate directly - use static <see cref="Instance"/> property instead; 
        /// </summary>
        private SettingsProvider()
        {
        }

        #endregion

        #region ProcessMessage

        public override ISettingsMessage ProcessMessage(ISettingsMessage request)
        {
            if(request == null)
            {
                throw new ArgumentNullException("request");
            }

            var result = new SettingsMessage
            {
                Handle = request.Handle
            };
            return result;
        }

        #endregion

    }
}