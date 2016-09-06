using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class TestModel : BindableBase, ITestModel
    {
        private string _testName;
        private string _userName;
        private bool _testPassed;
        private bool _testFailing;
        private bool _testPending;
        private bool _testInvalid;

        #region Implementation of ITestModel

        public string TestName
        {
            get { return _testName; }
            set
            {
                _testName = value;
                OnPropertyChanged(()=>TestName);
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(()=>UserName);
            }
        }

        public string Password { get; set; }
        public DateTime LastRunDate { get; set; }
        public List<ITestInput> Inputs { get; set; }
        public List<ITestOutput> Outputs { get; set; }
        public bool Error { get; set; }
        public bool IsNewTest { get; set; }
        public bool IsTestSelected { get; set; }

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

        #endregion
    }
}