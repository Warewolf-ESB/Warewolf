
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.InterfaceImplementors
{
    /// <summary>
    /// A provider responsible for providing an aggregated list of <see cref="IEnvironmentModel"/>'s.
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

        public List<IEnvironmentModel> Load()
        {
            return Load(EnvironmentRepository.Instance);
        }

        public List<IEnvironmentModel> Load(IEnvironmentRepository environmentRepository)
        {
            // PBI 6597 : TWR
            // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

            if(environmentRepository == null)
            {
                throw new ArgumentNullException("environmentRepository");
            }

            var environments = environmentRepository.All();

            return environments.ToList();
        }

        #endregion Methods
    }
}
