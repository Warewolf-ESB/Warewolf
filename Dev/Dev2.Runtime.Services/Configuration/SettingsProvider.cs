using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Linq;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;

namespace Dev2.Runtime.Configuration
{
    /// <summary>
    /// Do NOT instantiate directly - use static <see cref="Instance" /> property instead; use for testing only!
    /// </summary>
    public class SettingsProvider : NetworkMessageProviderBase<SettingsMessage>
    {
        public static string WebServerUri { get; set; }

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
        /// Do NOT instantiate directly - use static <see cref="Instance" /> property instead;
        /// </summary>
        public SettingsProvider()
        {
            AssemblyHashCode = GetAssemblyHashCode();
            Configuration = ReadConfiguration();
        }

        #endregion

        public string AssemblyHashCode { get; private set; }

        public Settings.Configuration Configuration { get; private set; }

        #region ProcessMessage

        public override SettingsMessage ProcessMessage(SettingsMessage request)
        {
            if(request == null)
            {
                throw new ArgumentNullException("request");
            }

            switch(request.Action)
            {
                case NetworkMessageAction.Write:
                case NetworkMessageAction.Overwrite:
                    return ProcessWrite(request);

                default:
                    return ProcessRead(request);
            }
        }

        #endregion

        #region ProcessRead

        SettingsMessage ProcessRead(SettingsMessage request)
        {
            if(request.AssemblyHashCode != AssemblyHashCode)
            {
                request.Result = NetworkMessageResult.VersionConflict;
                request.AssemblyHashCode = AssemblyHashCode;
                request.Assembly = GetAssemblyBytes();
            }
            else
            {
                request.Assembly = null;
                request.Result = NetworkMessageResult.Success;
            }

            request.ConfigurationXml = Configuration.ToXml();

            return request;
        }

        #endregion

        #region ProcessWrite

        SettingsMessage ProcessWrite(SettingsMessage request)
        {
            var filePath = GetFilePath();
            var canSave = request.Action == NetworkMessageAction.Overwrite || !File.Exists(filePath);
            var configNew = new Settings.Configuration(request.ConfigurationXml);
            configNew.WebServerUri = WebServerUri;
            if(!canSave)
            {
                var configOld = new Settings.Configuration(XElement.Load(filePath));
                canSave = configNew.Version >= configOld.Version;
            }

            if(canSave)
            {
                var dir = Path.GetDirectoryName(filePath);
                if(dir != null)
                {
                    if(!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
                configNew.IncrementVersion();
                configNew.ToXml().Save(filePath);
                Configuration = configNew;
                request.Result = NetworkMessageResult.Success;
            }
            else
            {
                request.Result = NetworkMessageResult.VersionConflict;
            }

            return request;
        }

        #endregion

        //
        // Static Helpers
        //

        #region GetAssemblyHashCode

        static string GetAssemblyHashCode()
        {
            var assemblyBytes = GetAssemblyBytes();

            var hashAlgorithm = SHA256.Create();
            var hash = hashAlgorithm.ComputeHash(assemblyBytes);
            var hex = BitConverter.ToString(hash).Replace("-", string.Empty);

            return hex;
        }

        #endregion

        #region GetAssemblyBytes

        static byte[] GetAssemblyBytes()
        {
            var assembly = Assembly.GetAssembly(typeof(IConfigurationAssemblyMarker));
            return File.ReadAllBytes(assembly.Location);
        }

        #endregion

        #region GetFilePath

        public static string GetFilePath()
        {
            var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // ReSharper disable AssignNullToNotNullAttribute
            return Path.Combine(rootDir, "Settings", "Application.xml");
            // ReSharper restore AssignNullToNotNullAttribute
        }

        #endregion

        #region ReadConfiguration

        Settings.Configuration ReadConfiguration()
        {
            var filePath = GetFilePath();
            if(File.Exists(filePath))
            {
                try
                {
                    var xml = XElement.Load(filePath);
                    xml.SetAttributeValue("WebServerUri", WebServerUri);

                    return new Settings.Configuration(xml);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // error occurred so ignore and load empty
                }
            }
            return new Settings.Configuration(WebServerUri);
        }

        #endregion

    }
}