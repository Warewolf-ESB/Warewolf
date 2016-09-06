using System;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceServiceTestOutput : BindableBase, IServiceTestOutput
    {
        private string _variable;
        private string _value;

        public ServiceServiceTestOutput(string variable, string value)
        {
            if(variable == null)
                throw new ArgumentNullException(nameof(variable));
            Variable = variable;
            Value = value;
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
    }
}