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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Util;


namespace Dev2.Studio.Core
{

    public class ServerRepository : IServerRepository
    {
        static readonly List<IServer> EmptyList = new List<IServer>();

        static readonly object FileLock = new Object();
        static readonly object RestoreLock = new Object();
        protected List<IServer> Environments;
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
        static volatile IServerRepository _instance;

        static readonly object SyncInstance = new Object();
        
        public static IServerRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncInstance)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServerRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region CTOR
        
        public ServerRepository()
            : this(CreateEnvironmentModel(Guid.Empty, new Uri(string.IsNullOrEmpty(AppSettings.LocalHost) ? $"http://{Environment.MachineName.ToLowerInvariant()}:3142" : AppSettings.LocalHost), StringResources.DefaultEnvironmentName))
        {
        }
        
        protected ServerRepository(IServer source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Environments = new List<IServer> { Source };
        }
        
        public ServerRepository(IServerRepository serverRepository)
        {
#pragma warning disable S3010 // For testing
            _instance = serverRepository;
#pragma warning restore S3010
        }

        #endregion

        public event EventHandler ItemAdded;
        public event EventHandler<IEnvironmentEditedArgs> ItemEdited;

        public IServer Source { get; private set; }
        public IServer ActiveServer { get; set; }

        public bool IsLoaded { get; set; }

        #region Clear

        public void Clear()
        {
            foreach (var environment in Environments)
            {
                environment.Disconnect();
            }
            Environments.Clear();
        }

        #endregion

        #region All/Find/FindSingle/Load

        public virtual ICollection<IServer> All()
        {
            LoadInternal();
            return Environments;
        }

        public virtual ICollection<IServer> ReloadServers()
        {
            LoadInternal(true);
            return Environments;
        }

        public virtual ICollection<IServer> ReloadAllServers()
        {
            LoadComplete();
            return Environments;
        }
        public ICollection<IServer> Find(Expression<Func<IServer, bool>> expression)
        {
            LoadInternal();
            return expression == null ? EmptyList : Environments.AsQueryable().Where(expression).ToList();
        }

        public IServer FindSingle(Expression<Func<IServer, bool>> expression)
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

        public IServer Get(Guid id)
        {
            return All().FirstOrDefault(e => e.EnvironmentID == id);
        }

        #endregion

        #region Save

        public void Save(ICollection<IServer> environments)
        {
            if (environments == null || environments.Count == 0)
            {
                return;
            }
            foreach (var environmentModel in environments)
            {
                AddInternal(environmentModel);
            }
        }

        public string Save(IServer environment)
        {
            if (environment == null)
            {
                return "Not Saved";
            }
            AddInternal(environment);
            return "Saved";
        }

        #endregion

        #region Remove

        public void Remove(ICollection<IServer> environments)
        {
            if (environments == null || environments.Count == 0)
            {
                return;
            }
            foreach (var environmentModel in environments)
            {
                RemoveInternal(environmentModel);
            }
            //
            // NOTE: This should NEVER remove the environment source from the server 
            //       as this is done by the user via the explorer
            //
        }

        public void Remove(IServer environment)
        {
            if (environment == null)
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
            lock (FileLock)
            {
                var path = GetEnvironmentsFilePath();

                var tryReadFile = File.Exists(path) ? File.ReadAllText(path) : null;

                
                var xml = new XElement("Environments");
                
                var result = new List<Guid>();

                if (!string.IsNullOrEmpty(tryReadFile))
                {
                    try
                    {
                        xml = XElement.Parse(tryReadFile);
                        var guids = xml.Descendants("Environment").Select(id => id.Value).ToList();
                        foreach (var guidStr in guids)
                        {
                            if (Guid.TryParse(guidStr, out Guid guid))
                            {
                                result.Add(guid);
                            }
                        }
                    }                    
                    catch (Exception e)
                    {
                        Dev2Logger.Warn(e.Message, "Warewolf Warn");
                    }
                    
                }

                return result;
            }
        }

        public virtual void WriteSession(IEnumerable<Guid> environmentGuids)
        {
            lock (FileLock)
            {
                var xml = new XElement("Environments");
                if (environmentGuids != null)
                {
                    foreach (var environmentId in environmentGuids.Where(id => id != Guid.Empty))
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

        public IServer Fetch(IServer server)
        {
            LoadInternal();

            IServer environment = null;
            if (server != null)
            {
                Guid id = server.EnvironmentID;
                environment = Environments.FirstOrDefault(e => e.EnvironmentID == id) ?? CreateEnvironmentModel(id, server.Connection.AppServerUri, server.Connection.AuthenticationType, server.Connection.UserName, server.Connection.Password, server.Name);
            }
            return environment;
        }

        static IServer CreateEnvironmentModel(Guid id, Uri applicationServerUri, AuthenticationType authenticationType, string userName, string password, string name)
        {
            ServerProxy connectionProxy;
            if (authenticationType == AuthenticationType.Windows || authenticationType == AuthenticationType.Anonymous)
            {
                connectionProxy = new ServerProxy(applicationServerUri);
            }
            else
            {
                if (authenticationType == AuthenticationType.Public)
                {
                    userName = "\\";
                    password = "";
                }
                connectionProxy = new ServerProxy(applicationServerUri.ToString(), userName, password);
            }
            return new Server(id, connectionProxy) { Name = name };
        }

        #endregion

        #region RaiseItemAdded

        void RaiseItemAdded()
        {
            ItemAdded?.Invoke(this, new EventArgs());
        }

        void RaiseItemEdited(IServer environment, bool isConnected)
        {
            ItemEdited?.Invoke(this, new EnvironmentEditedArgs(environment, isConnected));
        }

        #endregion

        #region LoadInternal
        protected virtual void LoadInternal(bool force = false)
        {
            lock (RestoreLock)
            {
                if (IsLoaded && !force)
                {
                    return;
                }
                var environments = LookupEnvironments(Source);
                // Don't just clear and add, environments may be connected!!!
                foreach (var newEnv in environments.Where(newEnv => !ValidateIfEnvironmentExists(newEnv)))
                {
                    AddEnvironmentIfNotExist(newEnv);
                }
                foreach (var newEnv in environments.Where(newEnv => Environments.Contains(newEnv)))
                {
                    var res = Environments.FirstOrDefault(a => a.EnvironmentID == newEnv.EnvironmentID);
                    if (res != null && !res.Equals(newEnv))
                    {
                        if (res.IsConnected)
                        {
                            res.Disconnect();
                        }
                        Environments.Remove(res);
                        AddEnvironmentIfNotExist(newEnv);
                    }
                }

                var toBeRemoved = Environments.Where(e => !e.Equals(Source) && !environments.Contains(e)).ToList();
                foreach (var environment in toBeRemoved)
                {
                    environment.Disconnect();
                    Environments.Remove(environment);
                }

                IsLoaded = true;
            }
        }

        private void AddEnvironmentIfNotExist(IServer newEnv)
        {
            if (!ValidateIfEnvironmentExists(newEnv))
            {
                Environments.Add(newEnv);
            }
        }

        private bool ValidateIfEnvironmentExists(IServer newEnv)
        {
            return Environments.Contains(newEnv);
        }

        protected virtual void LoadComplete()
        {
            lock (RestoreLock)
            {
                var environments = LookupEnvironments(Source);
                Environments = new List<IServer> { Source };
                Environments.AddRange(environments.ToList());
            }
        }

        #endregion

        #region Add/RemoveInternal

        protected virtual void AddInternal(IServer environment)
        {
            var index = Environments.IndexOf(environment);

            if (index == -1)
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

        protected virtual bool RemoveInternal(IServer environment)
        {
            var index = Environments.IndexOf(environment);
            if (index != -1)
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

        public IList<IServer> LookupEnvironments(IServer defaultEnvironment) => LookupEnvironments(defaultEnvironment, null);

        public IList<IServer> LookupEnvironments(IServer defaultEnvironment, IList<string> environmentGuids)
        {
            if (defaultEnvironment == null)
            {
                throw new ArgumentNullException(nameof(defaultEnvironment));
            }

            var result = new List<IServer>();
            try
            {
                defaultEnvironment.Connect();
            }
            
            catch (Exception err)
            
            {
                Dev2Logger.Info(err, "Warewolf Info");
                //Swallow exception for localhost connection
            }
            if (!defaultEnvironment.IsConnected)
            {
                return result;
            }

            var hasEnvironmentGuids = environmentGuids != null;

            if (hasEnvironmentGuids)
            {
                var servers = defaultEnvironment.ResourceRepository.FindResourcesByID(defaultEnvironment, environmentGuids, ResourceType.Source);
                foreach (var env in servers)
                {
                    var payload = env.WorkflowXaml;

                    if (payload != null)
                    {
                        #region Parse connection string values

                        // Let this use of strings go, payload should be under the LOH size limit if 85k bytes ;)
                        XElement xe = XElement.Parse(payload.ToString());
                        var conStr = xe.AttributeSafe("ConnectionString");
                        Dictionary<string, string> connectionParams = ParseConnectionString(conStr);

                        if (!connectionParams.TryGetValue("AppServerUri", out string tmp))
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

                        if (!connectionParams.TryGetValue("WebServerPort", out tmp))
                        {
                            continue;
                        }
                        if (!int.TryParse(tmp, out int webServerPort))
                        {
                            continue;
                        }

                        if (!connectionParams.TryGetValue("AuthenticationType", out tmp))
                        {
                            tmp = "";
                        }
                        if (!Enum.TryParse(tmp, true, out AuthenticationType authenticationType))
                        {
                            authenticationType = AuthenticationType.Windows;
                        }
                        connectionParams.TryGetValue("UserName", out string userName);
                        connectionParams.TryGetValue("Password", out string password);
                        #endregion

                        var environment = CreateEnvironmentModel(env.ID, appServerUri, authenticationType, userName, password, env.DisplayName);
                        result.Add(environment);
                    }
                }
            }
            else
            {
                var servers = defaultEnvironment.ResourceRepository.FindSourcesByType<Connection>(defaultEnvironment, enSourceType.Dev2Server);
                if (servers != null)
                {
                    foreach (var connection in servers)
                    {
                        if (!string.IsNullOrEmpty(connection.Address) && !string.IsNullOrEmpty(connection.WebAddress))
                        {
                            var environmentModel = CreateEnvironmentModel(connection);
                            result.Add(environmentModel);
                        }
                    }
                }
            }

            return result;
        }

        static IServer CreateEnvironmentModel(Connection connection)
        {

            var resourceId = connection.ResourceID;
            ServerProxy connectionProxy;
            if (connection.AuthenticationType == AuthenticationType.Windows || connection.AuthenticationType == AuthenticationType.Anonymous)
            {
                connectionProxy = new ServerProxy(new Uri(connection.WebAddress));
            }
            else
            {
                var userName = connection.UserName;
                var password = connection.Password;
                if (connection.AuthenticationType == AuthenticationType.Public)
                {
                    userName = "\\";
                    password = "";
                }

                connectionProxy = new ServerProxy(connection.WebAddress, userName, password);
            }
            return new Server(resourceId, connectionProxy) { Name = connection.ResourceName };
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

            if (!Directory.Exists(path))
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

        private static IServer CreateEnvironmentModel(Guid id, Uri applicationServerUri, string alias)
        {
            var acutalWebServerUri = new Uri(applicationServerUri.ToString().ToUpper().Replace("localhost".ToUpper(), Environment.MachineName));
            var environmentConnection = new ServerProxy(acutalWebServerUri);
            return new Server(id, environmentConnection) { Name = alias };
        }

        #endregion

        public static string GetAppServerUriFromConnectionString(string connectionstring)
        {
            if (string.IsNullOrWhiteSpace(connectionstring))
            {
                return string.Empty;
            }

            const string toLookFor = "AppServerUri";
            var appServerUriIdx = connectionstring.IndexOf(toLookFor, StringComparison.Ordinal);
            var length = toLookFor.Length;
            var substring = connectionstring.Substring(appServerUriIdx + length + 1);
            var indexofDelimiter = substring.IndexOf(';');
            var uri = substring.Substring(0, indexofDelimiter);
            return uri;
        }
        #endregion

        #region Implementation of IDisposable

        ~ServerRepository()
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
            if (!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
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

    public class EnvironmentEditedArgs : EventArgs, IEnvironmentEditedArgs
    {
        public IServer Environment { get; set; }
        public bool IsConnected { get; set; }

        public EnvironmentEditedArgs(IServer environment, bool isConnected)
        {
            Environment = environment;
            IsConnected = isConnected;
        }
    }
}
