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
using System.IO;
using Dev2.Common;
using Dev2.PathOperations;

namespace Dev2.Simulation
{
    /// <summary>
    ///     A repository for <see cref="ISimulationResult" />'s.
    /// </summary>
    public class SimulationRepository : FileRepository<ISimulationKey, ISimulationResult>, ISimulationRepository
    {
        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        private static volatile SimulationRepository _instance;
        private static readonly object SyncRoot = new Object();

        /// <summary>
        ///     Gets the repository instance.
        /// </summary>
        public static SimulationRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new SimulationRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Initialization

        // Prevent instantiation
        private SimulationRepository()
            : base(Path.Combine(EnvironmentVariables.WorkspacePath, "Simulations"), "usi")
        {
        }

        #endregion
    }
}