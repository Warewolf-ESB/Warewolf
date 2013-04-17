using Dev2.Studio.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Studio.Core.InterfaceImplementors
{
    /// <summary>
    /// A provider responsible for providing an aggregated list of <see cref="IServer"/>'s.
    /// </summary>
    public class ServerProvider : IServerProvider
    {

        #region Singleton Instance

        static volatile ServerProvider _instance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the repository instance.
        /// </summary>
        public static ServerProvider Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
                        {
                            _instance = new ServerProvider();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region CTOR

        // Singleton instance only
        ServerProvider()
        {
        }

        #endregion


        #region Load

        /// <summary>
        /// Loads the a list of currently available servers from the <see cref="EnvironmentRepository.DefaultEnvironment"/>.
        /// </summary>
        public List<IServer> Load()
        {
            return Load(EnvironmentRepository.DefaultEnvironment);
        }

        /// <summary>
        /// Loads the a list of servers from the given environment.
        /// </summary>
        /// <param name="targetEnvironment">The target environment to be queried</param>
        /// <param name="addTargetEnvironment">if set to <c>true</c> adds the <paramref name="targetEnvironment"/> to the list returned.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">targetEnvironment</exception>
        public static List<IServer> Load(IEnvironmentModel targetEnvironment, bool addTargetEnvironment = true)
        {
            if(targetEnvironment == null)
            {
                throw new ArgumentNullException("targetEnvironment");
            }

            // PBI 6597: TWR
            var environments = EnvironmentRepository.LookupEnvironments(targetEnvironment);

            if(addTargetEnvironment)
            {
                environments.Insert(0, targetEnvironment);
            }

            return new List<IServer>(environments.Select(e => new ServerDTO(e)));
        }

        #endregion Methods
    }
}
