using System;
using System.IO;

namespace Dev2.Services.Configuration
{
    public class UserConfigurationService : ConfigurationService
    {
        public static readonly string ConfigPath = Path.Combine(DefaultPath, "User.Config");

        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile UserConfigurationService _instance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static UserConfigurationService Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
                        {
                            _instance = Read<UserConfigurationService>(ConfigPath);
                        }
                    }
                }
                return _instance;
            }
        }


        #endregion

        public UserConfigurationService()
            : this(ConfigPath)
        {
        }

        public UserConfigurationService(string filePath)
            : base(filePath)
        {
            Help = new HelpConfiguration();
        }

        public HelpConfiguration Help { get; private set; }

    }
}
