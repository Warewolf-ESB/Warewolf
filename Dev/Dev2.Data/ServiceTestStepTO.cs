using System;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
// ReSharper disable InconsistentNaming

namespace Dev2.Data
{
    public class ServiceTestStepTO:IServiceTestStep
    {
        public ServiceTestStepTO(Guid stepUniqueId, string stepActivityType, ObservableCollection<IServiceTestOutput> outputs, StepType stepType)
        {
            UniqueId = stepUniqueId;
            ActivityType = stepActivityType;
            Type = stepType;
            StepOutputs = outputs;
        }

        public ServiceTestStepTO()
        {
            StepOutputs = new ObservableCollection<IServiceTestOutput>();
            Children = new ObservableCollection<IServiceTestStep>();
        }

        public Guid UniqueId { get; set; }
        public string ActivityType { get; set; }
        public StepType Type { get; set; }
        public ObservableCollection<IServiceTestOutput> StepOutputs { get; set; }
        public IServiceTestStep Parent { get; set; }
        public ObservableCollection<IServiceTestStep> Children { get; set; }
        public string StepDescription { get; set; }
        public TestRunResult Result { get; set; }
    }
}