using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestModel : BindableBase, IServiceTestModel
    {
        private string _testName;
        private string _userName;
        private bool _testPassed;
        private bool _testFailing;
        private bool _testPending;
        private bool _testInvalid;
        private string _password;
        private List<IServiceTestInput> _inputs;
        private List<IServiceTestOutput> _outputs;
        private bool _noErrorExpected;
        private bool _errorExpected;
        private bool _isNewTest;
        private bool _isTestSelected;
        private DateTime _lastRunDate;
        private bool _enabled;
        private string _runSelectedTestUrl;
        private bool _isDirty;
        private AuthenticationType _authenticationType;

        #region Implementation of IServiceTestModel

        public string TestName
        {
            get { return _testName; }
            set
            {
                _testName = value;
                OnPropertyChanged(() => TestName);
                OnPropertyChanged(() => NameForDisplay);
            }
        }

        public string NameForDisplay
        {
            get
            {
                if (IsDirty)
                {
                    return TestName + " *";
                }
                return TestName;
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(() => UserName);
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged(() => Password);
            }
        }
        public DateTime LastRunDate
        {
            get
            {
                return _lastRunDate;
            }
            set
            {
                _lastRunDate = value;
                OnPropertyChanged(() => LastRunDate);
            }
        }

        public List<IServiceTestInput> Inputs
        {
            get { return _inputs; }
            set
            {
                _inputs = value;
                OnPropertyChanged(() => Inputs);
            }
        }

        public List<IServiceTestOutput> Outputs
        {
            get { return _outputs; }
            set
            {
                _outputs = value;
                OnPropertyChanged(() => Outputs);
            }
        }
        public bool NoErrorExpected
        {
            get
            {
                return _noErrorExpected;
            }
            set
            {
                _noErrorExpected = value;
                OnPropertyChanged(() => NoErrorExpected);
            }
        }
        public bool ErrorExpected
        {
            get
            {
                return _errorExpected;
            }
            set
            {
                _errorExpected = value;
                OnPropertyChanged(() => ErrorExpected);
            }
        }

        public bool IsNewTest
        {
            get
            {
                return _isNewTest;
            }
            set
            {
                _isNewTest = value;
                OnPropertyChanged(() => IsNewTest);
            }
        }
        public bool IsTestSelected
        {
            get
            {
                return _isTestSelected;
            }
            set
            {
                _isTestSelected = value;
                OnPropertyChanged(() => IsTestSelected);
            }
        }

        public bool TestPassed
        {
            get { return _testPassed; }
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => TestPassed);
            }
        }

        public bool TestFailing
        {
            get { return _testFailing; }
            set
            {
                _testFailing = value;
                OnPropertyChanged(() => TestFailing);
            }
        }

        public bool TestInvalid
        {
            get { return _testInvalid; }
            set
            {
                _testInvalid = value;
                OnPropertyChanged(() => TestInvalid);
            }
        }

        public bool TestPending
        {
            get { return _testPending; }
            set
            {
                _testPending = value;
                OnPropertyChanged(() => TestPending);
            }
        }
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                OnPropertyChanged(() => Enabled);
            }
        }
        public string RunSelectedTestUrl
        {
            get
            {
                return _runSelectedTestUrl;
            }
            set
            {
                _runSelectedTestUrl = value;
                OnPropertyChanged(()=>RunSelectedTestUrl);
            }
        }
        public AuthenticationType AuthenticationType
        {
            get
            {
                return _authenticationType;
            }
            set
            {
                _authenticationType = value;
                OnPropertyChanged(() => AuthenticationType);
                OnPropertyChanged(() => UserAuthenticationSelected);
            }
        }
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
                OnPropertyChanged(() => IsDirty);
                OnPropertyChanged(() => NameForDisplay);
            }
        }
        public bool UserAuthenticationSelected => AuthenticationType == AuthenticationType.User;

        #endregion
    }
}