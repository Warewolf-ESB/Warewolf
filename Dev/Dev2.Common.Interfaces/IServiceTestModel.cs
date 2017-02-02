using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;

// ReSharper disable UnusedMemberInSuper.Global

namespace Dev2.Common.Interfaces
{
    public interface IServiceTestModel : INotifyPropertyChanged
    {
        Guid ParentId { get; set; }
        string OldTestName { get; set; }
        string TestName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        DateTime LastRunDate { get; set; }
        ObservableCollection<IServiceTestInput> Inputs { get; set; }
        ObservableCollection<IServiceTestOutput> Outputs { get; set; }
        bool NoErrorExpected { get; set; }
        bool ErrorExpected { get; set; }
        string ErrorContainsText { get; set; }
        bool IsNewTest { get; set; }
        bool IsTestSelected { get; set; }
        bool IsTestLoading { get; set; }
        bool TestPassed { get; set; }
        bool TestFailing { get; set; }
        bool TestInvalid { get; set; }
        bool TestPending { get; set; }
        bool Enabled { get; set; }
        string RunSelectedTestUrl { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string NameForDisplay { get; }
        bool IsDirty { get; }
        bool UserAuthenticationSelected { get; }
        bool NewTest { get; set; }
        bool IsTestRunning { get; set; }
        string NeverRunString { get; set; }
        bool LastRunDateVisibility { get; set; }
        bool NeverRunStringVisibility { get; set; }
        IList<IDebugState> DebugForTest { get; set; }
        string DuplicateTestTooltip { get; set; }
        ObservableCollection<IServiceTestStep> TestSteps { get; set; }
        IServiceTestStep SelectedTestStep { get; set; }

        void SetItem(IServiceTestModel model);
        IServiceTestModel Clone();
        IServiceTestStep AddTestStep(string activityUniqueId, string activityDisplayName, string activityTypeName, ObservableCollection<IServiceTestOutput> serviceTestOutputs, StepType stepType = StepType.Assert);
    }

    public interface IServiceTestInput
    {
        string Variable { get; set; }
        string Value { get; set; }
        bool EmptyIsNull { get; set; }
    }

    public interface IServiceTestOutput
    {
        string Variable { get; set; }
        string Value { get; set; }
        string From { get; set; }
        string To { get; set; }
        [DefaultValue("=")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        string AssertOp { get; set; }
        bool HasOptionsForValue { get; set; }
        List<string> OptionsForValue { get; set; }
        TestRunResult Result { get; set; }
    }
}