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
using System.Linq;
using Dev2.Studio.Interfaces;


namespace Dev2.Studio.Core.InterfaceImplementors
{
    /// <summary>
    /// A provider responsible for providing an aggregated list of <see cref="IServer"/>'s.
    /// </summary>
    public class ServerProvider : IEnvironmentModelProvider
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
            return Load(ServerRepository.Instance);
        }

        public List<IServer> Load(IServerRepository serverRepository)
        {
            // PBI 6597 : TWR
            // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

            if(serverRepository == null)
            {
                throw new ArgumentNullException("serverRepository");
            }

            var environments = serverRepository.All();

            return environments.ToList();
        }

        public List<IServer> ReloadServers()
        {
            return ReloadServers(ServerRepository.Instance);
        }

        public List<IServer> ReloadServers(IServerRepository serverRepository)
        {
            // PBI 6597 : TWR
            // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

            if (serverRepository == null)
            {
                throw new ArgumentNullException("serverRepository");
            }

            var environments = serverRepository.ReloadServers();

            return environments.ToList();
        }

        #endregion Methods
    }
}
