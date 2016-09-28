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
        private bool _hasOptionsForValue;
        private List<string> _optionsForValue;
        private string _from;
        private string _to;

        public ServiceTestOutput(string variable, string value, string from, string to)
        {
            if(variable == null)
                throw new ArgumentNullException(nameof(variable));
            Variable = variable;
            Value = value;
            From = from;
            To = to;
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
        public string From
        {
            get
            {
                return _from;
            }
            set
            {
                _from = value;
                OnPropertyChanged(() => From);
            }
        }
        public string To
        {
            get
            {
                return _to;
            }
            set
            {
                _to = value;
                OnPropertyChanged(() => To);
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

        public bool HasOptionsForValue
        {
            get { return _hasOptionsForValue; }
            set
            {
                _hasOptionsForValue = value; 
                OnPropertyChanged(() => HasOptionsForValue);
            }
        }

        public List<string> OptionsForValue
        {
            get { return _optionsForValue; }
            set
            {
                _optionsForValue = value; 
                OnPropertyChanged(() => OptionsForValue);
            }
        }

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