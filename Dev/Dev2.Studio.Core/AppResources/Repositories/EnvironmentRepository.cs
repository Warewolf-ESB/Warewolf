using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Util;
using ResourceType = Dev2.Studio.Core.AppResources.Enums.ResourceType;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core
{

    public class EnvironmentRepository : IEnvironmentRepository
    {
        static readonly List<IEnvironmentModel> EmptyList = new List<IEnvironmentModel>();

        static readonly object FileLock = new Object();
        static readonly object RestoreLock = new Object();
        protected readonly List<IEnvironmentModel> Environments;
        private bool _isDisposed;

        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a SyncInstance 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile EnvironmentRepository _instance;

        static readonly object SyncInstance = new Object();

        /// <summary>
        /// Gets the repository instance.
        /// </summary>
        public static IEnvironmentRepository Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncInstance)
                    {
                        if(_instance == null)
                        {
                            _instance = new EnvironmentRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        public static EnvironmentRepository Create(IEnvironmentModel source)
        {
            return new EnvironmentRepository(source);
        }

        #region CTOR

        // Singleton instance only
        protected EnvironmentRepository()
            : this(CreateEnvironmentModel(Guid.Empty, new Uri(AppSettings.LocalHost), StringResources.DefaultEnvironmentName))
        {
        }

        // Testing only!!!
        protected EnvironmentRepository(IEnvironmentModel source)
        {
            if(source == null)
            {
                throw new ArgumentNullException("source");
            }
            Source = source;
            Environments = new List<IEnvironmentModel> { Source };
        }

        //For Testing Only!!!!!!!
        public EnvironmentRepository(EnvironmentRepository environmentRepository)
        {
            _instance = environmentRepository;
        }

        #endregion

        public event EventHandler ItemAdded;
        public event EventHandler<EnvironmentEditedArgs> ItemEdited;

        public IEnvironmentModel Source { get; private set; }
        public IEnvironmentModel ActiveEnvironment { get; set; }

        public bool IsLoaded { get; set; }

        #region Clear

        public void Clear()
        {
            foreach(var environment in Environments)
            {
                environment.Disconnect();
            }
            Environments.Clear();
        }

        #endregion

        #region All/Find/FindSingle/Load

        public virtual ICollection<IEnvironmentModel> All()
        {
            LoadInternal();
            return Environments;
        }

        public ICollection<IEnvironmentModel> Find(Expression<Func<IEnvironmentModel, bool>> expression)
        {
            LoadInternal();
            return expression == null ? EmptyList : Environments.AsQueryable().Where(expression).ToList();
        }

        public IEnvironmentModel FindSingle(Expression<Func<IEnvironmentModel, bool>> expression)
        {
            LoadInternal();
            return expression == null ? null : Environments.AsQueryable().FirstOrDefault(expression);
        }

        public void Load()
        {
            LoadInternal();
        }

        public void ForceLoad()
        {
            IsLoaded = false;
            LoadInternal();
        }

        public void Remove(Guid id)
        {
            var toRemove = Get(id);
            Remove(toRemove);
        }

        public IEnvironmentModel Get(Guid id)
        {
            return All().FirstOrDefault(e => e.ID == id);
        }

        #endregion

        #region Save

        public void Save(ICollection<IEnvironmentModel> environments)
        {
            if(environments == null || environments.Count == 0)
            {
                return;
            }
            foreach(var environmentModel in environments)
            {
                AddInternal(environmentModel);
            }
        }

        public string Save(IEnvironmentModel environment)
        {
            if(environment == null)
            {
                return "Not Saved";
            }
            AddInternal(environment);
            return "Saved";
        }

        #endregion

        #region Remove

        public void Remove(ICollection<IEnvironmentModel> environments)
        {
            if(environments == null || environments.Count == 0)
            {
                return;
            }
            foreach(var environmentModel in environments)
            {
                RemoveInternal(environmentModel);
            }
            //
            // NOTE: This should NEVER remove the environment source from the server 
            //       as this is done by the user via the explorer
            //
        }

        public void Remove(IEnvironmentModel environment)
        {
            if(environment == null)
            {
                return;
            }
            RemoveInternal(environment);
            //
            // NOTE: This should NEVER remove the environment source from the server 
            //       as this is done by the user via the explorer
            //
        }

        #endregion

        #region Read/WriteFile

        public virtual IList<Guid> ReadSession()
        {
            lock(FileLock)
            {
                var path = GetEnvironmentsFilePath();

                var tryReadFile = File.Exists(path) ? File.ReadAllText(path) : null;

                // ReSharper disable RedundantAssignment
                var xml = new XElement("Environments");
                // ReSharper restore RedundantAssignment
                var result = new List<Guid>();

                if(!string.IsNullOrEmpty(tryReadFile))
                {
                    try
                    {
                        xml = XElement.Parse(tryReadFile);
                        var guids = xml.Descendants("Environment").Select(id => id.Value).ToList();
                        foreach(var guidStr in guids)
                        {
                            Guid guid;
                            if(Guid.TryParse(guidStr, out guid))
                            {
                                result.Add(guid);
                            }
                        }
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch { }
                    // ReSharper restore EmptyGeneralCatchClause
                }

                return result;
            }
        }

        public virtual void WriteSession(IEnumerable<Guid> environmentGuids)
        {
            lock(FileLock)
            {
                var xml = new XElement("Environments");
                if(environmentGuids != null)
                {
                    foreach(var environmentId in environmentGuids.Where(id => id != Guid.Empty))
                    {
                        xml.Add(new XElement("Environment", environmentId));
                    }
                }
                var path = GetEnvironmentsFilePath();
                xml.Save(path);
            }
        }

        #endregion

        #region Fetch

        public IEnvironmentModel Fetch(IEnvironmentModel server)
        {
            LoadInternal();

            IEnvironmentModel environment = null;
            if(server != null)
            {
                Guid id = server.ID;
                environment = Environments.FirstOrDefault(e => e.ID == id) ?? CreateEnvironmentModel(id, server.Connection.AppServerUri, server.Connection.AuthenticationType, server.Connection.UserName, server.Connection.Password, server.Name);
            }
            return environment;
        }

        static IEnvironmentModel CreateEnvironmentModel(Guid id, Uri applicationServerUri, AuthenticationType authenticationType, string userName, string password, string name)
        {
            ServerProxy connectionProxy;
            if(authenticationType == AuthenticationType.Windows || authenticationType == AuthenticationType.Anonymous)
            {
                connectionProxy = new ServerProxy(applicationServerUri);
            }
            else
            {
                connectionProxy = new ServerProxy(applicationServerUri.ToString(), userName, password);
            }
            return new EnvironmentModel(id, connectionProxy) { Name = name };
        }

        #endregion

        #region RaiseItemAdded

        void RaiseItemAdded()
        {
            if(ItemAdded != null)
            {
                ItemAdded(this, new EventArgs());
            }
        }

        void RaiseItemEdited(IEnvironmentModel environment, bool isConnected)
        {
            if(ItemEdited != null)
            {
                ItemEdited(this, new EnvironmentEditedArgs(environment, isConnected));
            }
        }

        #endregion

        #region LoadInternal
        protected virtual void LoadInternal()
        {
            lock(RestoreLock)
            {
                if(IsLoaded)
                {
                    return;
                }

                var environments = LookupEnvironments(Source);

                // Don't just clear and add, environments may be connected!!!
                foreach(var newEnv in environments.Where(newEnv => !Environments.Contains(newEnv)))
                {
                    Environments.Add(newEnv);
                }


                var toBeRemoved = Environments.Where(e => !e.Equals(Source) && !environments.Contains(e)).ToList();
                foreach(var environment in toBeRemoved)
                {
                    environment.Disconnect();
                    Environments.Remove(environment);
                }

                IsLoaded = true;
            }
        }

        #endregion

        #region Add/RemoveInternal

        protected virtual void AddInternal(IEnvironmentModel environment)
        {
            var index = Environments.IndexOf(environment);

            if(index == -1)
            {
                Environments.Add(environment);
            }
            else
            {
                var environmentModel = Environments[index];
                var isConnected = environmentModel.IsConnected;
                Environments.RemoveAt(index);
                Environments.Add(environment);
                RaiseItemEdited(environment, isConnected);
            }
            RaiseItemAdded();
        }

        protected virtual bool RemoveInternal(IEnvironmentModel environment)
        {
            var index = Environments.IndexOf(environment);
            if(index != -1)
            {
                environment.Disconnect();
                Environments.RemoveAt(index);
                return true;
            }
            return false;
        }

        #endregion

        #region Static Helpers

        #region LookupEnvironments

        /// <summary>
        /// Lookups the environments.
        /// <remarks>
        /// If <paramref name="environmentGuids"/> is <code>null</code> or empty then this returns all <see cref="enSourceType.Dev2Server"/> sources.
        /// </remarks>
        /// </summary>
        /// <param name="defaultEnvironment">The default environment.</param>
        /// <param name="environmentGuids">The environment guids to be queried; may be null.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">defaultEnvironment</exception>
        public static IList<IEnvironmentModel> LookupEnvironments(IEnvironmentModel defaultEnvironment, IList<string> environmentGuids = null)
        {
            if(defaultEnvironment == null)
            {
                throw new ArgumentNullException("defaultEnvironment");
            }

            var result = new List<IEnvironmentModel>();
            try
            {
                defaultEnvironment.Connect();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                //Swallow exception for localhost connection
            }
            if(!defaultEnvironment.IsConnected)
            {
                return result;
            }

            var hasEnvironmentGuids = environmentGuids != null;

            if(hasEnvironmentGuids)
            {
                var servers = defaultEnvironment.ResourceRepository.FindResourcesByID(defaultEnvironment, environmentGuids, ResourceType.Source);
                foreach(var env in servers)
                {
                    var payload = env.WorkflowXaml;

                    if(payload != null)
                    {
                        #region Parse connection string values

                        // Let this use of strings go, payload should be under the LOH size limit if 85k bytes ;)
                        XElement xe = XElement.Parse(payload.ToString());
                        var conStr = xe.AttributeSafe("ConnectionString");
                        Dictionary<string, string> connectionParams = ParseConnectionString(conStr);

                        string tmp;
                        if(!connectionParams.TryGetValue("AppServerUri", out tmp))
                        {
                            continue;
                        }

                        Uri appServerUri;
                        try
                        {
                            appServerUri = new Uri(tmp);
                        }
                        catch
                        {
                            continue;
                        }

                        if(!connectionParams.TryGetValue("WebServerPort", out tmp))
                        {
                            continue;
                        }
                        int webServerPort;
                        if(!int.TryParse(tmp, out webServerPort))
                        {
                            continue;
                        }

                        if(!connectionParams.TryGetValue("AuthenticationType", out tmp))
                        {
                            tmp = "";
                        }
                        AuthenticationType authenticationType;
                        if(!Enum.TryParse(tmp, true, out authenticationType))
                        {
                            authenticationType = AuthenticationType.Windows;
                        }
                        string userName;
                        connectionParams.TryGetValue("UserName", out userName);
                        string password;
                        connectionParams.TryGetValue("Password", out password);
                        #endregion

                        var environment = CreateEnvironmentModel(env.ID, appServerUri, authenticationType, userName, password, env.DisplayName);
                        result.Add(environment);
                    }
                }
            }
            else
            {
                var servers = defaultEnvironment.ResourceRepository.FindSourcesByType<Connection>(defaultEnvironment, enSourceType.Dev2Server);
                if(servers != null)
                {
                    result.AddRange(from env in servers let uri = new Uri(env.Address) select CreateEnvironmentModel(env));
                }
            }

            return result;
        }

        static IEnvironmentModel CreateEnvironmentModel(Connection connection)
        {

            var resourceId = connection.ResourceID;
            ServerProxy connectionProxy;
            if(connection.AuthenticationType == AuthenticationType.Windows || connection.AuthenticationType == AuthenticationType.Anonymous)
            {
                connectionProxy = new ServerProxy(new Uri(connection.WebAddress));
            }
            else
            {
                connectionProxy = new ServerProxy(connection.WebAddress, connection.UserName, connection.Password);
            }
            return new EnvironmentModel(resourceId, connectionProxy) { Name = connection.ResourceName };
        }

        #endregion

        #region ParseConnectionString

        static Dictionary<string, string> ParseConnectionString(string s)
        {
            var values = s.Split(';');

            var enumerable = values.Select(value => value.Split('=')).Where(kvp => kvp.Length > 1);
            var connectionString = enumerable.ToDictionary(kvp => kvp[0], kvp => kvp[1]);
            return connectionString;
        }

        #endregion

        #region GetEnvironmentsDirectory

        public static string GetEnvironmentsDirectory()
        {
            var path = Path.Combine(new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                StringResources.App_Data_Directory,
                StringResources.Environments_Directory
            });

            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        #endregion

        #region GetEnvironmentsFilePath

        public static string GetEnvironmentsFilePath()
        {
            return Path.Combine(GetEnvironmentsDirectory(), "Environments.xml");
        }

        #endregion

        #region CreateEnvironmentModel

        static IEnvironmentModel CreateEnvironmentModel(Guid id, Uri applicationServerUri, string alias)
        {
            var acutalWebServerUri = new Uri(applicationServerUri.ToString().Replace("localhost", Environment.MachineName));
            var environmentConnection = new ServerProxy(acutalWebServerUri);
            return new EnvironmentModel(id, environmentConnection) { Name = alias };
        }

        #endregion

        public static string GetAppServerUriFromConnectionString(string connectionstring)
        {
            if(string.IsNullOrWhiteSpace(connectionstring))
            {
                return string.Empty;
            }

            const string ToLookFor = "AppServerUri";
            var appServerUriIdx = connectionstring.IndexOf(ToLookFor, StringComparison.Ordinal);
            var length = ToLookFor.Length;
            var substring = connectionstring.Substring(appServerUriIdx + length + 1);
            var indexofDelimiter = substring.IndexOf(';');
            var uri = substring.Substring(0, indexofDelimiter);
            return uri;
        }
        #endregion

        #region Implementation of IDisposable

        ~EnvironmentRepository()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // TODO 
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                _isDisposed = true;
            }
        }

        #endregion
    }

    public class EnvironmentEditedArgs : EventArgs
    {
        public IEnvironmentModel Environment { get; set; }
        public bool IsConnected { get; set; }

        public EnvironmentEditedArgs(IEnvironmentModel environment, bool isConnected)
        {
            Environment = environment;
            IsConnected = isConnected;
        }
    }
}
