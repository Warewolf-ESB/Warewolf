using System;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestInput: BindableBase,ITestInput
    {
        private string _variable;
        private string _value;
        private bool _emptyIsNull;

        public ServiceTestInput(string variableName, string value)
        {
            if(variableName == null)
                throw new ArgumentNullException(nameof(variableName));
            EmptyIsNull = true;
            Variable = variableName;
            Value = value;
        }

        #region Implementation of ITestInput

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
        public bool EmptyIsNull
        {
            get
            {
                return _emptyIsNull;
            }
            set
            {
                _emptyIsNull = value;               
                OnPropertyChanged(()=>EmptyIsNull);
            }
        }

        #endregion
    }
}