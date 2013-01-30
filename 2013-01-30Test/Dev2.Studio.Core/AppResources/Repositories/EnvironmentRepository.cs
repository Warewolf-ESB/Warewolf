using Dev2;
using Dev2.DynamicServices;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Dev2.Studio.Core
{
    [Export(typeof(IFrameworkRepository<IEnvironmentModel>))]
    public class EnvironmentRepository : IFrameworkRepository<IEnvironmentModel>
    {
        static readonly int DefaultWebServerPort = Int32.Parse(StringResources.Default_WebServer_Port);

        #region DefaultEnvironment

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile IEnvironmentModel _defaultEnvironment;
        static readonly object SyncRoot = new Object();

        public static IEnvironmentModel DefaultEnvironment
        {
            get
            {
                if(_defaultEnvironment == null)
                {
                    lock(SyncRoot)
                    {
                        if(_defaultEnvironment == null)
                        {
                            _defaultEnvironment = EnvironmentModelFactory.CreateEnvironmentModel(
                                Guid.Empty,
                                new Uri(StringResources.Uri_Live_Environment),
                                StringResources.DefaultEnvironmentName,
                                DefaultWebServerPort);
                        }
                    }
                }
                return _defaultEnvironment;
            }
        }

        #endregion

        private readonly List<IEnvironmentModel> _environments;

        [Import]
        public IFrameworkSecurityContext SecurityContext { get; set; }

        public EnvironmentRepository()
            : this(new[] { DefaultEnvironment })
        {
        }

        public EnvironmentRepository(IEnumerable<IEnvironmentModel> environments)
        {
            _environments = environments == null
                ? new List<IEnvironmentModel>()
                : new List<IEnvironmentModel>(environments);
        }

        private void RestoreEnvironments()
        {
            var environmentGuids = ReadFile();
            var environments = LookupEnvironments(DefaultEnvironment, environmentGuids);
            _environments.AddRange(environments);
        }

        public ICollection<IEnvironmentModel> All()
        {
            return _environments;
        }

        public ICollection<IEnvironmentModel> Find(System.Linq.Expressions.Expression<Func<IEnvironmentModel, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public IEnvironmentModel FindSingle(System.Linq.Expressions.Expression<Func<IEnvironmentModel, bool>> expression)
        {
            return _environments.AsQueryable().FirstOrDefault(expression);
        }

        public event EventHandler ItemAdded;

        protected void OnItemAdded()
        {
            if (ItemAdded != null)
            {
                ItemAdded(this, new System.EventArgs());
            }
        }

        public void Load()
        {
            RestoreEnvironments();
        }

        #region Save

        public void Save(ICollection<IEnvironmentModel> instanceObjs)
        {
            throw new NotImplementedException();
        }

        public void Save(IEnvironmentModel instanceObj)
        {
            var index = _environments.IndexOf(instanceObj);
            if(index == -1)
            {
                _environments.Add(instanceObj);
            }
            else
            {
                _environments.RemoveAt(index);
                _environments.Insert(index, instanceObj);
            }
            WriteFile();
        }

        #endregion

        #region Remove

        public void Remove(ICollection<IEnvironmentModel> instanceObjs)
        {
            foreach(var environmentModel in instanceObjs)
            {
                Remove(environmentModel);
            }
        }

        public void Remove(IEnvironmentModel instanceObj)
        {
            var index = _environments.IndexOf(instanceObj);
            if(index != -1)
            {
                _environments.RemoveAt(index);
                WriteFile();
                //
                // NOTE: This should NEVER remove the environment source from the server 
                //       as this is done by the user via the explorer
                //
            }
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

        #region Read/WriteFile

        public IList<string> ReadFile()
        {
            lock(SyncRoot)
            {
                var path = GetEnvironmentsFilePath();

                var xml = File.Exists(path) ? XElement.Load(path) : new XElement("Environments");
                return xml.Descendants("Environment").Select(id => id.Value).ToList();
            }
        }

        public void WriteFile()
        {
            lock(SyncRoot)
            {
                var xml = new XElement("Environments");
                foreach(var environment in _environments.Where(e => e.ID != Guid.Empty))
                {
                    xml.Add(new XElement("Environment", environment.ID));
                }

                var path = GetEnvironmentsFilePath();
                xml.Save(path);
            }
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

                    var environment = EnvironmentModelFactory.CreateEnvironmentModel(id, appServerUri, displayName, webServerPort);
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


        #endregion

    }
}
