using System;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;

namespace Dev2.Data
{
    public class ServiceTestStepTO : IServiceTestStep
    {
        public ServiceTestStepTO(Guid activityID, string stepActivityType, ObservableCollection<IServiceTestOutput> outputs, StepType stepType)
        {
            ActivityID = activityID;
            UniqueID = activityID;
            ActivityType = stepActivityType;
            Type = stepType;
            StepOutputs = outputs;
        }

        public ServiceTestStepTO()
        {
            StepOutputs = new ObservableCollection<IServiceTestOutput>();
            Children = new ObservableCollection<IServiceTestStep>();
        }

        private Guid _activityID;
        public Guid ActivityID 
        {
            get => _activityID == Guid.Empty ? UniqueID : _activityID;
            set => _activityID = value;
        }
        public Guid UniqueID { get; set; }
        public string ActivityType { get; set; }
        public StepType Type { get; set; }
        public ObservableCollection<IServiceTestOutput> StepOutputs { get; set; }
        public IServiceTestStep Parent { get; set; }
        public ObservableCollection<IServiceTestStep> Children { get; set; }
        public string StepDescription { get; set; }
        public TestRunResult Result { get; set; }
        
        public bool MockSelected 
        {
            get { return Type is StepType.Mock;  }
            set { }
        }
        public bool AssertSelected 
        { 
            get { return Type is StepType.Assert; }
            set { }
        }
        public void AddNewOutput(string varName) { }

        T IServiceTestStep.As<T>() => this as T;
    }
}