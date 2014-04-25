using System;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.PathOperations.Interfaces;

namespace Dev2.Simulation
{
    /// <summary>
    /// Defines the requirements for a simulation result
    /// </summary>
    public interface ISimulationResult : IRepositoryItem<ISimulationKey>, IEquatable<ISimulationResult>
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        IBinaryDataList Value { get; set; }
    }
}
