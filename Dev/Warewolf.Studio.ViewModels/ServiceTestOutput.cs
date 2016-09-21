using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestOutput : BindableBase, IServiceTestOutput
    {
        private string _variable;
        private string _value;
        private string _assertOp;
        private List<string> _assertOps;

        public ServiceTestOutput(string variable, string value)
        {
            if(variable == null)
                throw new ArgumentNullException(nameof(variable));
            Variable = variable;
            Value = value;
            AssertOps = new List<string> { "=" };
        }

        public string Variable
        {
            get
            {
                return _variable;
            }
            set
            {
                _variable = value;
                OnPropertyChanged(()=>Variable);
            }
        }
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged(() => Value);
            }
        }

        public string AssertOp
        {
            get { return _assertOp; }
            set
            {
                _assertOp = value; 
                OnPropertyChanged(() => AssertOp);
            }
        }
        public bool HasOptionsForValue { get; set; }
        public List<string> OptionsForValue { get; set; }

        public List<string> AssertOps
        {
            get { return _assertOps; }
            set
            {
                _assertOps = value; 
                OnPropertyChanged(() => AssertOps);
            }
        }
    }
}