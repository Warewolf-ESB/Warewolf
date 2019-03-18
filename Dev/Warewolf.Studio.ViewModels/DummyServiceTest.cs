#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Commands;

namespace Warewolf.Studio.ViewModels
{
    public class DummyServiceTest : IServiceTestModel, INewServiceResource
    {
        bool _isNewTest;
        ICommand _newCommand;

        public DummyServiceTest(Action<bool> createNewAction)
        {
            NameForDisplay = "'";
            NeverRunString = "Never run";
            _isNewTest = true;
            _newCommand = new DelegateCommand(() => createNewAction?.Invoke(false));
            TestSteps = new ObservableCollection<IServiceTestStep>();
        }

        #region Implementation of INewServiceResource


        public ICommand CreateTestCommand
        {
            get
            {
                return _newCommand;
            }
            set
            {
                _newCommand = value;
            }
        }

        #endregion

        #region Implementation of IServiceTestModel

        public Guid ParentId { get; set; }
        public string OldTestName { get; set; }
        public string TestName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime LastRunDate { get; set; }
        public ObservableCollection<IServiceTestInput> Inputs { get; set; }
        public ObservableCollection<IServiceTestOutput> Outputs { get; set; }
        public bool NoErrorExpected { get; set; }
        public bool ErrorExpected { get; set; }
        public string ErrorContainsText { get; set; }
        public bool IsNewTest
        {
            get => _isNewTest;
            set => _isNewTest = value;
        }
        public bool NewTest { get; set; }
        public bool IsTestRunning { get; set; }
        public string NeverRunString { get; set; }
        public bool LastRunDateVisibility { get; set; }
        public bool NeverRunStringVisibility { get; set; }
        public IList<IDebugState> DebugForTest { get; set; }
        public string DuplicateTestTooltip { get; set; }
        public ObservableCollection<IServiceTestStep> TestSteps { get; set; }
        public bool IsTestSelected { get; set; }
        public bool IsTestLoading { get; set; }
        public bool TestPassed { get; set; }
        public bool TestFailing { get; set; }
        public bool TestInvalid { get; set; }
        public bool TestPending { get; set; }
        public bool Enabled { get; set; }
        public string RunSelectedTestUrl { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string NameForDisplay { get; }
        public bool IsDirty { get; set; }
        public bool UserAuthenticationSelected { get; set; }
        public ICommand DeleteTestCommand { get; set; }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public IServiceTestStep AddDebugItemTestStep(IDebugState debugItemContent, ObservableCollection<IServiceTestOutput> serviceTestOutputs) => AddTestStep(debugItemContent.ID.ToString(), debugItemContent.DisplayName, debugItemContent.ActualType, serviceTestOutputs, StepType.Mock);
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Implementation of ICloneable
        
        public IServiceTestModel Clone() => this;
        public void SetItem(IServiceTestModel model) => throw new NotImplementedException();
        public void ResetOldTestName() => throw new NotImplementedException();
        public IServiceTestStep AddTestStep(string activityUniqueId, string activityDisplayName, string activityTypeName, ObservableCollection<IServiceTestOutput> serviceTestOutputs) => throw new NotImplementedException();
        public IServiceTestStep AddTestStep(string activityUniqueId, string activityDisplayName, string activityTypeName, ObservableCollection<IServiceTestOutput> serviceTestOutputs, StepType stepType) => throw new NotImplementedException();

        #endregion
    }

    interface INewServiceResource
    {
        ICommand CreateTestCommand { get; set; }
    }
}