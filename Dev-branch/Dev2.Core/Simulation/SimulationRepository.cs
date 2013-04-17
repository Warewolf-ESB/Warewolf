using System;
using System.IO;
using Dev2.Common;
using Dev2.PathOperations;

namespace Dev2.Simulation
{
    /// <summary>
    /// A repository for <see cref="ISimulationResult"/>'s.
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
        static volatile SimulationRepository _instance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the repository instance.
        /// </summary>
        public static SimulationRepository Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
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
            : base(Path.Combine(GlobalConstants.WorkspacePath, "Simulations"), "usi")
        {
        }

        #endregion
    }
}
