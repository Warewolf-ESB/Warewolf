using System;
using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces
{
    public interface IServiceTestStep
    {
        Guid UniqueId { get; set; }
        string ActivityType { get; set; }
        StepType Type { get; set; }
        ObservableCollection<IServiceTestOutput> StepOutputs { get; set; }
        IServiceTestStep Parent { get; set; }
        ObservableCollection<IServiceTestStep> Children { get; set; }
        string StepDescription { get; set; }
        TestRunResult Result { get; set; }
    }
}