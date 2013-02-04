


using Dev2.PathOperations.Interfaces;

namespace Dev2.Simulation
{
    /// <summary>
    /// Defines the requirements for a simulation repository
    /// </summary>
    public interface ISimulationRepository : IRepository<ISimulationKey, ISimulationResult>
    {
    }
}
