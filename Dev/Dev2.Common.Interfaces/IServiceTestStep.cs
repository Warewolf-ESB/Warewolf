using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Dev2.Common.Interfaces
{
    [JsonObject(IsReference = true)]
    public interface IServiceTestStep
    {
        Guid UniqueId { get; set; }
        string ActivityType { get; set; }
        StepType Type { get; set; }
        ObservableCollection<IServiceTestOutput> StepOutputs { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        IServiceTestStep Parent { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        ObservableCollection<IServiceTestStep> Children { get; set; }
        string StepDescription { get; set; }
        TestRunResult Result { get; set; }
    }
}