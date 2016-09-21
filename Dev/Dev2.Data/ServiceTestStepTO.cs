using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
// ReSharper disable InconsistentNaming

namespace Dev2.Data
{
    public class ServiceTestStepTO:IServiceTestStep
    {
        public ServiceTestStepTO(Guid stepUniqueId, string stepActivityType, List<IServiceTestOutput> outputs, StepType stepType)
        {
            UniqueId = stepUniqueId;
            ActivityType = stepActivityType;
            Type = stepType;
            StepOutputs = outputs;
        }

        public Guid UniqueId { get; set; }
        public string ActivityType { get; set; }
        public StepType Type { get; set; }
        public List<IServiceTestOutput> StepOutputs { get; set; }
    }
}