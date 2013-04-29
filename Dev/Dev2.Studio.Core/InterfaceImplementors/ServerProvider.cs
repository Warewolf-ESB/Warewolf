using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Interfaces;

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
        protected ServerProvider()
        {
        }

        #endregion

        #region Load

        public List<IServer> Load()
        {
            return Load(EnvironmentRepository.Instance);
        }

        public List<IServer> Load(IEnvironmentRepository environmentRepository)
        {
            // PBI 6597 : TWR
            // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

            if(environmentRepository == null)
            {
                throw new ArgumentNullException("environmentRepository");
            }

            var environments = environmentRepository.All();

            return new List<IServer>(environments.Select(e => new ServerDTO(e)));
        }

        #endregion Methods
    }
}
