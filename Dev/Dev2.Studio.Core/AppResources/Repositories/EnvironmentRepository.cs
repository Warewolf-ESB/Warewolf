using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.DynamicServices;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Wizards.Interfaces;

namespace Dev2.Studio.Core
{
    // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

    public class EnvironmentRepository : IEnvironmentRepository
    {
        static readonly List<IEnvironmentModel> EmptyList = new List<IEnvironmentModel>();
        static readonly int DefaultWebServerPort = Int32.Parse(StringResources.Default_WebServer_Port);

        readonly object _fileLock = new Object();
        readonly object _restoreLock = new Object();
        readonly List<IEnvironmentModel> _environments;

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
        public static EnvironmentRepository Instance
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

        #region CTOR

        // Singleton instance only
        protected EnvironmentRepository()
            : this(CreateEnvironmentModel(Guid.Empty, new Uri(StringResources.Uri_Live_Environment), StringResources.DefaultEnvironmentName, DefaultWebServerPort))
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
            _environments = new List<IEnvironmentModel> { Source };
        }

        #endregion

        public event EventHandler ItemAdded;

        public IEnvironmentModel Source { get; private set; }

        public bool IsLoaded { get; set; }

        #region Clear

        public void Clear()
        {
            foreach(var environment in _environments)
            {
                environment.Disconnect();
            }
            _environments.Clear();
        }

        #endregion

        #region All/Find/FindSingle/Load

        public ICollection<IEnvironmentModel> All()
        {
            LoadInternal();
            return _environments;
        }

        public ICollection<IEnvironmentModel> Find(Expression<Func<IEnvironmentModel, bool>> expression)
        {
            LoadInternal();
            return expression == null ? EmptyList : _environments.AsQueryable().Where(expression).ToList();
        }

        public IEnvironmentModel FindSingle(Expression<Func<IEnvironmentModel, bool>> expression)
        {
            LoadInternal();
            return expression == null ? null : _environments.AsQueryable().FirstOrDefault(expression);
        }

        public void Load()
        {
            IsLoaded = false;
            LoadInternal();
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

        public void Save(IEnvironmentModel environment)
        {
            if(environment == null)
            {
                return;
            }
            AddInternal(environment);
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
            lock(_fileLock)
            {
                var path = GetEnvironmentsFilePath();

                var xml = File.Exists(path) ? XElement.Load(path) : new XElement("Environments");
                var guids = xml.Descendants("Environment").Select(id => id.Value).ToList();
                var result = new List<Guid>();
                foreach(var guidStr in guids)
                {
                    Guid guid;
                    if(Guid.TryParse(guidStr, out guid))
                    {
                        result.Add(guid);
                    }
                }
                return result;
            }
        }

        public virtual void WriteSession(IEnumerable<Guid> environmentGuids)
        {
            lock(_fileLock)
            {
                var xml = new XElement("Environments");
                if(environmentGuids != null)
                {
                    foreach(var environmentID in environmentGuids.Where(id => id != Guid.Empty))
                    {
                        xml.Add(new XElement("Environment", environmentID));
                    }
                }
                var path = GetEnvironmentsFilePath();
                xml.Save(path);
            }
        }

        #endregion

        #region Fetch

        public IEnvironmentModel Fetch(IServer server)
        {
            LoadInternal();

            IEnvironmentModel environment = null;
            if(server != null)
            {
                Guid id;
                if(!Guid.TryParse(server.ID, out id))
                {
                    id = Guid.NewGuid();
                }
                environment = _environments.FirstOrDefault(e => e.ID == id) ?? CreateEnvironmentModel(id, server.AppUri, server.Alias, server.WebUri.Port);
                server.Environment = environment;
            }

            return environment;
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

        #endregion

        #region LoadInternal

        protected virtual void LoadInternal()
        {
            lock(_restoreLock)
            {
                if(IsLoaded)
                {
                    return;
                }

                var environments = LookupEnvironments(Source);

                // Don't just clear and add, environments may be connected!!!
                foreach(var newEnv in environments.Where(newEnv => !_environments.Contains(newEnv)))
                {
                    _environments.Add(newEnv);
                }

                var toBeRemoved = _environments.Where(e => !e.Equals(Source) && !environments.Contains(e)).ToList();
                foreach(var environment in toBeRemoved)
                {
                    environment.Disconnect();
                    _environments.Remove(environment);
                }

                IsLoaded = true;
            }
        }

        #endregion

        #region Add/RemoveInternal

        protected virtual void AddInternal(IEnvironmentModel environment)
        {
            var index = _environments.IndexOf(environment);
            if(index == -1)
            {
                _environments.Add(environment);
            }
            else
            {
                _environments.RemoveAt(index);
                _environments.Insert(index, environment);
            }
            RaiseItemAdded();
        }

        protected virtual bool RemoveInternal(IEnvironmentModel environment)
        {
            var index = _environments.IndexOf(environment);
            if(index != -1)
            {
                environment.Disconnect();
                _environments.RemoveAt(index);
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
            defaultEnvironment.Connect();
            if(!defaultEnvironment.IsConnected)
            {
                return result;
            }

            var hasEnvironmentGuids = environmentGuids != null;
            var servers = hasEnvironmentGuids
                              ? ResourceRepository.FindResourcesByID(defaultEnvironment, environmentGuids, ResourceType.Source)
                              : ResourceRepository.FindSourcesByType(defaultEnvironment, enSourceType.Dev2Server);

            foreach(dynamic server in servers)
            {
                Guid id;
                string serverID = server.ID;
                if(Guid.TryParse(serverID, out id) && (!hasEnvironmentGuids || environmentGuids.Contains(serverID)))
                {
                    string displayName = server.DisplayName;
                    Uri appServerUri;
                    int webServerPort;

                    #region Parse connection string values

                    Dictionary<string, string> connectionParams = ParseConnectionString(server.ConnectionString);
                    string value;
                    if(!connectionParams.TryGetValue("AppServerUri", out value))
                    {
                        continue;
                    }
                    try
                    {
                        appServerUri = new Uri(value);
                    }
                    catch
                    {
                        continue;
                    }

                    if(!connectionParams.TryGetValue("WebServerPort", out value))
                    {
                        continue;
                    }
                    if(!int.TryParse(value, out webServerPort))
                    {
                        continue;
                    }

                    #endregion

                    var environment = CreateEnvironmentModel(
                        id, appServerUri, displayName, webServerPort,
                        defaultEnvironment.Connection.SecurityContext,
                        defaultEnvironment.Connection.EventAggregator,
                        defaultEnvironment.WizardEngine);

                    result.Add(environment);
                }
            }

            return result;
        }

        #endregion

        #region ParseConnectionString

        static Dictionary<string, string> ParseConnectionString(string s)
        {
            var values = s.Split(';');

            return values.Select(value => value.Split('=')).Where(kvp => kvp.Length > 1).ToDictionary(kvp => kvp[0], kvp => kvp[1]);
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

        static IEnvironmentModel CreateEnvironmentModel(Guid id, Uri applicationServerUri, string alias, int webServerPort)
        {
            // MEF!!!!
            var eventAggregator = ImportService.GetExportValue<IEventAggregator>();
            var securityContext = ImportService.GetExportValue<IFrameworkSecurityContext>();
            var wizardEngine = ImportService.GetExportValue<IWizardEngine>();

            return CreateEnvironmentModel(id, applicationServerUri, alias, webServerPort, securityContext, eventAggregator, wizardEngine);
        }

        static IEnvironmentModel CreateEnvironmentModel(Guid id, Uri applicationServerUri, string alias, int webServerPort,
                                                        IFrameworkSecurityContext securityContext, IEventAggregator eventAggregator, IWizardEngine wizardEngine)
        {
            var environmentConnection = new TcpConnection(securityContext, applicationServerUri, webServerPort, eventAggregator);
            return new EnvironmentModel(id, environmentConnection, wizardEngine) { Name = alias };
        }

        #endregion

        #endregion

    }
}
