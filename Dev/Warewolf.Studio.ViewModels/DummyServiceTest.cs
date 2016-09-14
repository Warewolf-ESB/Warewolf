using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Commands;

namespace Warewolf.Studio.ViewModels
{
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
        public List<IServiceTestInput> Inputs { get; set; }
        public List<IServiceTestOutput> Outputs { get; set; }
        public bool NoErrorExpected { get; set; }
        public bool ErrorExpected { get; set; }
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
        public Visibility LastRunDateVisibility => Visibility.Visible;
        public Visibility NeverRunStringVisibility => Visibility.Collapsed;

        public void SetItem(IServiceTestModel model)
        {
            
        }

        public bool IsTestSelected { get; set; }
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
    }

    internal interface INewServiceResource
    {
        ICommand CreateTestCommand { get; set; }
    }
}