using System;
using System.Runtime.Serialization;

namespace Dev2.Simulation
{
    public interface ISimulationKey : ISerializable, IEquatable<ISimulationKey>
    {
        string WorkflowID { get; set; }
        string ActivityID { get; set; }
        string ScenarioID { get; set; }
    }
}