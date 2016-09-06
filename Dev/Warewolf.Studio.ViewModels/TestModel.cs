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
        public List<ITestOutput> OutPuts { get; set; }
        public bool Error { get; set; }
        public bool IsNewTest { get; set; }
        public bool IsTestSelected { get; set; }

        #endregion
    }
}