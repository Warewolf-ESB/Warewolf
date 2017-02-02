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
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace Warewolf.Studio.ViewModels
{
    [ExcludeFromCodeCoverage] //Excluded as this is purely for visual effect
    public class DummyServiceTest : IServiceTestModel, INewServiceResource
    {
#pragma warning disable 649
        private readonly bool _isNewTest;
        private ICommand _newCommand;
#pragma warning restore 649

        public DummyServiceTest(Action createNewAction)
        {
            NameForDisplay = "'";
            NeverRunString = "Never run";
            _isNewTest = true;
            _newCommand = new DelegateCommand(createNewAction);
            TestSteps = new ObservableCollection<IServiceTestStep>();
        }

        #region Implementation of INewServiceResource

        // ReSharper disable once ConvertToAutoProperty
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
            get
            {
                return _isNewTest;
            }
            set
            {
                
            }
        }
        public bool NewTest { get; set; }
        public bool IsTestRunning { get; set; }
        public string NeverRunString { get; set; }
        public bool LastRunDateVisibility { get; set; }
        public bool NeverRunStringVisibility { get; set; }
        public IList<IDebugState> DebugForTest { get; set; }

        public string DuplicateTestTooltip { get; set; }

        public ObservableCollection<IServiceTestStep> TestSteps { get; set; }
        public IServiceTestStep SelectedTestStep { get; set; }

        public void SetItem(IServiceTestModel model)
        {
            
        }

        public IServiceTestStep AddTestStep(string activityUniqueId, string activityDisplayName, string activityTypeName, ObservableCollection<IServiceTestOutput> serviceTestOutputs, StepType stepType = StepType.Mock)
        {
            return null;
        }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #region Implementation of ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public IServiceTestModel Clone()
        {
            return this;
        }

        #endregion
    }

    internal interface INewServiceResource
    {
        ICommand CreateTestCommand { get; set; }
    }
}